import { throwIfNotOk } from "@/services";
import { fetchLoggedIn } from "@/services";

import {
  enrichBetrokkeneWithDigitaleAdressen,
  enrichBetrokkeneWithKlantContact,
  enrichInterneTakenWithActoren,
  fetchBetrokkenen,
  fetchKlantByKlantIdentificatorOk2,
  filterOutContactmomenten,
  KlantContactExpand,
  type BetrokkeneMetKlantContact,
  type ExpandedKlantContactApiViewmodel,
} from "@/services/openklant2";

import type { ContactmomentViewModel } from "../../../types";
import {
  enrichContactmomentWithZaaknummer,
  enrichOnderwerpObjectenWithZaaknummers,
} from "@/features/contact/shared";
import {
  type Systeem,
  registryVersions,
} from "@/services/environment/fetch-systemen";
import { fetchInternetakenByKlantIdFromObjecten } from "@/services/internetaak/service";
import type { KlantIdentificator } from "@/services/openklant/types";
import {
  fetchKlantByKlantIdentificatorOk1,
  enrichContactverzoekObjectWithContactmoment,
} from "@/services/openklant1";
import type { ContactverzoekOverzichtItem } from "./contactverzoeken/types";
import { fullName } from "@/helpers/string";

export interface ContactmomentDetails {
  id: string;
  startdatum: string;
  einddatum: string;
  gespreksresultaat?: string;
  vraag?: string;
  specifiekeVraag?: string;
  emailadresKcm?: string;
  verantwoordelijkeAfdeling?: string;
}

const contactmomentDetails = "/api/contactmomentdetails";

const fetchContactmomentDetails = (
  url: string,
): Promise<ContactmomentDetails | null> =>
  fetchLoggedIn(
    `${contactmomentDetails}?${new URLSearchParams({ id: url })}`,
  ).then((r) => (r.status === 404 ? null : throwIfNotOk(r).json()));

//wordt alleen gebruikt bij zoeken. daarheen verhiozen
export function mapKlantContactToContactmomentViewModel(
  systeemId: string,
  klantContact: ExpandedKlantContactApiViewmodel,
  zaaknummers: string[],
) {
  const medewerker = klantContact.hadBetrokkenActoren?.find(
    (x) => x.soortActor === "medewerker",
  );
  const vm: ContactmomentViewModel = {
    url: klantContact.url,
    registratiedatum: klantContact.plaatsgevondenOp,
    kanaal: klantContact?.kanaal,
    tekst: klantContact?.inhoud,
    zaaknummers,
    medewerkerIdentificatie: {
      identificatie: medewerker?.actoridentificator?.objectId || "",
      voorletters: "",
      achternaam: medewerker?.naam || "",
      voorvoegselAchternaam: "",
    },
  };
  return vm;
}

//wordt alleen gebruikt bij zoeken. daarheen verhiozen
export async function enrichContactmomentWithDetails(
  cm: ContactmomentViewModel,
): Promise<ContactmomentViewModel & Partial<ContactmomentDetails>> {
  const details = await fetchContactmomentDetails(cm.url);
  return {
    ...cm,
    ...(details || {}),
  };
}

//////////////////////////////////////

export function mapKlantcontactToContactverzoekOverzichtItem(
  betrokkeneMetKlantcontact: (BetrokkeneMetKlantContact & {
    zaaknummers: string[];
  })[],
  systeemId: string,
): ContactverzoekOverzichtItem[] {
  return betrokkeneMetKlantcontact.map(
    ({
      klantContact,
      contactnaam,
      organisatienaam,
      expandedDigitaleAdressen,
      wasPartij,
      zaaknummers,
    }) => {
      const internetaak = klantContact._expand?.leiddeTotInterneTaken?.[0];

      if (!internetaak) {
        throw new Error("");
      }

      return {
        uuid: internetaak.uuid,
        url: internetaak.url,
        onderwerp: klantContact.onderwerp,
        toelichtingBijContactmoment: klantContact.inhoud,
        status: internetaak.status,
        registratiedatum: klantContact.plaatsgevondenOp,
        vraag: klantContact.onderwerp,
        aangemaaktDoor: klantContact.hadBetrokkenActoren?.[0]?.naam || "",
        behandelaar: internetaak?.actor?.naam,
        toelichtingVoorCollega: internetaak.toelichting,
        betrokkene: {
          persoonsnaam: contactnaam,
          digitaleAdressen: expandedDigitaleAdressen || [],
          isGeauthenticeerd: !!wasPartij,
          organisatie: organisatienaam,
        },
        kanaal: klantContact.kanaal,
        zaaknummers,
        systeemId: systeemId,
      } satisfies ContactverzoekOverzichtItem;
    },
  );
}

export function mapObjectToContactverzoekOverzichtItem({
  contactverzoekObject,
  contactmoment,
  details,
}: {
  contactverzoekObject: any;
  contactmoment: ContactmomentViewModel | null;
  details: ContactmomentDetails | null;
}): ContactverzoekOverzichtItem {
  const getVraag = (cd: ContactmomentDetails | null) => {
    const { vraag, specifiekeVraag } = cd || {};
    if (!vraag) return specifiekeVraag;
    if (!specifiekeVraag) return vraag;
    return `${vraag} (${specifiekeVraag})`;
  };
  const vraag = getVraag(details) || "";
  const record = contactverzoekObject.record;
  const data = record.data;

  return {
    uuid: contactverzoekObject.uuid,
    url: contactverzoekObject.url,
    onderwerp: vraag,
    toelichtingBijContactmoment: contactmoment?.tekst || "",
    status: data.status || "onbekend",
    registratiedatum: data.registratiedatum,
    vraag,
    toelichtingVoorCollega: data.toelichting || "",
    behandelaar: data.actor?.naam || "",
    betrokkene: {
      isGeauthenticeerd: !!data.betrokkene?.klant,
      persoonsnaam: data.betrokkene?.persoonsnaam || {},
      digitaleAdressen: data.betrokkene?.digitaleAdressen || [],
    },
    aangemaaktDoor: fullName(contactmoment?.medewerkerIdentificatie),
    kanaal: contactmoment?.kanaal || "",
    zaaknummers: contactmoment?.zaaknummers || [],
  } satisfies ContactverzoekOverzichtItem;
}

export async function fetchContactverzoekenByKlantIdentificator(
  klantIdentificator: KlantIdentificator,
  systemen: Systeem[],
): Promise<ContactverzoekOverzichtItem[]> {
  const promises = systemen.map((systeem) => {
    if (systeem.registryVersion === registryVersions.ok1) {
      return fetchKlantByKlantIdentificatorOk1(
        systeem.identifier,
        klantIdentificator,
      )
        .then((klant) =>
          !klant?.url
            ? []
            : fetchInternetakenByKlantIdFromObjecten({
                systeemId: systeem.identifier,
                klantUrl: klant.url,
              }).then(({ page }) => page),
        )
        .then(async (page) => {
          const result = [];
          for (const obj of page) {
            result.push(
              await enrichContactverzoekObjectWithContactmoment(
                systeem.identifier,
                obj,
              )
                .then(async ({ contactmoment, ...item }) => ({
                  ...item,
                  contactmoment: await enrichContactmomentWithZaaknummer(
                    systeem.identifier,
                    contactmoment,
                  ),
                }))
                .then(mapObjectToContactverzoekOverzichtItem),
            );
          }
          return result;
        });
    }

    return fetchKlantByKlantIdentificatorOk2(
      systeem.identifier,
      klantIdentificator,
    ).then((klant) =>
      !klant?.url
        ? []
        : fetchBetrokkenen({
            systeemId: systeem.identifier,
            pageSize: "100",
            wasPartij__url: klant.url,
          }).then(({ page }) =>
            enrichBetrokkeneWithKlantContact(systeem.identifier, page, [
              KlantContactExpand.leiddeTotInterneTaken,
              KlantContactExpand.gingOverOnderwerpobjecten,
            ])
              .then(filterOutContactmomenten)
              .then((page) =>
                enrichBetrokkeneWithDigitaleAdressen(systeem.identifier, page),
              )
              .then((page) =>
                enrichInterneTakenWithActoren(systeem.identifier, page),
              )
              .then((page) =>
                Promise.all(
                  page.map(async (item) => ({
                    ...item,
                    zaaknummers: await enrichOnderwerpObjectenWithZaaknummers(
                      systeem.identifier,
                      item.klantContact._expand.gingOverOnderwerpobjecten || [],
                    ),
                  })),
                ),
              )
              .then((x) =>
                mapKlantcontactToContactverzoekOverzichtItem(
                  x,
                  systeem.identifier,
                ),
              ),
          ),
    );
  });
  return Promise.all(promises).then((all) =>
    all
      .flat()
      .sort(
        (a, b) =>
          new Date(b.registratiedatum).valueOf() -
          new Date(a.registratiedatum).valueOf(),
      ),
  );
}
