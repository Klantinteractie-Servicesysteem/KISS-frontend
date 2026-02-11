import type { DigitaalAdresTypes } from "@/services/openklant2";

export type ContactverzoekOverzichtItem = {
  uuid: string;
  kanaal: string;
  url: string;
  registratiedatum: string;
  onderwerp: string;
  status: string;
  aangemaaktDoor: string;
  behandelaar: string;
  toelichtingVoorCollega: string;
  toelichtingBijContactmoment: string;
  betrokkene?: {
    isGeauthenticeerd: boolean;
    persoonsnaam: {
      voorletters?: string;
      voornaam?: string;
      voorvoegselAchternaam?: string;
      achternaam?: string;
    };
    organisatie?: string;
    digitaleAdressen: {
      omschrijving?: string;
      soortDigitaalAdres?: DigitaalAdresTypes;
      adres: string;
    }[];
  };
  vraag: string;
  zaaknummers: string[];
  systeemId?: string;
};
