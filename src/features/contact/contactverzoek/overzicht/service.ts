import { throwIfNotOk, parseJson } from "@/services";
import {
  DigitaleAdressenExpand,
  enrichBetrokkeneWithDigitaleAdressen,
  enrichBetrokkeneWithKlantContact,
  enrichInterneTakenWithActoren,
  fetchBetrokkenen,
  fetchKlantByKlantIdentificatorOk2,
  filterOutContactmomenten,
  KlantContactExpand,
  searchDigitaleAdressen,
  type Betrokkene,
  type BetrokkeneMetKlantContact,
  type DigitaalAdresExpandedApiViewModel,
} from "@/services/openklant2";
import {
  enrichContactverzoekObjectWithContactmoment,
  fetchKlantByKlantIdentificatorOk1,
} from "@/services/openklant1";
import type { ContactverzoekOverzichtItem } from "./types";
import type { ContactmomentDetails } from "../../contactmoment";
import { fullName } from "@/helpers/string";
import type { ContactmomentViewModel } from "../../types";
import {
  registryVersions,
  type Systeem,
} from "@/services/environment/fetch-systemen";
import { fetchInternetakenByKlantIdFromObjecten } from "@/services/internetaak/service";
import {
  enrichContactmomentWithZaaknummer,
  enrichOnderwerpObjectenWithZaaknummers,
} from "../../shared";
import { fetchWithSysteemId } from "@/services/fetch-with-systeem-id";
import type { KlantIdentificator } from "@/services/openklant/types";

function searchRecursive(
  systeemId: string,
  urlStr: string,
  page = 1,
): Promise<any[]> {
  const url = new URL(urlStr);
  url.searchParams.set("page", page.toString());

  return fetchWithSysteemId(systeemId, url.toString())
    .then(throwIfNotOk)
    .then(parseJson)
    .then(async (j) => {
      if (!Array.isArray(j?.results)) {
        throw new Error("Expected array: " + JSON.stringify(j));
      }

      if (!j.next) return j.results;
      const nextResults = await searchRecursive(systeemId, urlStr, page + 1);
      return [...j.results, ...nextResults];
    });
}

async function searchOk2Recursive(
  systeemId: string,
  adres: string,
  page = 1,
): Promise<DigitaalAdresExpandedApiViewModel[]> {
  const paginated = await searchDigitaleAdressen({
    systeemId,
    adres,
    page,
    expand: [DigitaleAdressenExpand.verstrektDoorBetrokkene],
  });
  if (!paginated.next) return paginated.page;
  return [
    ...paginated.page,
    ...(await searchOk2Recursive(systeemId, adres, page + 1)),
  ];
}

export async function search(
  systemen: Systeem[],
  query: string,
): Promise<ContactverzoekOverzichtItem[]> {
  const promises = systemen.map(async (systeem) => {
    // OK2
    if (systeem.registryVersion === registryVersions.ok2) {
      const adressen = await searchOk2Recursive(systeem.identifier, query);

      const betrokkenen = adressen
        .map((x) => x?._expand?.verstrektDoorBetrokkene)
        .filter(Boolean) as Betrokkene[];

      const uniqueBetrokkenen = new Map<string, Betrokkene>(
        betrokkenen.map((betrokkene) => [betrokkene.uuid, betrokkene]),
      );

      return enrichBetrokkeneWithKlantContact(
        systeem.identifier,
        [...uniqueBetrokkenen.values()],
        [
          KlantContactExpand.leiddeTotInterneTaken,
          KlantContactExpand.gingOverOnderwerpobjecten,
        ],
      )
        .then(filterOutContactmomenten)
        .then((page) =>
          enrichBetrokkeneWithDigitaleAdressen(systeem.identifier, page),
        )
        .then((page) => enrichInterneTakenWithActoren(systeem.identifier, page))
        .then(async (page) => {
          const result = [];
          for (const item of page) {
            const zaaknummers = await enrichOnderwerpObjectenWithZaaknummers(
              systeem.identifier,
              item.klantContact._expand.gingOverOnderwerpobjecten || [],
            );
            const enriched = {
              ...item,
              zaaknummers,
            };
            result.push(enriched);
          }
          return result;
        })
        .then(mapKlantcontactToContactverzoekOverzichtItem)
        .then(filterOutGeauthenticeerdeContactverzoeken);
    }

    /// OK1 heeft geen interne taak, dus gaan we naar de objecten registratie
    else {
      const url = new URL("/api/internetaak/api/v2/objects", location.origin);
      url.searchParams.set("ordering", "-record__data__registratiedatum");
      url.searchParams.set("pageSize", "10");
      url.searchParams.set(
        "data_attr",
        `betrokkene__digitaleAdressen__icontains__${query}`,
      );

      return searchRecursive(systeem.identifier, url.toString())
        .then(async (x) => {
          const items = [];
          for (const obj of x) {
            const enriched = await enrichContactverzoekObjectWithContactmoment(
              systeem.identifier,
              obj,
            )
              .then(async (cm) => ({
                ...cm,
                contactmoment: await enrichContactmomentWithZaaknummer(
                  systeem.identifier,
                  cm.contactmoment,
                ),
              }))
              .then(mapObjectToContactverzoekOverzichtItem);
            items.push(enriched);
          }
          return items;
        })

        .then(filterOutGeauthenticeerdeContactverzoeken);
    }
  });

  const all = await Promise.all(promises);
  return all
    .flat()
    .sort(
      (a, b) =>
        new Date(b.registratiedatum).valueOf() -
        new Date(a.registratiedatum).valueOf(),
    );
}

function filterOutGeauthenticeerdeContactverzoeken(
  value: ContactverzoekOverzichtItem[],
) {
  return value.filter((x) => !x.betrokkene?.isGeauthenticeerd);
}

function mapKlantcontactToContactverzoekOverzichtItem(
  betrokkeneMetKlantcontact: (BetrokkeneMetKlantContact & {
    zaaknummers: string[];
  })[],
): ContactverzoekOverzichtItem[] {
  return betrokkeneMetKlantcontact.map(
    ({
      klantContact,
      contactnaam,
      expandedDigitaleAdressen,
      wasPartij,
      zaaknummers,
    }) => {
      const internetaak = klantContact._expand?.leiddeTotInterneTaken?.[0];
      if (!internetaak) {
        throw new Error("");
      }

      return {
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
        },
        zaaknummers,
      } satisfies ContactverzoekOverzichtItem;
    },
  );
}

function mapObjectToContactverzoekOverzichtItem({
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
              .then(mapKlantcontactToContactverzoekOverzichtItem),
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
