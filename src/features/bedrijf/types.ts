export interface BedrijfKlant {
  id: string;
  klantnummer: string;
  bedrijfsnaam: string;
  telefoonnummers: { telefoonnummer: string }[];
  emails: { email: string }[];
}

export interface BedrijfHandelsregister {
  kvknummer: string;
  vestigingsnummer: string;
  postcode: string;
  huisnummer: string;
  telefoonnummer: string;
  email: string;
}
