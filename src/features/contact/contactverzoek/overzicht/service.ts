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

// API URL's
const klantinteractiesProxyRoot = "/api/klantinteracties";
const klantinteractiesApiRoot = "/api/v1";
const klantinteractiesBaseUrl = `${klantinteractiesProxyRoot}${klantinteractiesApiRoot}`;
const klantinteractiesBetrokkenen = `${klantinteractiesBaseUrl}/betrokkenen`;
const klantinteractiesDigitaleAdressen = `${klantinteractiesBaseUrl}/digitaleadressen`;

// Functie om de juiste URL op te bouwen op basis van de API keuze
function getSearchUrl(
 query: string,
  gebruikKlantInteractiesApi: Ref<boolean | null>
) {
  if (!query) return "";

  let url: URL;

  if (gebruikKlantInteractiesApi.value) {
    // Gebruik de klantinteracties API
    url = new URL(klantinteractiesDigitaleAdressen, location.origin);
    url.searchParams.set("adres", query);
    url.searchParams.set("expand", "verstrektDoorBetrokkene");
    url.searchParams.set("page", "1");
  } else {
    // Gebruik de interne taak API
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

// Recursieve zoekfunctie voor het ophalen van alle pagina's
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

      if (!j.next) return result; // Als er geen volgende pagina is, return de resultaten
      const nextResults = await searchRecursive(urlStr, page + 1); // Haal de volgende pagina op
      return [...result, ...nextResults];
    });
}

// Functie die afhankelijk van de API keuze ofwel enriched ofwel normale zoekresultaten geeft
export async function search(
  params: Ref<string>,
  gebruikKlantInteractiesApi: Ref<boolean | null>
) : Promise<PaginatedResult<Contactverzoek>[]> {
  const url = getSearchUrl(params.value, gebruikKlantInteractiesApi);

  if (gebruikKlantInteractiesApi.value) {
    // Enriched search: zoekresultaten ophalen en verrijken met contactverzoeken
    const searchResults = await searchRecursive(url);
    const enrichedResults = await Promise.all(
      searchResults.map(async (result: any) => {
        const klantUrl = ref(result.url);
        return await getContactverzoekenByKlantUrl(klantUrl);
      })
    );
    return enrichedResults;
  } else {
    // Normale search: zoekresultaten zonder verrijking
    return await searchRecursive(url);
  }
}

// // Gebruik van de search met de juiste logica op basis van de gekozen API
// export function useSearch(
//   params: Ref<SearchParameters>,
//   gebruikKlantInteractiesApi: Ref<boolean | null>
// ) {
//   const getUrl = () => getSearchUrl(params.value, gebruikKlantInteractiesApi);

//   // Kies de juiste fetch functie afhankelijk van de API keuze
//   const fetchFunction =
//     gebruikKlantInteractiesApi.value === true ? searchAndEnrich : normalSearch;

//   return ServiceResult.fromFetcher(getUrl, fetchFunction);
// }

// Hulpfunctie om contactverzoeken op te halen voor een specifieke klant
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
  id: Ref<string>, // De ID van de klant als een Ref<string>
  gebruikKlantInteractiesApi: Ref<boolean | null>, // Of de klantinteracties API gebruikt moet worden als een Ref<boolean | null>
  urlParam: string // Een parameter voor de URL key
) {
  function getUrl() {
    if (gebruikKlantInteractiesApi.value === null) {
      return "";
    }
    if (gebruikKlantInteractiesApi.value === true) {
      const searchParams = new URLSearchParams();
      searchParams.set(urlParam, id.value); // Gebruik de meegegeven URL-parameter hier
      return `${klantinteractiesBetrokkenen}?${searchParams.toString()}`;
    } else {
      if (!id.value) return "";
      const url = new URL("/api/internetaak/api/v2/objects", location.origin);
      url.searchParams.set("ordering", "-record__data__registratiedatum");
      url.searchParams.set("pageSize", "10");
      url.searchParams.set("data_attrs", `betrokkene__klant__exact__${id.value}`);
      return url.toString();
    }
  }

  const fetchContactverzoeken = (url: string) => {
    if (gebruikKlantInteractiesApi.value) {
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

  return ServiceResult.fromFetcher(getUrl, (u: string) => fetchContactverzoeken(u));
}