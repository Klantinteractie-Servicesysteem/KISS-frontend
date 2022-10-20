export interface ContactmomentViewModel {
  id: string;
  url: string;
  startdatum: Date;
  registratiedatum: Date;
  medewerker: string;
  kanaal: string;
  resultaat: string;
  tekst: string;
  zaken: ContactmomentZaak[];
  contactverzoeken: ContactmomentContactverzoek[];
  "x-commongateway-metadata": {
    owner: string;
  };
  primaireVraag?: string;
  primaireVraagWeergave?: string;
  afwijkendOnderwerp?: string;
}

export interface ContactmomentZaak {
  status: string;
  zaaktype: string;
  zaaknummer: string;
}

export interface ContactmomentContactverzoek {
  medewerker: string;
  completed?: Date;
}

export enum KlantType {
  Persoon = "natuurlijk_persoon",
  Bedrijf = "vestiging",
}

export interface Klant {
  _typeOfKlant: "klant";
  id: string;
  klantnummer: string;
  voornaam?: string;
  voorvoegselAchternaam?: string;
  achternaam?: string;
  telefoonnummers: { telefoonnummer: string }[];
  emails: { email: string }[];
  bsn?: string;
  bedrijfsnaam?: string;
  vestigingsnummer?: string;
}
