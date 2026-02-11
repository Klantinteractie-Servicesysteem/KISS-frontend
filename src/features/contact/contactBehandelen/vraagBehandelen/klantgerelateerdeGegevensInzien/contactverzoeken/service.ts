import { throwIfNotOk, parseJson } from "@/services";
import {
  DigitaleAdressenExpand,
  enrichBetrokkeneWithDigitaleAdressen,
  enrichBetrokkeneWithKlantContact,
  enrichInterneTakenWithActoren,
  filterOutContactmomenten,
  KlantContactExpand,
  searchDigitaleAdressen,
  type Betrokkene,
  type DigitaalAdresExpandedApiViewModel,
} from "@/services/openklant2";
import { enrichContactverzoekObjectWithContactmoment } from "@/services/openklant1";
import type { ContactverzoekOverzichtItem } from "./types";
import {
  registryVersions,
  type Systeem,
} from "@/services/environment/fetch-systemen";
import { fetchWithSysteemId } from "@/services/fetch-with-systeem-id";
import {
  enrichContactmomentWithZaaknummer,
  enrichOnderwerpObjectenWithZaaknummers,
} from "@/features/contact/shared";
import {
  mapKlantcontactToContactverzoekOverzichtItem,
  mapObjectToContactverzoekOverzichtItem,
} from "../service";

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
        .then((x) =>
          mapKlantcontactToContactverzoekOverzichtItem(x, systeem.identifier),
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
