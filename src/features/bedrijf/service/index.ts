import { KlantType } from "@/features/shared/types";
import { getKlantIdUrl } from "@/features/shared/urls";
import {
  enforceOneOrZero,
  fetchLoggedIn,
  parseJson,
  parsePagination,
  ServiceResult,
  throwIfNotOk,
  type Paginated,
  type ServiceData,
} from "@/services";
import { mutate } from "swrv";
import type { BedrijfHandelsregister, BedrijfKlant } from "../types";
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
    _bedrijfType: "handelsregister",
    kvknummer,
    vestigingsnummer,
    postcode,
    huisnummer: straatHuisnummer,
    telefoonnummer,
    email: emailAdres,
  };
}

function mapKlantRegister(json: any): BedrijfKlant {
  const { klantnummer, id, embedded, bedrijfsnaam } = json ?? {};
  const { subjectIdentificatie, emails, telefoonnummers } = embedded ?? {};
  const { vestigingsnummer } = subjectIdentificatie ?? {};

  return {
    _bedrijfType: "klant",
    id,
    emails: emails ?? [],
    telefoonnummers: telefoonnummers ?? [],
    vestigingsnummer,
    klantnummer,
    bedrijfsnaam,
  };
}

function searchBedrijvenInKlantRegister(url: string) {
  return fetchLoggedIn(url)
    .then(throwIfNotOk)
    .then(parseJson)
    .then((json) => parsePagination(json, mapKlantRegister))
    .then((paginated) => {
      paginated.page.forEach((klant) => {
        mutate(getKlantIdUrl(klant.id), klant);
      });
      return paginated;
    });
}

function searchBedrijvenInHandelsRegister(url: string) {
  return fetchLoggedIn(url)
    .then(throwIfNotOk)
    .then(parseJson)
    .then((json) => parsePagination(json, mapHandelsRegister));
}

type SearchBedrijfArguments<K extends SearchCategories> = {
  query: BedrijfQuery<K> | undefined;
  page: number | undefined;
};

export function useSearchBedrijven<K extends SearchCategories>(
  getArgs: () => SearchBedrijfArguments<K>
) {
  const fetcher = (
    url: string
  ): Promise<Paginated<BedrijfHandelsregister | BedrijfKlant>> => {
    return url.startsWith(handelsRegisterBaseUrl)
      ? searchBedrijvenInHandelsRegister(url)
      : searchBedrijvenInKlantRegister(url);
  };

  return ServiceResult.fromFetcher(
    () => getSearchBedrijvenUrl(getArgs()),
    fetcher
  );
}

export const useBedrijfHandelsregisterByVestigingsnummer = (
  getVestigingsnummer: () => string
) => {
  const getUrl = () => {
    const vestigingsnummer = getVestigingsnummer();
    if (!vestigingsnummer) return "";
    const url = new URL(handelsRegisterBaseUrl);
    url.searchParams.set("extend[]", "all");
    url.searchParams.set("vestigingsnummer", vestigingsnummer);
    return url.toString();
  };

  const getUniqueId = () => getUrl() + "_single";

  const fetcher = (url: string) =>
    searchBedrijvenInHandelsRegister(url).then(enforceOneOrZero);

  return ServiceResult.fromFetcher(getUrl, fetcher, {
    getUniqueId,
  });
};

export const useBedrijfKlantByVestigingsnummer = (
  getVestigingsnummer: () => string
) => {
  const getUrl = () => {
    const vestigingsnummer = getVestigingsnummer();
    if (!vestigingsnummer) return "";
    const url = new URL(klantRegisterBaseUrl);
    url.searchParams.set("extend[]", "all");
    url.searchParams.set(
      "subjectIdentificatie.vestigingsnummer",
      vestigingsnummer
    );
    url.searchParams.set("subjectType", KlantType.Bedrijf);
    return url.toString();
  };

  const getUniqueId = () => getUrl() + "_single";

  const fetcher = (url: string) =>
    searchBedrijvenInKlantRegister(url).then(enforceOneOrZero);

  return ServiceResult.fromFetcher(getUrl, fetcher, {
    getUniqueId,
  }) as ServiceData<BedrijfKlant | null>;
};
