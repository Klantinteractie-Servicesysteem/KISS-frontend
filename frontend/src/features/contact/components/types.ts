import type { DigitaalAdresTypes } from "@/services/openklant2";

export interface Vraag {
  description: string;
  questiontype: string;
}

export interface InputVraag extends Vraag {
  input: string;
}

export interface TextareaVraag extends Vraag {
  textarea?: string;
}

export interface DropdownVraag extends Vraag {
  options: string[];
  selectedDropdown: string;
}

export interface CheckboxVraag extends Vraag {
  options: string[];
  selectedCheckbox: string[];
}

export interface ContactVerzoekVragenSet {
  id: number;
  titel: string;
  vraagAntwoord: Vraag[];
  organisatorischeEenheidId: string;
  organisatorischeEenheidSoort: TypeOrganisatorischeEenheid;
}

// Kan alleen groep of afdeling zijn, medewerker bestaat niet
export enum TypeOrganisatorischeEenheid {
  Afdeling = "afdeling",
  Groep = "groep",
}

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
