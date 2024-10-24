import type { Klant } from "../openklant/types";

export type UpdateContactgegevensParams = Pick<
  Klant,
  "id" | "telefoonnummers" | "emailadressen"
>;

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

export interface ContactmomentObject {
  contactmoment: string;
  object: string;
  objectType: string;
}
