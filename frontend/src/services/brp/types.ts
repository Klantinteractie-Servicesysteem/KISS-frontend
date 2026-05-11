import type {
  GeslachtsnaamGeboortedatum,
  PostcodeHuisnummerMetAchternaam,
} from "@/helpers/validation";

export interface Persoon {
  _typeOfKlant: "persoon";
  bsn: string;
  geboortedatum?: Date;
  geslacht: string;
  voornaam: string;
  voorvoegselAchternaam?: string;
  achternaam: string;
  geboorteplaats?: string;
  geboorteland?: string;
  adresregel1?: string;
  adresregel2?: string;
  adresregel3?: string;
  geheimhoudingPersoonsgegevens?: boolean;
}

export type PersoonQuery =
  | {
      bsn: string;
    }
  | {
      postcodeHuisnummerAchternaam: PostcodeHuisnummerMetAchternaam;
    }
  | {
      geslachtsnaamGeboortedatum: GeslachtsnaamGeboortedatum;
    };
