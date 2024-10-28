import {
  ServiceResult,
  fetchLoggedIn,
  throwIfNotOk,
  parseJson,
  parsePagination,
  type PaginatedResult,
} from "@/services";
import {
  enrichBetrokkeneWithDigitaleAdressen,
  enrichBetrokkeneWithKlantContact,
  enrichInterneTakenWithActoren,
  enrichKlantcontactWithInterneTaak,
  fetchBetrokkene,
  filterOutContactmomenten,
  mapToContactverzoekViewModel,
  type ContactverzoekViewmodel,
} from "@/services/klantinteracties";
import { ref, type Ref } from "vue";
import type { Contactverzoek } from "./types";

const klantinteractiesProxyRoot = "/api/klantinteracties";
const klantinteractiesApiRoot = "/api/v1";
const klantinteractiesBaseUrl = `${klantinteractiesProxyRoot}${klantinteractiesApiRoot}`;
const klantinteractiesBetrokkenen = `${klantinteractiesBaseUrl}/betrokkenen`;
const klantinteractiesDigitaleAdressen = `${klantinteractiesBaseUrl}/digitaleadressen`;

function getSearchUrl(
 query: string,
  gebruikKlantInteractiesApi: Ref<boolean | null>
) {
  if (!query) return "";

  let url: URL;

  if (gebruikKlantInteractiesApi.value) {
    url = new URL(klantinteractiesDigitaleAdressen, location.origin);
    url.searchParams.set("adres", query);
    url.searchParams.set("expand", "verstrektDoorBetrokkene");
    url.searchParams.set("page", "1");
  } else {
    url = new URL("/api/internetaak/api/v2/objects", location.origin);
    url.searchParams.set("ordering", "-record__data__registratiedatum");
    url.searchParams.set("pageSize", "10");
    url.searchParams.set(
      "data_attrs",
      `betrokkene__digitaleAdressen__icontains__${query}`
    );
  }

  return url.toString();
}

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

      const result: any[] = [...j.results];

      if (!j.next) return result; 
      const nextResults = await searchRecursive(urlStr, page + 1);
      return [...result, ...nextResults];
    });
}

export async function search(
  query: Ref<string>,
  gebruikKlantInteractiesApi: Ref<boolean | null>
): Promise<PaginatedResult<Contactverzoek>[]> {
  const url = getSearchUrl(query.value, gebruikKlantInteractiesApi);

  if (gebruikKlantInteractiesApi.value) {
    const searchResults = await searchRecursive(url);
    const enrichedResults = await Promise.all(
      searchResults.map(async (result: any) => {
        const klantUrl = ref(result.url);
        return await getContactverzoekenByKlantUrl(klantUrl);
      })
    );
    return enrichedResults;
  } else {
    return await searchRecursive(url);
  }
}

export async function getContactverzoekenByKlantUrl(klantUrl: Ref<string>) {
  function getUrl() {
    const searchParams = new URLSearchParams();
    searchParams.set("verstrektedigitaalAdres__url", klantUrl.value);
    return `${klantinteractiesBetrokkenen}?${searchParams.toString()}`;
  }

  return fetchBetrokkene(getUrl())
    .then(enrichBetrokkeneWithKlantContact)
    .then(enrichKlantcontactWithInterneTaak)
    .then(filterOutContactmomenten)
    .then(enrichBetrokkeneWithDigitaleAdressen)
    .then(enrichInterneTakenWithActoren)
    .then(mapToContactverzoekViewModel);
}

export function useContactverzoekenByKlantId(
  id: Ref<string>,
  gebruikKlantInteractiesApi: Ref<boolean | null>,
) {
  function getUrl() {
    if (gebruikKlantInteractiesApi.value === null) {
      return "";
    }

    if (!id.value) return "";

    if (gebruikKlantInteractiesApi.value === true) {
      const searchParams = new URLSearchParams();
      searchParams.set("wasPartij__url", id.value);
      return `${klantinteractiesBetrokkenen}?${searchParams.toString()}`;
    } else {
      const url = new URL("/api/internetaak/api/v2/objects", location.origin);
      url.searchParams.set("ordering", "-record__data__registratiedatum");
      url.searchParams.set("pageSize", "10");
      url.searchParams.set(
        "data_attrs",
        `betrokkene__klant__exact__${id.value}`,
      );

      return url.toString();
    }
  }

  const fetchContactverzoeken = (
    url: string,
    gebruikKlantinteractiesApi: Ref<boolean | null>,
  ) => {
    if (gebruikKlantinteractiesApi.value) {
      return fetchBetrokkene(url)
        .then(enrichBetrokkeneWithKlantContact)
        .then(enrichKlantcontactWithInterneTaak)
        .then(filterOutContactmomenten)
        .then(enrichBetrokkeneWithDigitaleAdressen)
        .then(enrichInterneTakenWithActoren)
        .then(mapToContactverzoekViewModel);
    } else {
      return fetchLoggedIn(url)
        .then(throwIfNotOk)
        .then(parseJson)
        .then((r) => parsePagination(r, (v) => v as ContactverzoekViewmodel));
    }
  };

  return ServiceResult.fromFetcher(getUrl, (u: string) =>
    fetchContactverzoeken(u, gebruikKlantInteractiesApi),
  );
}