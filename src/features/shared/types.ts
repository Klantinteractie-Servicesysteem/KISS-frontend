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
