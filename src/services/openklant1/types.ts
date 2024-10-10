export type UpdateContactgegevensParams = Pick<
  Klant,
  "id" | "telefoonnummers" | "emailadressen"
>;

export interface Klant {
  _typeOfKlant: "klant";
  id: string;
  klantnummer: string;
  telefoonnummers: string[];
  emailadressen: string[];
  bsn?: string;
  bedrijfsnaam?: string;
  vestigingsnummer?: string;
  url?: string;
  kvkNummer?: string;
  nietNatuurlijkPersoonIdentifier?: string;
}

export enum KlantType {
  Persoon = "natuurlijk_persoon",
  Bedrijf = "vestiging",
  NietNatuurlijkPersoon = "niet_natuurlijk_persoon",
}

export interface Contactverzoek {
  contactmoment: string;
}

export type BedrijfIdentifier =
| {
    vestigingsnummer: string;
  }
| {
    nietNatuurlijkPersoonIdentifier: string;
  };
