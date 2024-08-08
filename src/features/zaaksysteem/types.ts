export type ZaakDetails = {
  id: string;
  identificatie: string;
  toelichting: string;
  startdatum?: Date;
  bronorganisatie: string;
  verantwoordelijkeOrganisatie: string;
  zaaktype: string;
  zaaktypeLabel: string;
  zaaktypeOmschrijving: string;
  status: string;
  behandelaar: string;
  aanvrager: string;
  fataleDatum?: Date;
  streefDatum?: Date;
  indienDatum?: Date;
  registratieDatum?: Date;
  self: string;
  url: string;
  documenten?: ZaakDocument[];
  omschrijving: string;
  zaaksysteemId: string;
};

export interface ZaakPreview {
  identificatie: string;
  zaaktypeLabel: string;
  status: string;
  zaaksysteemId: string;
}

export interface ZaakDocument {
  id: string;
  titel: string;
  bestandsomvang: number;
  bestandsnaam: string;
  creatiedatum: Date;
  vertrouwelijkheidaanduiding: string;
  formaat: string;
  url: string;
}

export type ZaakType = {
  id: string;
  onderwerp: string;
  omschrijving: string;
  doorlooptijd: string;
  servicenorm: string;
};

export type OrganisatorischeEenheid = {
  naam: string;
};

export type Vestiging = {
  handelsnaam?: string[];
  vestigingsNummer: string;
};

export type NietNatuurlijkPersoon = {
  statutaireNaam: string;
};

export type NatuurlijkPersoon = {
  voornamen: string;
  voorvoegselGeslachtsnaam: string;
  geslachtsnaam: string;
};

export type Medewerker = {
  achternaam: string;
  geslachtsnaam: string;
  identificatie: string;
  voorletters: string;
  voorvoegselAchternaam: string;
};

export type RolType = {
  url: string;
  uuid: string;
  zaak: string;
  betrokkene: string;
  betrokkeneType: string;
  roltype: string;
  omschrijving: string;
  omschrijvingGeneriek: string;
  roltoelichting: string;
  registratiedatum: string;
  indicatieMachtiging: string;
  betrokkeneIdentificatie:
    | Medewerker
    | OrganisatorischeEenheid
    | Vestiging
    | NietNatuurlijkPersoon
    | NatuurlijkPersoon;
};
