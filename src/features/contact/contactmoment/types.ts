export interface MedewerkerIdentificatie {
  identificatie: string;
  achternaam: string;
  voorletters: string;
  voorvoegselAchternaam: string;
}

export interface Contactmoment {
  bronorganisatie: string; //verplicht in de api
  registratiedatum: string; //2019-08-24T14:15:22Z //serverside?
  kanaal: string;
  tekst: string;
  onderwerpLinks: string[];
  initiatiefnemer: string;
  vraag?: string;
  specifiekevraag?: string;
  gespreksresultaat: string;

  //bovenstaande slaan we op bij een contactmoment.
  //de rest is mogelijk obsolete.
  //wellicht nog te gebruiken voor oa contactverzoeken
  vorigContactmoment: string | undefined;
  voorkeurskanaal: string;
  voorkeurstaal: string;
  medewerker: string;
  startdatum: string;
  einddatum: string;
  gespreksId?: string;
  verantwoordelijkeAfdeling?: string;
}

export interface ContactmomentDetails {
  id: string;
  startdatum: string;
  einddatum: string;
  gespreksresultaat?: string;
  vraag?: string;
  specifiekeVraag?: string;
  emailadresKcm?: string;
  verantwoordelijkeAfdeling?: string;
}

export interface Gespreksresultaat {
  definitie: string;
}

export interface ContactverzoekDetail {
  id: string;
  datum: string;
  status: string;
  behandelaar: string;
  afgerond: string;
  starttijd: string;
  aanmaker: string;
  notitie: string;
  vraag?: string;
  specifiekevraag?: string;
}

export interface ObjectContactmoment {
  url: string;
  contactmoment: string;
  object: string;
  objectType: string;
}

export interface KlantContactmoment {
  url: string;
  contactmoment: string;
  klant: string;
  rol: string;
}

export type SaveContactmomentResponseModel = {
  data?: { url: string; gespreksId?: string };
  errorMessage?: string;
};
