import type {
  GeslachtsnaamGeboortedatum,
  PostcodeHuisnummerMetAchternaam,
} from "@/helpers/validation";

export interface Persoon {
  _typeOfKlant: "persoon";
  bsn: string;
  geboortedatum?: Date;
  voornaam: string;
  voorvoegselAchternaam?: string;
  achternaam: string;
  geboorteplaats?: string;
  geboorteland?: string;
  adresregel1?: string;
  adresregel2?: string;
  adresregel3?: string;
}

export type PersoonQuery =
  | {
      bsn: string;
    }
  | {
      postcodeHuisnummer: PostcodeHuisnummerMetAchternaam;
    }
  | {
      geslachtsnaamGeboortedatum: GeslachtsnaamGeboortedatum;
    };
