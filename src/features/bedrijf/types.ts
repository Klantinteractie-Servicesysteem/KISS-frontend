import type { ServiceData } from "@/services";

export interface BedrijfKlant {
  _bedrijfType: "klant";
  id: string;
  klantnummer: string;
  bedrijfsnaam: string;
  telefoonnummers: { telefoonnummer: string }[];
  emails: { email: string }[];
  vestigingsnummer: string;
}

export interface BedrijfHandelsregister {
  _bedrijfType: "handelsregister";
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
