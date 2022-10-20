import type { ServiceData } from "@/services";

export type UpdateContactgegevensParams = Pick<
  Klant,
  "id" | "telefoonnummers" | "emails"
>;

export interface Klant {
  _typeOfKlant: "klant";
  id: string;
  klantnummer: string;
  voornaam: string;
  voorvoegselAchternaam?: string;
  achternaam: string;
  telefoonnummers: { telefoonnummer: string }[];
  emails: { email: string }[];
  bsn?: string;
}

export interface Persoon {
  _typeOfKlant: "persoon";
  bsn: string;
  postcode?: string;
  huisnummer?: string;
  geboortedatum?: Date;
  voornaam: string;
  voorvoegselAchternaam?: string;
  achternaam: string;
  geboorteplaats: string;
  geboorteland: string;
}

export interface EnrichedPersoon {
  naam: ServiceData<string | null>;
  bsn: string | undefined;
  telefoonnummers: ServiceData<string | null>;
  emails: ServiceData<string | null>;
  geboortedatum: ServiceData<Date | null | undefined>;
  postcodeHuisnummer: ServiceData<string | null>;
  create: () => Promise<void>;
  detailLink: ServiceData<{
    to: string;
    title: string;
  } | null>;
}
