import {
  ServiceResult,
  fetchLoggedIn,
  parseJson,
  parsePagination,
  throwIfNotOk,
  type PaginatedResult,
} from "@/services";

export interface Afdeling {
  id: string;
  identificatie: string;
  naam: string;
}
const getAfdelingenSearchUrl = (
  search: string | undefined,
  exactMatch: boolean,
) => {
  const searchParams = new URLSearchParams();
  searchParams.set("ordering", "record__data__naam");

  const matchType = exactMatch ? "exact" : "icontains";
  if (search) {
    searchParams.set("data_attr", `naam__${matchType}__${search.trim()}`);
  }
  return "/api/afdelingen/api/v2/objects?" + searchParams;
};

// eslint-disable-next-line @typescript-eslint/no-explicit-any
const mapOrganisatie = (x: any) =>
  ({
    ...x.record.data,
    id: x.uuid,
  }) as Afdeling;

const afdelingenFetcher = (url: string): Promise<PaginatedResult<Afdeling>> =>
  fetchLoggedIn(url)
    .then(throwIfNotOk)
    .then(parseJson)
    .then((json) => parsePagination(json, mapOrganisatie));

export const fetchAfdelingen = (
  search: string | undefined,
  exactMatch: boolean,
) => afdelingenFetcher(getAfdelingenSearchUrl(search, exactMatch));

export function useAfdelingen(search: () => string | undefined) {
  const getUrl = () => getAfdelingenSearchUrl(search(), false);
  return ServiceResult.fromFetcher(getUrl, afdelingenFetcher);
}
