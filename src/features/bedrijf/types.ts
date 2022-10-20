import type { ServiceData } from "@/services";

export interface BedrijfHandelsregister {
  _typeOfKlant: "bedrijf";
  kvknummer: string;
  vestigingsnummer: string;
  postcode: string;
  huisnummer: string;
  telefoonnummer: string;
  email: string;
}

export interface EnrichedBedrijf {
  bedrijfsnaam: ServiceData<string>;
  kvknummer: ServiceData<string>;
  postcodeHuisnummer: ServiceData<string>;
  email: ServiceData<string>;
  telefoonnummer: ServiceData<string>;
  create: () => Promise<void>;
  detailLink: ServiceData<{
    to: string;
    title: string;
  } | null>;
}

import type { PostcodeHuisnummer } from "@/helpers/validation";

export type SearchCategoryTypes = {
  handelsnaam: string;
  kvkNummer: string;
  postcodeHuisnummer: PostcodeHuisnummer;
  emailadres: string;
  telefoonnummer: string;
};

export type SearchCategories = keyof SearchCategoryTypes;

export type BedrijfQueryDictionary = {
  [K in SearchCategories]: (
    search: SearchCategoryTypes[K]
  ) => readonly [string, string][];
};

export type BedrijfQuery<K extends SearchCategories = SearchCategories> = {
  field: K;
  value: SearchCategoryTypes[K];
};
