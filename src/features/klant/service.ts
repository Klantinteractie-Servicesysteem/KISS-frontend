import {
  ServiceResult,
  fetchLoggedIn,
  type Paginated,
  parsePagination,
  throwIfNotOk,
  parseJson,
  type ServiceData,
  enforceOneOrZero,
} from "@/services";
import { mutate } from "swrv";
import type { Ref } from "vue";
import { KlantType } from "../shared/types";
import { getKlantIdUrl } from "../shared/urls";

import type { UpdateContactgegevensParams, Klant } from "./types";

type QueryParam = [string, string][];

type FieldParams = {
  email: string;
  telefoonnummer: string;
};

export function createKlantQuery<K extends KlantSearchField>(
  args: KlantSearch<K>
): KlantSearch<K> {
  return args;
}

export type KlantSearchField = keyof FieldParams;

type QueryDictionary = {
  [K in KlantSearchField]: (search: FieldParams[K]) => QueryParam;
};

const queryDictionary: QueryDictionary = {
  email: (search) => [["emails.email", `%${search}%`]],
  telefoonnummer: (search) => [
    ["telefoonnummers.telefoonnummer", `%${search}%`],
  ],
};

export type KlantSearch<K extends KlantSearchField> = {
  field: K;
  query: FieldParams[K];
};

function getQueryParams<K extends KlantSearchField>(params: KlantSearch<K>) {
  return queryDictionary[params.field](params.query) as ReturnType<
    QueryDictionary[K]
  >;
}

type KlantSearchParameters<K extends KlantSearchField = KlantSearchField> = {
  query: Ref<KlantSearch<K> | undefined>;
  page: Ref<number | undefined>;
};

const klantRootUrl = `${window.gatewayBaseUri}/api/klanten`;

function setExtend(url: URL) {
  url.searchParams.set("extend[]", "all");
}

function getKlantSearchUrl<K extends KlantSearchField>(
  search: KlantSearch<K> | undefined,
  page: number | undefined
) {
  if (!search?.query) return "";

  const url = new URL(klantRootUrl);
  setExtend(url);
  url.searchParams.set("order[achternaam]", "asc");
  url.searchParams.set("page", page?.toString() ?? "1");
  url.searchParams.append("subjectType", KlantType.Persoon);

  getQueryParams(search).forEach((tuple) => {
    url.searchParams.set(...tuple);
  });

  return url.toString();
}

function mapKlant(obj: any): Klant {
  const { subjectIdentificatie, emails, telefoonnummers } = obj?.embedded ?? {};
  const { inpBsn, verblijfsadres, geboortedatum } = subjectIdentificatie ?? {};
  const { aoaHuisnummer, aoaPostcode } = verblijfsadres ?? {};

  return {
    ...obj,
    _typeOfKlant: "klant",
    emails: emails ?? [],
    telefoonnummers: telefoonnummers ?? [],
    bsn: inpBsn,
    postcode: aoaPostcode,
    huisnummer: aoaHuisnummer,
    geboortedatum: geboortedatum && new Date(geboortedatum),
  };
}

function searchKlanten(url: string): Promise<Paginated<Klant>> {
  return fetchLoggedIn(url)
    .then(throwIfNotOk)
    .then(parseJson)
    .then((j) => parsePagination(j, mapKlant))
    .then((p) => {
      p.page.forEach((klant) => {
        const idUrl = getKlantIdUrl(klant.id);
        if (idUrl) {
          mutate(idUrl, klant);
        }
        const bsnUrl = getKlantBsnUrl(klant.bsn);

        if (bsnUrl) {
          mutate(bsnUrl, klant);
        }
      });
      return p;
    });
}

function getKlantBsnUrl(bsn?: string) {
  if (!bsn) return "";
  const url = new URL(klantRootUrl);
  setExtend(url);
  url.searchParams.set("subjectIdentificatie.inpBsn", bsn);
  return url.toString();
}

const searchSingleKlant = (url: string) =>
  searchKlanten(url).then(enforceOneOrZero);

const getSingleBsnSearchId = (bsn: string | undefined) => {
  const url = getKlantBsnUrl(bsn);
  if (!url) return url;
  return url + "_single";
};

function fetchKlantById(url: string) {
  return fetchLoggedIn(url).then(throwIfNotOk).then(parseJson).then(mapKlant);
}

export function useKlantById(id: Ref<string>) {
  return ServiceResult.fromFetcher(
    () => getKlantIdUrl(id.value),
    fetchKlantById
  );
}

function updateContactgegevens({
  id,
  telefoonnummers,
  emails,
}: UpdateContactgegevensParams): Promise<UpdateContactgegevensParams> {
  const url = klantRootUrl + "/" + id;
  return fetchLoggedIn(url + "?fields[]=klantnummer&fields[]=bronorganisatie")
    .then(throwIfNotOk)
    .then(parseJson)
    .then(({ klantnummer, bronorganisatie }) =>
      fetchLoggedIn(url, {
        method: "PUT",
        headers: {
          "Content-Type": "application/json",
        },
        body: JSON.stringify({
          telefoonnummers,
          emails,
          klantnummer,
          bronorganisatie,
        }),
      })
    )
    .then(throwIfNotOk)
    .then(parseJson)
    .then(({ embedded }) => ({
      id,
      telefoonnummers: embedded?.telefoonnummers ?? [],
      emails: embedded?.emails ?? [],
    }));
}

export function useSearchKlanten<K extends KlantSearchField>({
  query,
  page,
}: KlantSearchParameters<K>) {
  const getUrl = () => getKlantSearchUrl(query.value, page.value);
  return ServiceResult.fromFetcher(getUrl, searchKlanten);
}

export function useKlantByBsn(
  getBsn: () => string | undefined
): ServiceData<Klant | null> {
  const getUrl = () => getKlantBsnUrl(getBsn());

  return ServiceResult.fromFetcher(getUrl, searchSingleKlant, {
    getUniqueId: () => getSingleBsnSearchId(getBsn()),
  });
}

export function useUpdateContactGegevens() {
  return ServiceResult.fromSubmitter(updateContactgegevens);
}

export async function ensureKlantForBsn(bsn: string) {
  const bsnUrl = getKlantBsnUrl(bsn);
  const singleBsnId = getSingleBsnSearchId(bsn);

  if (!bsnUrl || !singleBsnId) throw new Error();

  const first = await searchSingleKlant(bsnUrl);

  if (first) {
    mutate(singleBsnId, first);
    const idUrl = getKlantIdUrl(first.id);
    mutate(idUrl, first);
    return first;
  }

  const response = await fetchLoggedIn(klantRootUrl, {
    method: "POST",
    body: JSON.stringify({
      bronorganisatie: window.organisatieIds[0],
      // TODO: WAT MOET HIER IN KOMEN?
      klantnummer: "123",
      subjectIdentificatie: { inpBsn: bsn },
      subjectType: KlantType.Persoon,
    }),
  });

  if (!response.ok) throw new Error();

  const json = await response.json();
  const newKlant = mapKlant(json);
  const idUrl = getKlantIdUrl(newKlant.id);

  mutate(idUrl, newKlant);
  mutate(singleBsnId, newKlant);

  return newKlant;
}
