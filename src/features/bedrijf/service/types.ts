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
