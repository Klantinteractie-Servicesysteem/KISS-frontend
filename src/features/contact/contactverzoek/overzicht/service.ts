import { fetchLoggedIn, throwIfNotOk, parseJson } from "@/services";
import {
  DigitaleAdressenExpand,
  enrichBetrokkeneWithDigitaleAdressen,
  enrichBetrokkeneWithKlantContact,
  enrichInterneTakenWithActoren,
  fetchBetrokkenen,
  filterOutContactmomenten,
  findKlantByIdentifier,
  KlantContactExpand,
  searchDigitaleAdressen,
  type Betrokkene,
  type BetrokkeneMetKlantContact,
  type DigitaalAdresExpandedApiViewModel,
} from "@/services/openklant2";
import {
  enrichContactverzoekObjectWithContactmoment,
  fetchKlantByIdentifierOpenKlant1,
} from "@/services/openklant1";
import type { ContactverzoekOverzichtItem } from "./types";
import type { ContactmomentDetails } from "../../contactmoment";
import { fullName } from "@/helpers/string";
import type { ContactmomentViewModel, KlantIdentificator } from "../../types";
import {
  registryVersions,
  type Systeem,
} from "@/services/environment/fetch-systemen";
import { fetchInternetakenByKlantIdFromObjecten } from "@/services/internetaak/service";
import {
  enrichContactmomentWithZaaknummer,
  enrichOnderwerpObjectenWithZaaknummers,
  getIdentificatorForOk1And2,
} from "../../shared";

function searchRecursive(urlStr: string, page = 1): Promise<any[]> {
  const url = new URL(urlStr);
  url.searchParams.set("page", page.toString());

  return fetchLoggedIn(url)
    .then(throwIfNotOk)
    .then(parseJson)
    .then(async (j) => {
      if (!Array.isArray(j?.results)) {
        throw new Error("Expected array: " + JSON.stringify(j));
      }

      if (!j.next) return j.results;
      const nextResults = await searchRecursive(urlStr, page + 1);
      return [...j.results, ...nextResults];
    });
}

async function searchOk2Recursive(
  adres: string,
  page = 1,
): Promise<DigitaalAdresExpandedApiViewModel[]> {
  const paginated = await searchDigitaleAdressen({
    adres,
    page,
    expand: [DigitaleAdressenExpand.verstrektDoorBetrokkene],
  });
  if (!paginated.next) return paginated.page;
  return [...paginated.page, ...(await searchOk2Recursive(adres, page + 1))];
}

export async function search(
  systeemId: string,
  query: string,
  gebruikKlantInteractiesApi: boolean,
): Promise<ContactverzoekOverzichtItem[]> {
  // OK2
  if (gebruikKlantInteractiesApi) {
    const adressen = await searchOk2Recursive(query);

    const betrokkenen = adressen
      .map((x) => x?._expand?.verstrektDoorBetrokkene)
      .filter(Boolean) as Betrokkene[];

    const uniqueBetrokkenen = new Map<string, Betrokkene>(
      betrokkenen.map((betrokkene) => [betrokkene.uuid, betrokkene]),
    );

    return enrichBetrokkeneWithKlantContact(
      systeemId,
      [...uniqueBetrokkenen.values()],
      [
        KlantContactExpand.leiddeTotInterneTaken,
        KlantContactExpand.gingOverOnderwerpobjecten,
      ],
    )
      .then(filterOutContactmomenten)
      .then((page) => enrichBetrokkeneWithDigitaleAdressen(systeemId, page))
      .then((page) => enrichInterneTakenWithActoren(systeemId, page))
      .then((page) =>
        Promise.all(
          page.map(async (item) => ({
            ...item,
            zaaknummers: await enrichOnderwerpObjectenWithZaaknummers(
              systeemId,
              item.klantContact._expand.gingOverOnderwerpobjecten || [],
            ),
          })),
        ),
      )
      .then((page) =>
        mapKlantcontactToContactverzoekOverzichtItem(systeemId, page),
      )
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

    return searchRecursive(url.toString())
      .then((x) =>
        Promise.all(
          x.map((obj) =>
            enrichContactverzoekObjectWithContactmoment(systeemId, obj)
              .then(async (cm) => ({
                ...cm,
                contactmoment: await enrichContactmomentWithZaaknummer(
                  systeemId,
                  cm.contactmoment,
                ),
              }))
              .then(mapObjectToContactverzoekOverzichtItem),
          ),
        ),
      )

      .then(filterOutGeauthenticeerdeContactverzoeken);
  }
}

function filterOutGeauthenticeerdeContactverzoeken(
  value: ContactverzoekOverzichtItem[],
) {
  return value.filter((x) => !x.betrokkene?.isGeauthenticeerd);
}

function mapKlantcontactToContactverzoekOverzichtItem(
  systeemId: string,
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
  id: KlantIdentificator,
  systemen: Systeem[],
): Promise<ContactverzoekOverzichtItem[]> {
  const klantidentificators = getIdentificatorForOk1And2(id);

  const promises = systemen.map((systeem) => {
    if (systeem.registryVersion === registryVersions.ok1) {
      if (!klantidentificators.ok1) return [];
      return fetchKlantByIdentifierOpenKlant1(
        systeem.identifier,
        klantidentificators.ok1,
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
    if (!klantidentificators.ok2) return [];
    return findKlantByIdentifier(
      systeem.identifier,
      klantidentificators.ok2,
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
              .then((page) =>
                mapKlantcontactToContactverzoekOverzichtItem(
                  systeem.identifier,
                  page,
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
