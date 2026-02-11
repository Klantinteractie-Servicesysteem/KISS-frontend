// Kan alleen groep of afdeling zijn, medewerker bestaat niet
export enum TypeOrganisatorischeEenheid {
  Afdeling = "afdeling",
  Groep = "groep",
}

//////////////////////////////////////

//contactverzoek vragen set interfaces

export interface ContactVerzoekVragenSet {
  id: number;
  titel: string;
  vraagAntwoord: Vraag[];
  organisatorischeEenheidId: string;
  organisatorischeEenheidSoort: TypeOrganisatorischeEenheid;
}

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

///////////////////////

export type Kanaal = {
  id: number;
  naam: string;
};

export type Afdeling = { afdelingnaam: string };

export type Kennisartikel = {
  url: string;
  title: string;
  sections: string[];
  afdelingen?: Afdeling[];
  afdeling?: string;
  sectionIndex: number;
};

export const globalSearchBaseUri = "/api/elasticsearch";
