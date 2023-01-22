import type { Werkbericht } from "./types";
import {
  createLookupList,
  parsePagination,
  ServiceResult,
  type LookupList,
  type Paginated,
  type ServiceData,
} from "@/services";
import type { Ref } from "vue";
import { fetchLoggedIn } from "@/services";

const WP_MAX_ALLOWED_PAGE_SIZE = "100";
const BERICHTEN_BASE_URI = `${window.gatewayBaseUri}/api/kiss_openpub_pub`;

export type UseWerkberichtenParams = {
  typeId?: number;
  search?: string;
  skillIds?: number[];
  page?: number;
  pagesize?: number;
};

function parseDateStrWithTimezone(dateStr: string) {
  const timezoneRegex = /T[0-9|:]*[+|-|Z]+/;

  if (timezoneRegex.test(dateStr)) return new Date(dateStr);

  // if no timezone info is present we assume UTC
  return new Date(dateStr + "Z");
}

function maxDate(dates: Date[]) {
  return new Date(Math.max(...dates.map((x) => x.getTime())));
}

/**
 * Tries to parse a json object returned by the api as a Werkbericht
 * @param jsonObject a json object
 * @param getBerichtTypeNameById a function to get the name of a berichttype from it's id
 */
function parseWerkbericht(
  jsonObject: any,
  getBerichtTypeNameById: (id: number) => string | undefined,
  getSkillNameById: (id: number) => string | undefined
): Werkbericht {
  if (
    typeof jsonObject?.embedded?.title?.rendered !== "string" ||
    typeof jsonObject?.embedded?.acf?.publicationContent !== "string" ||
    typeof jsonObject?.date !== "string"
  ) {
    throw new Error(
      "invalid werkbericht, required fields are missing. input: " +
        JSON.stringify(jsonObject)
    );
  }

  const berichtTypeId = jsonObject?.embedded?.acf?.publicationType;
  const berichtTypeName = getBerichtTypeNameById(berichtTypeId) ?? "onbekend";

  const skillIds = jsonObject?.embedded?.acf?.publicationSkill;
  const skillNames = Array.isArray(skillIds)
    ? skillIds.map(
        (x) => (typeof x === "number" && getSkillNameById(x)) || "onbekend"
      )
    : ["onbekend"];

  const dateCreated = parseDateStrWithTimezone(jsonObject.date);
  const dateModified = parseDateStrWithTimezone(jsonObject.modified);

  const dateLatest = maxDate([dateCreated, dateModified]);

  let dateRead = jsonObject["_self"]?.dateRead;

  if (
    dateRead &&
    new Date(dateModified) > new Date(parseDateStrWithTimezone(dateRead))
  ) {
    unreadBericht(jsonObject.id);

    dateRead = false;
  }

  return {
    id: jsonObject.id,
    read: !!dateRead,
    title: jsonObject.embedded.title.rendered,
    content: jsonObject.embedded.acf.publicationContent,
    date: dateLatest,
    type: berichtTypeName,
    skills: skillNames,
    url: `/api/kiss_openpub_pub/${jsonObject.id}`, //jsonObject["_self"]?.self,
    featured: jsonObject.embedded.acf.publicationFeatured,
  };
}

/**
 * Fetches a lookuplist from the api
 * @param url
 */
function fetchLookupList(urlStr: string): Promise<LookupList<number, string>> {
  const url = new URL(urlStr);

  // having pagination here is a nuisance.
  if (!url.searchParams.has("page")) {
    url.searchParams.set("per_page", WP_MAX_ALLOWED_PAGE_SIZE);
  }

  return fetchLoggedIn(url)
    .then((r) => r.json())
    .then((json) => {
      if (!Array.isArray(json))
        throw new Error(
          "Invalide json, verwacht een lijst: " + JSON.stringify(json)
        );
      return json
        .filter((x) => typeof x?.id === "number" && typeof x?.slug === "string")
        .map((x) => [x.id, x.slug] as [number, string]);
    })
    .then(createLookupList);
}

/**
 * Returns a reactive ServiceData object promising a LookupList of berichttypes
 */
export function useBerichtTypes(): ServiceData<LookupList<number, string>> {
  const url = window.gatewayBaseUri + "/api/openpub/openpub_type";
  return ServiceResult.fromFetcher(url, fetchLookupList);
}

/**
 * Returns a reactive ServiceData object promising a LookupList of skills
 */
export function useSkills(): ServiceData<LookupList<number, string>> {
  const url = window.gatewayBaseUri + "/api/openpub/openpub_skill";
  return ServiceResult.fromFetcher(url, fetchLookupList);
}

/**
 * Returns a reactive ServiceData object promising a paginated list of Werkberichten.
 * This has a dependency on useBerichtTypes()
 * @param parameters
 */
export function useWerkberichten(
  parameters?: Ref<UseWerkberichtenParams>
): ServiceData<Paginated<Werkbericht>> {
  const typesResult = useBerichtTypes();
  const skillsResult = useSkills();

  function getUrl() {
    // we return a falsy value if we haven't received the berichttypes yet,
    // because we need them to look up names of berichttypes by their id.
    // a falsy value indicates to the SWRV library that it should not yet trigger a fetch
    if (typesResult.state !== "success" || skillsResult.state !== "success")
      return "";

    if (!parameters?.value) return BERICHTEN_BASE_URI;

    const { typeId, search, page, skillIds } = parameters.value;

    const params: [string, string][] = [["extend[]", "_self.dateRead"]];

    params.push(["_limit", "10"]);
    params.push(["_order[modified]", "desc"]);
    params.push(["extend[]", "_self.self"]);
    params.push(["extend[]", "acf"]);
    params.push(["embedded.acf.publicationEndDate[after]", "now"]);

    if (typeId) {
      params.push([
        "embedded.acf.publicationType[int_compare]",
        typeId.toString(),
      ]);
    }

    if (search) {
      params.push([
        "_search[embedded.title.rendered,embedded.acf.publicationContent]",
        search,
      ]);
    }

    if (page) {
      params.push(["page", page.toString()]);
    }

    if (skillIds?.length) {
      skillIds.forEach((skillId) => {
        params.push([
          "embedded.acf.publicationSkill[int_compare][]",
          skillId.toString(),
        ]);
      });
    }
    return `${BERICHTEN_BASE_URI}?${new URLSearchParams(params)}`;
  }

  async function fetchBerichten(url: string): Promise<Paginated<Werkbericht>> {
    if (
      typesResult.state !== "success" ||
      skillsResult.state !== "success" ||
      !url
    )
      throw new Error(
        "this should never happen, we already check this in the url function"
      );

    const r = await fetchLoggedIn(url);
    if (!r.ok) throw new Error(r.status.toString());

    const json = await r.json();

    const berichten = json.results;

    if (!Array.isArray(berichten))
      throw new Error("expected a list, input: " + JSON.stringify(berichten));

    const featuredBerichten = berichten.filter(
      (bericht) => bericht.embedded.acf.publicationFeatured
    );
    const regularBerichten = berichten.filter(
      (bericht) => !bericht.embedded.acf.publicationFeatured
    );
    const sortedBerichten = [...featuredBerichten, ...regularBerichten];

    return parsePagination(
      { ...json, results: sortedBerichten },
      (bericht: any) =>
        parseWerkbericht(
          bericht,
          typesResult.data.fromKeyToValue,
          skillsResult.data.fromKeyToValue
        )
    );
  }

  return ServiceResult.fromFetcher(getUrl, fetchBerichten, { poll: true });
}

export function useFeaturedWerkberichtenCount() {
  async function fetchFeaturedWerkberichten(url: string): Promise<number> {
    const r = await fetchLoggedIn(url);

    if (!r.ok) throw new Error(r.status.toString());

    const json = await r.json();

    if (!json.results.length) return 0;

    return json.results.filter((result: any) => !result["_self"].dateRead)
      .length;
  }

  function getUrl() {
    const params: [string, string][] = [
      ["embedded.acf.publicationFeatured[bool_compare]", "true"],
      ["fields[]", "_self.dateRead"],
      ["extend[]", "_self.dateRead"],
      ["embedded.acf.publicationEndDate[after]", "now"],
    ];

    return `${BERICHTEN_BASE_URI}?${new URLSearchParams(params)}`;
  }

  return ServiceResult.fromFetcher(getUrl(), fetchFeaturedWerkberichten, {
    poll: true,
  });
}

export async function readBericht(id: string): Promise<boolean> {
  const res = await fetchLoggedIn(`${BERICHTEN_BASE_URI}/${id}?fields[]`);

  if (!res.ok)
    throw new Error(`Expected to read bericht: ${res.status.toString()}`);

  return res.ok;
}

export async function unreadBericht(id: string): Promise<boolean> {
  const res = await fetchLoggedIn(`${BERICHTEN_BASE_URI}/${id}`, {
    method: "PATCH",
    headers: {
      Accept: "application/json",
      "Content-Type": "application/json",
    },
    body: JSON.stringify({ "@dateRead": false }),
  });

  if (!res.ok)
    throw new Error(`Expected to unread bericht: ${res.status.toString()}`);

  return res.ok;
}
