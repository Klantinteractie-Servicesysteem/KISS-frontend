export interface Klant {
  _typeOfKlant: "klant";
  id?: string;
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
  url: string;
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
