//api types

export type DigitaalAdresApiViewModel = {
  adres: string;
  soortDigitaalAdres?: DigitaalAdresTypes;
  omschrijving?: string;
};

export type DigitaalAdresExpandedApiViewModel = DigitaalAdresApiViewModel & {
  _expand?: {
    verstrektDoorBetrokkene?: Betrokkene;
  };
};

export type ExpandedKlantContactApiViewmodel = {
  uuid: string;
  url: string;
  plaatsgevondenOp: string;
  kanaal: string;
  inhoud: string;
  onderwerp: string;
  hadBetrokkenActoren: Array<{
    soortActor: string;
    naam: string;
    actoridentificator: {
      objectId: string;
    };
  }>;
  gingOverOnderwerpobjecten: { uuid: string; url: string }[];
  _expand: {
    hadBetrokkenen?: Betrokkene[];
    leiddeTotInterneTaken?: InternetaakApiViewModel[];
    gingOverOnderwerpobjecten?: OnderwerpObjectPostModel[];
  };
};

export type InternetaakApiViewModel = {
  uuid: string;
  url: string;
  nummer: string;
  gevraagdeHandeling: string;
  toegewezenAanActoren: Array<{
    uuid: string;
    url: string;
  }>;
  toelichting: string;
  status: string;
  toegewezenOp: string;
  afgehandeldOp: string;
  actor: ActorApiViewModel;
};

export type ActorApiViewModel = {
  uuid: string;
  url: string;
  naam: string;
  soortActor: string;
  indicatieActief: boolean;
};

//applicatie types
export type Betrokkene = {
  uuid: string;
  wasPartij?: { uuid: string; url: string };

  digitaleAdressen: Array<{ uuid: string; url: string }>;
  contactnaam: {
    achternaam: string;
    voorletters: string;
    voornaam: string;
    voorvoegselAchternaam: string;
  };
  hadKlantcontact?: { uuid: string; url: string };
};

export type BetrokkeneMetKlantContact = Betrokkene & {
  klantContact: ExpandedKlantContactApiViewmodel;
  expandedDigitaleAdressen?: DigitaalAdresApiViewModel[];
};

export interface InternetaakPostModel {
  nummer: string;
  gevraagdeHandeling: string;
  aanleidinggevendKlantcontact: {
    uuid: string;
  };
  toegewezenAanActoren: {
    uuid: string;
  }[];
  toelichting: string;
  status: "te_verwerken" | "verwerkt";
  afgehandeldOp?: string;
}

export type SaveInterneTaakResponseModel = {
  data?: { url: string; gespreksId?: string };
  errorMessage?: string;
};

export type SaveKlantContactResponseModel = {
  data?: { url: string; uuid: string };
  errorMessage?: string;
};

export interface KlantContactPostmodel {
  uuid?: string;
  nummer?: string;
  kanaal: string;
  onderwerp: string;
  inhoud: string;
  indicatieContactGelukt: boolean;
  taal: string;
  vertrouwelijk: boolean;
  plaatsgevondenOp: string; // 2019-08-24T14:15:22Z
}

//todo: KlantBedrijfIdentifier vervangen door KlantIdentificator
export type KlantBedrijfIdentifier =
  | {
      bsn: string;
    }
  | {
      vestigingsnummer: string;
      kvkNummer: string;
    }
  | {
      kvkNummer: string;
    };

///////////////////////////////

export type Contactnaam = {
  voornaam: string;
  voorvoegselAchternaam?: string;
  achternaam: string;
};

export enum DigitaalAdresTypes {
  email = "email",
  telefoonnummer = "telefoonnummer",
  overig = "overig",
}

export enum CodeRegister {
  brp = "brp",
  hr = "hr",
  overig = "overig",
}

export enum CodeObjecttype {
  natuurlijkPersoon = "natuurlijk_persoon",
  vestiging = "vestiging",
  nietNatuurlijkPersoon = "niet_natuurlijk_persoon",
  overig = "overig",
}

export enum CodeSoortObjectId {
  bsn = "bsn",
  kvkNummer = "kvk_nummer",
  rsin = "rsin",
  vestigingsnummer = "vestigingsnummer",
  overig = "overig",
}

export type IdentificatorType = {
  codeRegister: CodeRegister;
  codeSoortObjectId: CodeSoortObjectId;
  codeObjecttype: CodeObjecttype;
};

export const identificatorTypes = {
  persoon: {
    codeRegister: CodeRegister.brp,
    codeSoortObjectId: CodeSoortObjectId.bsn,
    codeObjecttype: CodeObjecttype.natuurlijkPersoon,
  },
  vestiging: {
    codeRegister: CodeRegister.hr,
    codeSoortObjectId: CodeSoortObjectId.vestigingsnummer,
    codeObjecttype: CodeObjecttype.vestiging,
  },
  nietNatuurlijkPersoonRsin: {
    codeRegister: CodeRegister.hr,
    codeSoortObjectId: CodeSoortObjectId.rsin,
    codeObjecttype: CodeObjecttype.nietNatuurlijkPersoon,
  },
  nietNatuurlijkPersoonKvkNummer: {
    codeRegister: CodeRegister.hr,
    codeSoortObjectId: CodeSoortObjectId.kvkNummer,
    codeObjecttype: CodeObjecttype.nietNatuurlijkPersoon,
  },
} satisfies Record<string, IdentificatorType>;

export enum PartijTypes {
  persoon = "persoon",
  organisatie = "organisatie",
  contactpersoon = "contactpersoon",
}

export type Partij = {
  nummer?: string;
  uuid: string;
  url: string;
  partijIdentificatie: {
    contactnaam?: Contactnaam;
    naam?: string;
  };
  partijIdentificatoren: { uuid: string }[];
  _expand?: {
    digitaleAdressen?: {
      adres?: string;
      soortDigitaalAdres?: DigitaalAdresTypes;
    }[];
  };
};

export type Identificator = {
  objectId: string;
  codeObjecttype: string;
  codeRegister: string;
  codeSoortObjectId: string;
};

export type OnderwerpObjectPostModel = {
  klantcontact: { uuid: string };
  wasKlantcontact: { uuid: string } | null;
  onderwerpobjectidentificator: Identificator;
};

export type PartijIdentificator = {
  uuid: string;
  identificeerdePartij: {
    url: string;
    uuid: string;
  } | null;
  partijIdentificator: Identificator;
  subIdentificatorVan?: {
    uuid: string;
  };
};
