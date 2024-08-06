export interface Klant {
  _typeOfKlant: "klant";
  id: string;
  klantnummer: string;
  telefoonnummer?: string;
  emailadres?: string;
  bsn?: string;
  bedrijfsnaam?: string;
  vestigingsnummer?: string;
  url?: string;
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
