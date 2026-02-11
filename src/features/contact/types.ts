//todo: cleanup. not everything should be here

export interface ContactmomentViewModel {
  url: string;
  registratiedatum: string;
  kanaal: string;
  tekst: string;
  zaaknummers: string[];
  medewerkerIdentificatie: MedewerkerIdentificatie;
}

export interface MedewerkerIdentificatie {
  identificatie: string;
  achternaam: string;
  voorletters: string;
  voorvoegselAchternaam: string;
}

import type { TypeOrganisatorischeEenheid } from "@/features/types";
import type { DigitaalAdresTypes } from "@/services/openklant2";

export type DigitaalAdres = {
  adres: string;
  soortDigitaalAdres?: DigitaalAdresTypes;
  omschrijving?: string;
};

export type ContactverzoekData = {
  status: string;
  contactmoment: string;
  registratiedatum: string;
  datumVerwerkt?: string;
  toelichting?: string;
  actor: {
    naam: string;
    soortActor: string;
    identificatie: string;
    naamOrganisatorischeEenheid?: string;
    typeOrganisatorischeEenheid?: TypeOrganisatorischeEenheid;
    identificatieOrganisatorischeEenheid?: string;
  };
  betrokkene: {
    rol: "klant";
    klant?: string;
    persoonsnaam?: {
      voornaam?: string;
      voorvoegselAchternaam?: string;
      achternaam?: string;
    };
    organisatie?: string;
    digitaleAdressen: DigitaalAdres[];
  };
  verantwoordelijkeAfdeling: string;
};

export type NewContactverzoek = {
  record: {
    startAt: string;
    data: ContactverzoekData;
  };
};
