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
    typeof jsonObject?.embedded?.acf?.publication_content !== "string" ||
    typeof jsonObject?.date !== "string"
  ) {
    throw new Error(
      "invalid werkbericht, required fields are missing. input: " +
        JSON.stringify(jsonObject)
    );
  }

  const berichtTypeId = jsonObject?.embedded?.acf?.publication_type;
  const berichtTypeName = getBerichtTypeNameById(berichtTypeId) ?? "onbekend";

  const skillIds = jsonObject?.embedded?.acf?.publication_skill;
  const skillNames = Array.isArray(skillIds)
    ? skillIds.map(
        (x) => (typeof x === "number" && getSkillNameById(x)) || "onbekend"
      )
    : ["onbekend"];

  const dateCreated = parseDateStrWithTimezone(jsonObject.date);
  const dateModified = parseDateStrWithTimezone(
    jsonObject["x-commongateway-metadata"].dateModified
  );
  const dateLatest = maxDate([dateCreated, dateModified]);

  let dateRead = jsonObject["x-commongateway-metadata"]?.dateRead;

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
    content: jsonObject.embedded.acf.publication_content,
    date: dateLatest,
    type: berichtTypeName,
    skills: skillNames,
    url: jsonObject["x-commongateway-metadata"]?.self,
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

    const params: [string, string][] = [
      ["extend[]", "x-commongateway-metadata.dateRead"],
    ];

    params.push(["limit", "10"]);
    params.push(["order[modified]", "desc"]);
    params.push(["extend[]", "x-commongateway-metadata.dateModified"]);
    params.push(["extend[]", "x-commongateway-metadata.self"]);
    params.push(["extend[]", "acf"]);

    if (typeId) {
      params.push(["acf.publication_type", typeId.toString()]);
    }

    if (search) {
      params.push(["search", search]);
    }

    if (page) {
      params.push(["page", page.toString()]);
    }

    if (skillIds?.length) {
      skillIds.forEach((skillId) => {
        params.push(["acf.publication_skill[]", skillId.toString()]);
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

    return parsePagination(json, (bericht: any) =>
      parseWerkbericht(
        bericht,
        typesResult.data.fromKeyToValue,
        skillsResult.data.fromKeyToValue
      )
    );
  }

  return ServiceResult.fromFetcher(getUrl, fetchBerichten, { poll: true });
}

export async function readBericht(id: string): Promise<boolean> {
  const res = await fetchLoggedIn(`${BERICHTEN_BASE_URI}/${id}?fields[]`);

  if (!res.ok)
    throw new Error(`Expected to read bericht: ${res.status.toString()}`);

  return res.ok;
}

export async function unreadBericht(id: string): Promise<boolean> {
  const res = await fetchLoggedIn(`${BERICHTEN_BASE_URI}/${id}`, {
    method: "PUT",
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
