import type { PostcodeHuisnummer } from "@/helpers/validation";

export type KvkVestiging = {
  vestigingsnummer: string;
  kvkNummer: string;
  handelsnaam: string;
  postcode: string;
  huisnummer: string;
  huisletter: string;
  huisnummertoevoeging: string;
};

export type KvkPagination = {
  pagina: number;
  resultatenPerPagina: number;
  totaal: number;
  resultaten: any[];
};

export interface Bedrijf {
  _typeOfKlant: "bedrijf";
  kvkNummer: string;
  type: string;
  vestigingsnummer?: string;
  rsin?: string;
  bedrijfsnaam: string;
  postcode?: string;
  huisnummer?: string;
  straatnaam: string;
  huisletter?: string;
  huisnummertoevoeging?: string;
  woonplaats?: string;
}

export type BedrijfSearchOptions =
  | {
      handelsnaam: string;
    }
  | { kvkNummer: string }
  | { postcodeHuisnummer: PostcodeHuisnummer }
  | { vestigingsnummer: string };

export type BedrijfIdentifier =
  | {
      vestigingsnummer: string;
      kvkNummer: string;
    }
  | {
      kvkNummer: string;
    }
  | {
      //deze variant is nodig omdt we van de e-suite alleen het vestigingsnr terug krijkegen en niet het kvknummer.
      //we moeten dus ook op basis vn alleen een vestiginsnummer gegevens kunnen opzoeken
      vestigingsnummer: string;
    };
