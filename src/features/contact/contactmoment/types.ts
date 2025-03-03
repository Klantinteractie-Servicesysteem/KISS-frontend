export interface MedewerkerIdentificatie {
  identificatie: string;
  achternaam: string;
  voorletters: string;
  voorvoegselAchternaam: string;
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
