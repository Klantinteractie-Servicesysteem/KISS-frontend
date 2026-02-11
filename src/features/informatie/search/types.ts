export type SearchResult = {
  id: string;
  title: string;
  source: string;
  content: string;
  url?: URL;
  jsonObject: any;
  documentUrl: URL;
};

export type Source = {
  boost?: number;
  index: string;

  name: string;
};

export type Medewerker = {
  id: string;
  voornaam: string;
  voorvoegselAchternaam?: string;
  achternaam: string;
  emailadres: string;
  url: string;
};

export type Website = {
  title: string;
  url: string;
};

export type Vac = {
  uuid?: string;
  vraag: string;
  antwoord: string;
  toelichting?: string;
  afdelingen?: VacAfdeling[];
  trefwoorden?: { trefwoord: string }[];
  status?: string;
  doelgroep?: string;
};

export type Nieuwsbericht = {
  url: string;
  title: string;
};

export type Werkinstructie = {
  url: string;
  title: string;
};

export type VacAfdeling = { afdelingNaam: string };
