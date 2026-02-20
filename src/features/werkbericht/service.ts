import type { Berichttype, Werkbericht } from "./types";
import {
  createLookupList,
  ServiceResult,
  type LookupList,
  type Paginated,
  type ServiceData,
} from "@/services";
import { ref, type Ref } from "vue";
import { fetchLoggedIn } from "@/services";

export const featuredWerkberichtenCount = ref<number | undefined>(undefined);

export type UseWerkberichtenParams = {
  type?: Berichttype;
  search?: string;
  skillIds?: number[];
  page?: number;
  pagesize?: number;
};

/**
 * Tries to parse a json object returned by the api as a Werkbericht
 * @param jsonObject a json object
 * @param getBerichtTypeNameById a function to get the name of a berichttype from it's id
 */
function parseWerkbericht({
  id,
  inhoud,
  isBelangrijk,
  publicatieDatum,
  wijzigingsDatum,
  titel,
  type,
  skills,
  isGelezen,
  url,
}: any = {}): Werkbericht {
  return {
    id,
    read: isGelezen,
    title: titel,
    content: inhoud,
    date: publicatieDatum && new Date(publicatieDatum),
    modified: wijzigingsDatum && new Date(wijzigingsDatum),
    type,
    skills,
    featured: isBelangrijk,
    url,
  };
}

/**
 * Fetches a LookupList of skills with ID number and Name of the skills
 * @returns A Promise that contains a LookupList<number,string>
 */
export async function fetchSkills(): Promise<LookupList<number, string>> {
  const r = await fetchLoggedIn("/api/skills");

  if (!r.ok) throw new Error(r.status.toString());

  const json = await r.json();
  return createLookupList(
    json.map(({ id, naam }: { id: number; naam: string }) => [id, naam]),
  );
}

/**
 * Returns a reactive ServiceData object promising a paginated list of Werkberichten.
 * This has a dependency on useBerichtTypes()
 * @param parameters
 */
export function useWerkberichten(
  parameters?: Ref<UseWerkberichtenParams>,
): ServiceData<Paginated<Werkbericht>> {
  function getUrl() {
    const base = "/api/berichten/published";
    if (!parameters?.value) return base;

    const { type, search, page, skillIds } = parameters.value;

    const params: [string, string][] = [["pageSize", "10"]];

    if (type) {
      params.push(["type", type]);
    }

    if (search) {
      params.push(["search", search]);
    }

    if (page) {
      params.push(["page", page.toString()]);
    }

    if (skillIds?.length) {
      skillIds.forEach((skillId) => {
        params.push(["skillIds", skillId.toString()]);
      });
    }

    return `${base}?${new URLSearchParams(params)}`;
  }

  async function fetchBerichten(url: string): Promise<Paginated<Werkbericht>> {
    const r = await fetchLoggedIn(url);
    if (!r.ok) throw new Error(r.status.toString());

    const json = await r.json();

    if (!Array.isArray(json))
      throw new Error("expected a list, input: " + JSON.stringify(json));

    const page = json.map(parseWerkbericht);

    const intHeader = (name: string) => {
      const header = r.headers.get(name);
      if (!header) throw new Error("expected header with name " + name);
      return +header;
    };

    return {
      page,
      pageNumber: intHeader("X-Current-Page"),
      totalRecords: intHeader("X-Total-Records"),
      totalPages: intHeader("X-Total-Pages"),
      pageSize: intHeader("X-Page-Size"),
    };
  }

  return ServiceResult.fromFetcher(getUrl, fetchBerichten, { poll: true });
}

/**
 * Fetches the number of Werkberichten
 * @returns Returns a Promise that contains the number of Werkberichten
 */
export async function fetchFeaturedWerkberichten(): Promise<number> {
  const r = await fetchLoggedIn("/api/berichten/featuredcount");

  if (!r.ok) throw new Error(r.status.toString());

  const json = await r.json();

  featuredWerkberichtenCount.value = json.count;
  return json.count;
}

export async function putBerichtRead(id: string, isGelezen: boolean) {
  const res = await fetchLoggedIn(`/api/berichten/${id}/read`, {
    method: "PUT",
    headers: {
      "content-type": "application/json",
    },
    body: JSON.stringify({
      isGelezen,
    }),
  });

  if (!res.ok)
    throw new Error(`Expected to read bericht: ${res.status.toString()}`);
}
