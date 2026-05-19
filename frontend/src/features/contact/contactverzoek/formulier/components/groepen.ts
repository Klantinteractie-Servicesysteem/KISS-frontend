import {
  fetchLoggedIn,
  parseJson,
  parsePagination,
  ServiceResult,
  throwIfNotOk,
  type PaginatedResult,
} from "@/services";

export interface Groep {
  id: string;
  afdelingId?: string;
  identificatie: string;
  naam: string;
}

export const getGroepenSearchUrl = (
  search: string | undefined,
  exactMatch: boolean,
) => {
  const searchParams = new URLSearchParams();
  searchParams.set("ordering", "record__data__naam");

  const matchType = exactMatch ? "exact" : "icontains";
  if (search) {
    searchParams.set("data_attr", `naam__${matchType}__${search.trim()}`);
  }
  return "/api/groepen/api/v2/objects?" + searchParams.toString();
};

// eslint-disable-next-line @typescript-eslint/no-explicit-any
const mapGroep = (x: any): Groep => ({
  id: x.uuid,
  afdelingId: x.afdelingId,
  naam: x.record.data.naam,
  identificatie: x.record.data.identificatie,
});

export const groepenFetcher = (url: string): Promise<PaginatedResult<Groep>> =>
  fetchLoggedIn(url)
    .then(throwIfNotOk)
    .then(parseJson)
    .then((json) => parsePagination(json, mapGroep));

export const fetchGroepen = (search: string | undefined, exactMatch: boolean) =>
  groepenFetcher(getGroepenSearchUrl(search, exactMatch));

export function useGroepen(search: () => string | undefined) {
  const getUrl = () => getGroepenSearchUrl(search(), false);
  return ServiceResult.fromFetcher(getUrl, groepenFetcher);
}
