import {
  ServiceResult,
  fetchLoggedIn,
  throwIfNotOk,
  parseJson,
  parsePagination,
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
} from "@/services/openklant2";
import type { Ref } from "vue";

const klantinteractiesProxyRoot = "/api/klantinteracties";
const klantinteractiesApiRoot = "/api/v1";
const klantinteractiesBaseUrl = `${klantinteractiesProxyRoot}${klantinteractiesApiRoot}`;
const klantinteractiesBetrokkenen = `${klantinteractiesBaseUrl}/betrokkenen`;

type SearchParameters = {
  query: string;
};

function getSearchUrl({ query = "" }: SearchParameters) {
  if (!query) return "";

  const url = new URL("/api/internetaak/api/v2/objects", location.origin);
  url.searchParams.set("ordering", "-record__data__registratiedatum"); //todo: is dit de correcte orderning?
  url.searchParams.set("pageSize", "10");

  url.searchParams.set(
    "data_attrs",
    `betrokkene__digitaleAdressen__icontains__${query}`,
  ); //todo: kan je in 1 call op email OF telefoonnr zoeken? anders de interface aanpassen zodat duidelijk is dat je maar naar een van beide kan zoeken (bv samenvoegen in 1 zoekveld!)

  return url.toString();
}

function searchRecursive(urlStr: string, page = 1): Promise<any[]> {
  //recursive alle pagina's ophalen.
  //set de page param van de huidige op te halen pagina
  const url = new URL(urlStr);
  url.searchParams.set("page", page.toString());

  return (
    fetchLoggedIn(url)
      .then(throwIfNotOk)
      .then(parseJson)
      //todo: parsePagination toevoegen??
      .then(async (j) => {
        if (!Array.isArray(j?.results)) {
          throw new Error("expected array: " + JSON.stringify(j));
        }

        const result: any[] = [];
        j.results.forEach((k: any) => {
          result.push(k);
        });

        if (!j.next) return result;
        return await searchRecursive(urlStr, page + 1).then((next) => [
          ...result,
          ...next,
        ]);
      })
  );
}

function search(url: string) {
  return searchRecursive(url); //todo: sortering???  .then((r) => r.sort(sortKlant));
}

export function useSearch(params: Ref<SearchParameters>) {
  const getUrl = () => getSearchUrl(params.value);
  return ServiceResult.fromFetcher(getUrl, search);
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
