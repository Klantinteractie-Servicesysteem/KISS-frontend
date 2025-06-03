export type KlantIdentificator = {
  bsn?: string;
  kvkNummer?: string;
  vestigingsnummer?: string;
};

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
