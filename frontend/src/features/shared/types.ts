export interface ContactmomentZaak {
  status: string;
  zaaktype: string;
  zaaknummer: string;
}

export interface ContactmomentContactverzoek {
  medewerker: string;
  completed?: Date;
}
