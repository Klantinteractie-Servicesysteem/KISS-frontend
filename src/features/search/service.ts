import {
  parseJson,
  parseValidUrl,
  ServiceResult,
  throwIfNotOk,
  type Paginated,
} from "@/services";
import { fetchLoggedIn } from "@/services";
import type { Ref } from "vue";
import type { SearchResult, Source } from "./types";

export function mapResult(obj: any): SearchResult {
  const source = obj?._source?.object_bron ?? "Website";
  const id = obj?._id;

  const title = obj?._source?.title ?? obj?._source?.headings?.[0];
  const content = obj?._source?.body_content;
  const url = parseValidUrl(obj?._source?.url);
  const documentUrl = new URL(location.origin);
  documentUrl.searchParams.set("query", id);

  const jsonObject = obj?._source?.[source] ?? null;
  return {
    source,
    id,
    title,
    content,
    url,
    jsonObject,
    documentUrl,
  };
}

const pageSize = 10;

function mapSuggestions(json: any): string[] {
  if (!Array.isArray(json?.suggest?.suggestions)) return [];
  const result = [...json.suggest.suggestions].flatMap(({ options }: any) =>
    options.map(({ text }: any) => (text as string).toLocaleLowerCase()),
  ) as string[];
  return [...new Set(result)];
}

export function useGlobalSearch(
  parameters: Ref<{
    search?: string;
    page?: number;
    filters: Source[];
  }>,
) {
  const getUrl = () =>
    parameters.value.search ? "/api/search" : "";

  const getPayload = () => {
    if (!parameters.value.search) return "";
    return JSON.stringify({
      query: parameters.value.search,
      page: parameters.value.page || 1,
      filters: parameters.value.filters ?? [],
    });
  };

  async function fetcher(
    url: string,
  ): Promise<Paginated<SearchResult> & { suggestions: string[] }> {
    const r = await fetchLoggedIn(url, {
      method: "POST",
      headers: { "content-type": "application/json" },
      body: getPayload(),
    });
    if (!r.ok) throw new Error();
    const json = await r.json();
    const {
      hits: { total, hits },
    } = json ?? {};
    const totalPages = Math.ceil((total?.value || 0) / pageSize);
    const page = Array.isArray(hits) ? hits.map(mapResult) : [];
    return {
      page,
      pageSize,
      pageNumber: parameters.value.page || 1,
      totalPages,
      suggestions: mapSuggestions(json),
    };
  }

  function getUniqueId() {
    if (!parameters.value.search) return "";
    return `${getPayload()}`;
  }

  return ServiceResult.fromFetcher(getUrl, fetcher, { getUniqueId });
}

export function useSources() {
  async function fetcher(u: string): Promise<Source[]> {
    const r = await fetchLoggedIn(u, { method: "GET" });
    if (!r.ok) throw new Error();
    const json = await r.json();
    const {
      aggregations: { bronnen, domains },
    } = json ?? {};

    const sources: Source[] = [...bronnen.buckets, ...domains.buckets].flatMap(
      ({ key, by_index: { buckets } }) =>
        buckets.map((x: any) => ({
          index: x.key,
          name: key,
        })),
    );

    return sources;
  }

  return ServiceResult.fromFetcher(() => "/api/search/sources", fetcher);
}

export type DatalistItem = {
  value: string;
  description: string;
};

export async function searchMedewerkers(parameters: any): Promise<DatalistItem[]> {
  function mapToDataListItem(obj: any): any {
    const functie = obj?._source.Smoelenboek.functie || obj.function;
    const department =
      obj?._source.Smoelenboek.afdelingen?.[0]?.afdelingnaam ||
      obj?._source.Smoelenboek.department;

    const werk = [functie, department].filter(Boolean).join(" bij ");
    return {
      value: obj?._source.title,
      description: werk,
      identificatie: obj?._source?.Smoelenboek?.identificatie,
      afdelingen: obj?._source?.Smoelenboek?.afdelingen,
      groepen: obj?._source?.Smoelenboek?.groepen,
    };
  }

  const r = await fetchLoggedIn("/api/search/medewerkers", {
    method: "POST",
    headers: { "content-type": "application/json" },
    body: JSON.stringify({
      search: parameters.search || null,
      filterField: parameters.filterField || null,
      filterValue: parameters.filterValue || null,
    }),
  });
  throwIfNotOk(r);
  const json = await parseJson(r);
  const { hits: { hits } } = json ?? {};
  return Array.isArray(hits) ? hits.map(mapToDataListItem) : [];
}
