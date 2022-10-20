import { KlantType } from "@/features/shared/types";
import {
  fetchLoggedIn,
  parseJson,
  parsePagination,
  ServiceResult,
  throwIfNotOk,
} from "@/services";
import type { BedrijfHandelsregister } from "../types";
import type {
  SearchCategories,
  BedrijfQuery,
  BedrijfQueryDictionary,
} from "./types";

export * from "./types";

export const bedrijfQuery = <K extends SearchCategories>(
  query: BedrijfQuery<K>
) => query;

const klantRegisterBaseUrl = window.gatewayBaseUri + "/api/klanten";
const handelsRegisterBaseUrl = window.gatewayBaseUri + "/api/vestigingen";

const bedrijfQueryDictionary: BedrijfQueryDictionary = {
  postcodeHuisnummer: ({ postcode, huisnummer }) => [
    ["bezoekadres.postcode", postcode.digits + postcode.numbers],
    ["bezoekadres.straatHuisnummer", huisnummer],
  ],
  emailadres: (search) => [["emails.email", `%${search}%`]],
  telefoonnummer: (search) => [
    ["telefoonnummers.telefoonnummer", `%${search}%`],
  ],
  kvkNummer: (search) => [["kvknummer", search]],
  handelsnaam: (search) => [["eersteHandelsnaam", search]],
};

const getSearchBedrijvenUrl = <K extends SearchCategories>({
  query,
  page,
}: SearchBedrijfArguments<K>) => {
  if (!query) return "";

  const url = new URL(handelsRegisterBaseUrl);
  // TODO: think about how to search in both klantregister and handelsregister for phone / email

  url.searchParams.set("page", page?.toString() ?? "1");
  url.searchParams.set("extend[]", "all");

  const searchParams = bedrijfQueryDictionary[query.field](query.value);

  searchParams.forEach((tuple) => {
    // url.searchParams.set(...tuple);
  });

  return url.toString();
};

function mapHandelsRegister(json: any): BedrijfHandelsregister {
  const {
    bezoekadres,
    emailAdres,
    telefoonnummer,
    vestigingsnummer,
    kvknummer,
  } = json ?? {};

  const { straatHuisnummer, postcode } = bezoekadres ?? {};

  return {
    kvknummer,
    vestigingsnummer,
    postcode,
    huisnummer: straatHuisnummer,
    telefoonnummer,
    email: emailAdres,
  };
}

function mapKlantRegister(json: any): any {}

function searchBedrijven(url: string) {
  const mapper = url.startsWith(handelsRegisterBaseUrl)
    ? mapHandelsRegister
    : mapKlantRegister;

  return fetchLoggedIn(url)
    .then(throwIfNotOk)
    .then(parseJson)
    .then((json) => parsePagination(json, mapper));
}

type SearchBedrijfArguments<K extends SearchCategories> = {
  query: BedrijfQuery<K> | undefined;
  page: number | undefined;
};

export const useSearchBedrijven = <K extends SearchCategories>(
  getArgs: () => SearchBedrijfArguments<K>
) => {
  return ServiceResult.fromFetcher(
    () => getSearchBedrijvenUrl(getArgs()),
    searchBedrijven
  );
};
