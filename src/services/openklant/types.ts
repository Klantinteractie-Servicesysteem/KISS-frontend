export interface Klant {
  _typeOfKlant: "klant";
  id: string;
  klantnummer: string;
  telefoonnummers: string[];
  emailadressen: string[];

  telefoonnummer?: string;
  emailadres?: string;

  bsn?: string;
  bedrijfsnaam?: string;
  vestigingsnummer?: string;
  rsin?: string;
  kvkNummer?: string;
  nietNatuurlijkPersoonIdentifier?: string;
  url: string;
}
