export interface Medewerker {
  id: string;
  voornaam: string;
  voorvoegselAchternaam?: string;
  achternaam: string;
  emailadres: string;
  telefoonnummer1?: string;
  telefoonnummer2?: string;
}

type VestigingIdentificatie = {
  vestigingsnummer: string;
  kvkNummer: string;
  bedrijfsnaam: string;
};

type RechtspersoonIdentificatie = {
  kvkNummer: string;
  rsin: string;
  bedrijfsnaam: string;
};

type NatuurlijkPersoonIdentificatie = {
  bsn: string;
  voornaam: string;
  voorvoegselAchternaam?: string;
  achternaam: string;
};

export type KlantIdentificatie = Partial<
  VestigingIdentificatie &
    RechtspersoonIdentificatie &
    NatuurlijkPersoonIdentificatie
>;
