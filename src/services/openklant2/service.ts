import {
  enforceOneOrZero,
  parseJson,
  parsePagination,
  throwIfNotOk,
} from "@/services";

import { SPECIFIEKEVRAAG_MAXLENGTH } from "@/services/openklant/service";

import {
  type BetrokkeneMetKlantContact,
  type ExpandedKlantContactApiViewmodel,
  type ActorApiViewModel,
  type InternetaakPostModel,
  type SaveInterneTaakResponseModel,
  type KlantContactPostmodel,
  type SaveKlantContactResponseModel,
  type DigitaalAdresApiViewModel,
  PartijTypes,
  identificatorTypes,
  type Contactnaam,
  type Partij,
  DigitaalAdresTypes,
  type OnderwerpObjectPostModel,
  type Betrokkene,
  CodeSoortObjectId,
  type PartijIdentificator,
  type KlantBedrijfIdentifier,
} from "./types";

import type { ContactverzoekData } from "../../features/contact/types";
import type { Klant, KlantIdentificator } from "../openklant/types";
import type { Vraag } from "@/stores/contactmoment/index";
import { fetchWithSysteemId } from "../fetch-with-systeem-id";
import { codeObjecttype } from "../knownConsts";

const klantinteractiesProxyRoot = "/api/klantinteracties";
const klantinteractiesApiRoot = "/api/v1";
const klantinteractiesBaseUrl = `${klantinteractiesProxyRoot}${klantinteractiesApiRoot}`;
const klantinteractiesKlantcontacten = `${klantinteractiesBaseUrl}/klantcontacten`;
const klantinteractiesActoren = `${klantinteractiesBaseUrl}/actoren`;
const klantinteractiesDigitaleadressen = `${klantinteractiesBaseUrl}/digitaleadressen`;
const klantinteractiesBetrokkenen = `${klantinteractiesBaseUrl}/betrokkenen`;

////////////////////////////////////////////
// contactmomenten and contactverzoeken
export async function enrichBetrokkeneWithKlantContact(
  systeemId: string,
  value: Betrokkene[],
  expand?: KlantContactExpand[],
): Promise<BetrokkeneMetKlantContact[]> {
  for (const betrokkene of value) {
    const klantContactId = betrokkene.hadKlantcontact?.uuid;
    if (!klantContactId) {
      continue;
    }
    const klantcontact = await fetchKlantcontact({
      systeemId,
      uuid: klantContactId,
      expand,
    });
    (betrokkene as BetrokkeneMetKlantContact).klantContact = klantcontact; // er is altijd maar 1 contact bij een betrokkeke!
  }
  return value as BetrokkeneMetKlantContact[];
}

////////////////////////////////////////////
// contactverzoeken
export function filterOutContactmomenten(
  value: BetrokkeneMetKlantContact[],
): BetrokkeneMetKlantContact[] {
  const filtered = value.filter(
    (item) => item?.klantContact?._expand?.leiddeTotInterneTaken?.length,
  );
  return filtered;
}

export async function enrichInterneTakenWithActoren(
  systeemId: string,
  value: BetrokkeneMetKlantContact[],
): Promise<BetrokkeneMetKlantContact[]> {
  for (const betrokkeneWithKlantcontact of value) {
    const internetaak =
      betrokkeneWithKlantcontact?.klantContact?._expand
        ?.leiddeTotInterneTaken?.[0];

    if (!internetaak) {
      continue;
    }

    const actoren = internetaak.toegewezenAanActoren || [];

    //we halen alle actoren op en kiezen dan de eerste medewerker. als er geen medewerkers bij staan de eerste organisatie
    //wordt naar verwachting tzt aangepast, dan gaan we gewoon alle actoren bij de internetak tonen

    const actorenDetails = [] as ActorApiViewModel[];

    for (const actor of actoren) {
      const actorDetails = await fetchActor(systeemId, actor.uuid);
      actorenDetails.push(actorDetails);
    }

    const medewerkerActor = actorenDetails.find(
      (x) => x.soortActor === "medewerker",
    );

    if (medewerkerActor) {
      internetaak.actor = medewerkerActor as ActorApiViewModel;
    } else {
      const organisatorischerEenheidActor = actorenDetails.find(
        (x) => x.soortActor === "organisatorische_eenheid",
      );
      internetaak.actor = organisatorischerEenheidActor as ActorApiViewModel;
    }
  }

  return value;
}

export function fetchActor(systeemId: string, id: string) {
  return fetchWithSysteemId(systeemId, `${klantinteractiesActoren}/${id}`)
    .then(throwIfNotOk)
    .then(parseJson)
    .then((d) => d as ActorApiViewModel);
}

export async function enrichBetrokkeneWithDigitaleAdressen(
  systeemId: string,
  value: BetrokkeneMetKlantContact[],
): Promise<BetrokkeneMetKlantContact[]> {
  for (const betrokkeneWithKlantcontact of value) {
    betrokkeneWithKlantcontact.expandedDigitaleAdressen ??= [];
    for (const digitaalAdres of betrokkeneWithKlantcontact.digitaleAdressen) {
      const url = `${klantinteractiesDigitaleadressen}/${digitaalAdres.uuid}?`;
      const expanded = await fetchWithSysteemId(systeemId, url)
        .then(throwIfNotOk)
        .then(parseJson)
        .then((d) => d as DigitaalAdresApiViewModel);
      betrokkeneWithKlantcontact.expandedDigitaleAdressen.push(expanded);
    }
  }

  return value;
}

export function fetchBetrokkenen({
  systeemId,
  ...params
}: {
  systeemId: string;
  wasPartij__url: string;
  pageSize: string;
}) {
  const query = new URLSearchParams(params);
  return fetchWithSysteemId(
    systeemId,
    `${klantinteractiesBetrokkenen}?${query}`,
  )
    .then(throwIfNotOk)
    .then(parseJson)
    .then((p) => parsePagination(p, (x) => x as Betrokkene));
}

export enum DigitaleAdressenExpand {
  verstrektDoorBetrokkene = "verstrektDoorBetrokkene",
  verstrektDoorBetrokkene_hadKlantcontact = "verstrektDoorBetrokkene.hadKlantcontact",
  verstrektDoorBetrokkene_hadKlantcontact_leiddeTotInterneTaken = "verstrektDoorBetrokkene.hadKlantcontact.leiddeTotInterneTaken",
}
export function searchDigitaleAdressen({
  systeemId,
  adres,
  page = 1,
  expand = [],
}: {
  systeemId: string;
  adres: string;
  page: number;
  expand: DigitaleAdressenExpand[];
}) {
  const params = new URLSearchParams({ adres, page: page.toString() });
  expand?.length && params.set("expand", expand.join(","));
  return fetchWithSysteemId(
    systeemId,
    klantinteractiesDigitaleadressen + "?" + params,
  )
    .then(throwIfNotOk)
    .then(parseJson)
    .then((r) => parsePagination(r, (x) => x as DigitaalAdresApiViewModel));
}

export function saveBetrokkene(
  systemIdentifier: string,
  {
    partijId,
    klantcontactId,
    organisatienaam,
    voornaam,
    voorvoegselAchternaam,
    achternaam,
  }: {
    partijId?: string;
    klantcontactId: string;
    organisatienaam?: string;
    voornaam?: string;
    voorvoegselAchternaam?: string;
    achternaam?: string;
  },
): Promise<{ uuid: string }> {
  const voorletters = voornaam ? voornaam.charAt(0) : undefined;

  return fetchWithSysteemId(systemIdentifier, klantinteractiesBetrokkenen, {
    method: "POST",
    headers: {
      "Content-Type": "application/json",
    },
    body: JSON.stringify({
      wasPartij: partijId ? { uuid: partijId } : null,
      hadKlantcontact: {
        uuid: klantcontactId,
      },
      rol: "klant",
      initiator: true,
      organisatienaam: organisatienaam || "",
      contactnaam: {
        voorletters: voorletters || "",
        voornaam: voornaam || "",
        voorvoegselAchternaam: voorvoegselAchternaam || "",
        achternaam: achternaam || "",
      },
    }),
  })
    .then(throwIfNotOk)
    .then((response) => response.json())
    .then((data) => ({
      uuid: data.uuid,
    }));
}

export const saveInternetaak = async (
  systemIdentifier: string,
  toelichting: string,
  contactmomentId: string,
  actorenIds: string[],
): Promise<SaveInterneTaakResponseModel> => {
  const createInternetaakPostModel = (
    uuid: string,
    actoren: string[],
  ): InternetaakPostModel => {
    const interneTaak: InternetaakPostModel = {
      nummer: "",
      gevraagdeHandeling: "Contact opnemen met betrokkene",
      aanleidinggevendKlantcontact: {
        uuid: uuid,
      },
      toegewezenAanActoren: [],
      toelichting: toelichting,
      status: "te_verwerken",
    };

    actoren.forEach((actor) => {
      interneTaak.toegewezenAanActoren.push({ uuid: actor });
    });

    return interneTaak;
  };

  const interneTaak = createInternetaakPostModel(contactmomentId, actorenIds);

  const response = await postInternetaak(systemIdentifier, interneTaak);
  const responseBody = await response.json();

  throwIfNotOk(response);
  return { data: responseBody };
};

const postInternetaak = (
  systemIdentifier: string,
  data: InternetaakPostModel,
): Promise<Response> => {
  return fetchWithSysteemId(systemIdentifier, `/api/postinternetaak`, {
    method: "POST",
    headers: {
      Accept: "application/json",
      "Content-Type": "application/json",
    },
    body: JSON.stringify(data),
  });
};

export const ensureActoren = async (
  systeemId: string,
  actorData: undefined | ContactverzoekData["actor"],
): Promise<string[]> => {
  if (!actorData) {
    return [];
  }

  const {
    identificatie,
    naam,
    typeOrganisatorischeEenheid,
    naamOrganisatorischeEenheid,
    identificatieOrganisatorischeEenheid,
  } = actorData;

  const getOrCreateActor = async (
    name: string,
    id: string,
    type?: "afdeling" | "groep" | undefined,
  ) => {
    const parsedModel = await mapActor({
      fullName: name,
      identificatie: id,
      typeOrganisatorischeEenheid: type,
    });
    const { results } = await getActorById(
      systeemId,
      parsedModel.actoridentificator,
    );
    if (results.length === 0) {
      return await postActor({
        systeemId: systeemId,
        parsedModel,
      });
    }
    return results[0].uuid;
  };

  const actoren: string[] = [];

  // als zowel een afdeling/groep als medewerker is geselecteerd
  if (naamOrganisatorischeEenheid && identificatieOrganisatorischeEenheid) {
    const actorUuid = await getOrCreateActor(naam, identificatie, undefined); // medewerker zonder organisatorische eenheid
    const organisatorischeActorUuid = await getOrCreateActor(
      naamOrganisatorischeEenheid,
      identificatieOrganisatorischeEenheid,
      typeOrganisatorischeEenheid,
    );
    if (actorUuid) actoren.push(actorUuid);
    if (organisatorischeActorUuid) actoren.push(organisatorischeActorUuid);
  } else {
    // als alleen een afdeling/groep is geselecteerd
    const actorUuid = await getOrCreateActor(
      naam,
      identificatie,
      typeOrganisatorischeEenheid,
    );
    if (actorUuid) actoren.push(actorUuid);
  }

  return actoren;
};

async function getActorById(
  systeemId: string,
  identificator: MappedActor["actoridentificator"],
): Promise<{ results: { uuid: string }[] }> {
  const url = `${klantinteractiesActoren}?${new URLSearchParams({
    actoridentificatorObjectId: identificator.objectId,
    actoridentificatorCodeRegister: identificator.codeRegister,
    actoridentificatorCodeSoortObjectId: identificator.codeSoortObjectId,
    actoridentificatorCodeObjecttype: identificator.codeObjecttype,
  })}`;
  const response = await fetchWithSysteemId(systeemId, url, {
    method: "GET",
    headers: {
      Accept: "application/json",
    },
  });

  throwIfNotOk(response);
  return await response.json();
}

export type BaseOrganizationalUnitType = "afdeling" | "groep" | "medewerker";

/**
 * Get actor configuration based on type and email settings
 * @param type The organizational unit type
 * @param config Configuration object with medewerker email settings
 * @returns Configuration object with appropriate settings
 */
function getActorConfig(
  type: BaseOrganizationalUnitType | undefined,
  config: { medewerkerEmailEnabled: boolean },
): {
  codeObjecttype: string;
  soortActor: string;
  codeRegister: string;
  codeSoortObjectId: string;
} {
  switch (type) {
    case "afdeling":
      return {
        codeObjecttype: codeObjecttype.afd.codeObjecttype,
        soortActor: "organisatorische_eenheid",
        codeRegister: "obj",
        codeSoortObjectId: "idf",
      };

    case "groep":
      return {
        codeObjecttype: codeObjecttype.grp.codeObjecttype,
        soortActor: "organisatorische_eenheid",
        codeRegister: "obj",
        codeSoortObjectId: "idf",
      };

    case "medewerker":
    default:
      // Handle medewerker with or without email enabled
      return {
        codeObjecttype: "mdw",
        soortActor: "medewerker",
        codeRegister: config.medewerkerEmailEnabled ? "handmatig" : "obj",
        codeSoortObjectId: config.medewerkerEmailEnabled ? "email" : "idf",
      };
  }
}

type MappedActor = {
  naam: string;
  soortActor: string;
  indicatieActief: boolean;
  actoridentificator: {
    objectId: string;
    codeObjecttype: string;
    codeRegister: string;
    codeSoortObjectId: string;
  };
};

async function mapActor({
  fullName,
  identificatie,
  typeOrganisatorischeEenheid,
}: {
  fullName: string;
  identificatie: string;
  typeOrganisatorischeEenheid: BaseOrganizationalUnitType | undefined;
}): Promise<MappedActor> {
  const medewerkerEmailEnabled = await useMedewerkeremail();

  const unitInfo = getActorConfig(typeOrganisatorischeEenheid, {
    medewerkerEmailEnabled,
  });

  return {
    naam: fullName,
    soortActor: unitInfo.soortActor,
    indicatieActief: true,
    actoridentificator: {
      objectId: identificatie,
      codeObjecttype: unitInfo.codeObjecttype,
      codeRegister: unitInfo.codeRegister,
      codeSoortObjectId: unitInfo.codeSoortObjectId,
    },
  };
}

async function postActor({
  systeemId,
  parsedModel,
}: {
  parsedModel: MappedActor;
  systeemId: string;
}): Promise<string> {
  const response = await fetchWithSysteemId(
    systeemId,
    klantinteractiesActoren,
    {
      method: "POST",
      headers: {
        Accept: "application/json",
        "Content-Type": "application/json",
      },
      body: JSON.stringify(parsedModel),
    },
  );

  throwIfNotOk(response);
  const jsonResponse = await response.json();
  return jsonResponse.uuid;
}

export const extractOnderwerp = (vraag: Vraag): string => {
  const ELLIPSIS = "...";
  const MAX_ONDERWERP_TOTAL = 200;

  const vraagTitle = vraag.vraag?.title?.trim() || "";
  const specifiekeVraag = vraag.specifiekevraag || "";

  //specifiekevraag length should be guarded by the input validation, just in case we'll cut it off
  const truncatedSpecifiekeVraag = specifiekeVraag.substring(
    0,
    SPECIFIEKEVRAAG_MAXLENGTH,
  );

  if (vraagTitle === "anders" || !vraagTitle) {
    return truncatedSpecifiekeVraag;
  }

  if (vraagTitle && truncatedSpecifiekeVraag) {
    const suffix = ` (${truncatedSpecifiekeVraag})`;
    const totalLength = vraagTitle.length + suffix.length;

    if (totalLength <= MAX_ONDERWERP_TOTAL) {
      //this should result in 'vraag (specifiekevraag)'
      return `${vraagTitle}${suffix}`;
    }

    const allowedVraagLength =
      MAX_ONDERWERP_TOTAL - suffix.length - ELLIPSIS.length;
    //this should result in 'truncatedVraag... (specifiekevraag)'
    return `${vraagTitle.slice(0, allowedVraagLength)}${ELLIPSIS}${suffix}`;
  }

  //this should result in 'vraag'
  if (vraagTitle.length <= MAX_ONDERWERP_TOTAL) {
    return vraagTitle;
  }

  //this should result in 'truncatedVraag...'
  return vraagTitle.slice(0, MAX_ONDERWERP_TOTAL - ELLIPSIS.length) + ELLIPSIS;
};

export const saveKlantContact = async (
  systemIdentifier: string,
  vraag: Vraag,
): Promise<SaveKlantContactResponseModel> => {
  const klantcontactPostModel: KlantContactPostmodel = {
    kanaal: vraag.kanaal,
    onderwerp: extractOnderwerp(vraag),
    inhoud: vraag.notitie,
    indicatieContactGelukt: true,
    taal: "nld",
    vertrouwelijk: false,
    plaatsgevondenOp: new Date().toISOString(),
  };

  const response = await postKlantContact(
    systemIdentifier,
    klantcontactPostModel,
  );
  const responseBody = await response.json();

  throwIfNotOk(response);
  return { data: responseBody };
};

const postKlantContact = (
  systemIdentifier: string,
  data: KlantContactPostmodel,
): Promise<Response> => {
  return fetchWithSysteemId(systemIdentifier, `/api/postklantcontacten`, {
    method: "POST",
    headers: {
      Accept: "application/json",
      "Content-Type": "application/json",
    },
    body: JSON.stringify(data),
  });
};

export const saveDigitaleAdressen = async (
  systemIdentifier: string,
  digitaleAdressen: DigitaalAdresApiViewModel[],
  verstrektDoorBetrokkeneUuid: string,
): Promise<Array<{ uuid: string; url: string }>> => {
  const savedAdressen: Array<{ uuid: string; url: string }> = [];

  for (const adres of digitaleAdressen) {
    const postBody = {
      verstrektDoorBetrokkene: { uuid: verstrektDoorBetrokkeneUuid },
      verstrektDoorPartij: null,
      adres: adres.adres,
      soortDigitaalAdres: adres.soortDigitaalAdres,
      omschrijving: adres.omschrijving || "onbekend",
    };

    const savedAdres = await postDigitaalAdres(systemIdentifier, postBody);
    savedAdressen.push(savedAdres);
  }

  return savedAdressen;
};

const postDigitaalAdres = async (
  systemIdentifier: string,
  data: {
    verstrektDoorBetrokkene: { uuid: string };
    verstrektDoorPartij?: { uuid: string } | null;
    adres: string;
    soortDigitaalAdres: DigitaalAdresTypes | undefined;
    omschrijving: string;
  },
): Promise<{ uuid: string; url: string }> => {
  const response = await fetchWithSysteemId(
    systemIdentifier,
    klantinteractiesDigitaleadressen,
    {
      method: "POST",
      headers: {
        Accept: "application/json",
        "Content-Type": "application/json",
      },
      body: JSON.stringify(data),
    },
  );

  const responseBody = await response.json();
  throwIfNotOk(response);
  return { uuid: responseBody.uuid, url: responseBody.url };
};

//-----------------------------------------------------------------------------------------------------------

export const fetchKlantByIdOk2 = (systeemId: string, uuid: string) => {
  return fetchWithSysteemId(
    systeemId,
    `${klantinteractiesBaseUrl}/partijen/${uuid}?${new URLSearchParams({ expand: "digitaleAdressen" })}`,
  )
    .then(throwIfNotOk)
    .then(parseJson)
    .then((partij) => mapPartijToKlant(systeemId, partij));
};

export const ensureOk2Klant = async (
  systeemId: string,
  parameters: KlantBedrijfIdentifier,
) => {
  return (
    (await findKlantByIdentifierOpenKlant2(systeemId, parameters)) ??
    (await createKlant(systeemId, parameters))
  );
};

export function fetchKlantByKlantIdentificatorOk2(
  systeemId: string,
  klantIdentificator: KlantIdentificator,
): Promise<Klant | null> {
  const expand = "digitaleAdressen";
  let soortPartij: string;
  let partijIdentificator__codeSoortObjectId: string;
  let partijIdentificator__objectId: string;

  if (klantIdentificator.bsn) {
    soortPartij = PartijTypes.persoon;
    partijIdentificator__codeSoortObjectId =
      identificatorTypes.persoon.codeSoortObjectId;
    partijIdentificator__objectId = klantIdentificator.bsn;
  } else {
    soortPartij = PartijTypes.organisatie;

    if (klantIdentificator.vestigingsnummer) {
      partijIdentificator__codeSoortObjectId =
        identificatorTypes.vestiging.codeSoortObjectId;
      partijIdentificator__objectId = klantIdentificator.vestigingsnummer;
    } else if (klantIdentificator.kvkNummer) {
      partijIdentificator__codeSoortObjectId =
        identificatorTypes.nietNatuurlijkPersoonKvkNummer.codeSoortObjectId;
      partijIdentificator__objectId = klantIdentificator.kvkNummer;
    } else {
      throw new Error("Geen geldige identificator opgegeven.");
    }
  }

  const searchParams = new URLSearchParams({
    expand,
    soortPartij,
    partijIdentificator__codeSoortObjectId,
    partijIdentificator__objectId,
  });

  return fetchWithSysteemId(
    systeemId,
    `${klantinteractiesBaseUrl}/partijen?${searchParams}`,
  )
    .then(throwIfNotOk)
    .then(parseJson)
    .then((r) =>
      parsePagination(r, (x) => mapPartijToKlant(systeemId, x as Partij)),
    )
    .then((x) => {
      //als er op kvk en vestiging gezocht is, moet de gevonden partij wel matchen
      // we willen niet en vestiging reourneren die bij een andere onderneming hoort

      if (
        x.page &&
        klantIdentificator.vestigingsnummer &&
        klantIdentificator.kvkNummer
      ) {
        return x.page.filter(
          (p) => p.kvkNummer == klantIdentificator.kvkNummer,
        );
      }

      return x;
    })
    .then(enforceOneOrZero);
}

export async function findKlantByIdentifierOpenKlant2(
  systeemId: string,
  query:
    | {
        bsn: string;
      }
    | {
        vestigingsnummer: string;
        kvkNummer: string;
      }
    | {
        kvkNummer: string;
      },
): Promise<Klant | null> {
  const expand = "digitaleAdressen";

  let soortPartij,
    partijIdentificator__codeSoortObjectId,
    partijIdentificator__objectId;

  if ("bsn" in query) {
    soortPartij = PartijTypes.persoon;
    partijIdentificator__codeSoortObjectId =
      identificatorTypes.persoon.codeSoortObjectId;
    partijIdentificator__objectId = query.bsn;

    const searchParams = new URLSearchParams({
      expand,
      soortPartij,
      partijIdentificator__codeSoortObjectId,
      partijIdentificator__objectId,
    });

    return fetchWithSysteemId(
      systeemId,
      `${klantinteractiesBaseUrl}/partijen?${searchParams}`,
    )
      .then(throwIfNotOk)
      .then(parseJson)
      .then((r) =>
        parsePagination(r, (x) => mapPartijToKlant(systeemId, x as Partij)),
      )
      .then(enforceOneOrZero);
  } else if ("vestigingsnummer" in query) {
    soortPartij = PartijTypes.organisatie;

    partijIdentificator__codeSoortObjectId =
      identificatorTypes.vestiging.codeSoortObjectId;
    partijIdentificator__objectId = query.vestigingsnummer;

    const searchParams = new URLSearchParams({
      expand,
      soortPartij,
      partijIdentificator__codeSoortObjectId,
      partijIdentificator__objectId,
    });

    const partijen = await fetchWithSysteemId(
      systeemId,
      `${klantinteractiesBaseUrl}/partijen?${searchParams}`,
    )
      .then(throwIfNotOk)
      .then(parseJson)
      .then((r) =>
        parsePagination(r, (x) => mapPartijToKlant(systeemId, x as Partij)),
      );

    if (!partijen || partijen.count === 0) {
      return null;
    }

    const matchingPartij = partijen.page?.find(
      (x) => x.kvkNummer === query.kvkNummer,
    );
    if (matchingPartij) {
      return matchingPartij;
    }

    return null;
  } else if ("kvkNummer" in query) {
    soortPartij = PartijTypes.organisatie;
    partijIdentificator__codeSoortObjectId =
      identificatorTypes.nietNatuurlijkPersoonKvkNummer.codeSoortObjectId;
    partijIdentificator__objectId = query.kvkNummer;

    const searchParams = new URLSearchParams({
      expand,
      soortPartij,
      partijIdentificator__codeSoortObjectId,
      partijIdentificator__objectId,
    });

    return await fetchWithSysteemId(
      systeemId,
      `${klantinteractiesBaseUrl}/partijen?${searchParams}`,
    )
      .then(throwIfNotOk)
      .then(parseJson)
      .then((r) =>
        parsePagination(r, (x) => mapPartijToKlant(systeemId, x as Partij)),
      )
      .then(enforceOneOrZero);
  }

  return null;
}

export async function createKlant(
  systeemId: string,
  parameters:
    | {
        bsn: string;
      }
    | {
        vestigingsnummer: string;
        kvkNummer: string;
      }
    | {
        kvkNummer: string;
      },
) {
  let partijIdentificatie, partijIdentificator, soortPartij;

  if ("bsn" in parameters) {
    soortPartij = PartijTypes.persoon;

    // dit is de enige manier om een partij zonder contactnaam aan te maken
    partijIdentificatie = { contactnaam: null };

    partijIdentificator = {
      ...identificatorTypes.persoon,
      objectId: parameters.bsn,
    };
  } else {
    soortPartij = PartijTypes.organisatie;

    // dit is de enige manier om een partij zonder naam aan te maken
    partijIdentificatie = { naam: "" };

    if ("vestigingsnummer" in parameters && parameters.vestigingsnummer) {
      partijIdentificator = {
        ...identificatorTypes.vestiging,
        objectId: parameters.vestigingsnummer,
      };
    } else {
      partijIdentificator = {
        ...identificatorTypes.nietNatuurlijkPersoonKvkNummer,
        objectId: parameters.kvkNummer,
      };
    }
  }

  if (!partijIdentificator) throw new Error("");

  const partij = await createPartij(
    systeemId,
    partijIdentificatie,
    soortPartij,
  );

  const identificatorPayload = {
    identificeerdePartij: {
      url: partij.url,
      uuid: partij.uuid,
    },
    partijIdentificator,
  };

  const identificatoren: PartijIdentificator[] = [];

  // Create / update PartijIdentificatoren
  if ("bsn" in parameters) {
    // natuurlijk_persoon
    identificatoren.push(
      await createPartijIdentificator(systeemId, identificatorPayload),
    );
  } else {
    // vestiging / niet_natuurlijk_persoon

    let kvkIdentificator: PartijIdentificator | null;

    // find kvkIdentificator, maybe null
    kvkIdentificator = await findPartijIdentificator(
      systeemId,
      identificatorTypes.nietNatuurlijkPersoonKvkNummer.codeSoortObjectId,
      parameters.kvkNummer,
    );

    if ("vestigingsnummer" in parameters && parameters.vestigingsnummer) {
      // ensure kvkIdentificator to use for reference from subIdentificatorVan
      if (!kvkIdentificator) {
        kvkIdentificator = await createPartijIdentificator(systeemId, {
          identificeerdePartij: null,
          partijIdentificator: {
            ...identificatorTypes.nietNatuurlijkPersoonKvkNummer,
            objectId: parameters.kvkNummer,
          },
        });
      }

      // create vestigingsIdentificator with subIdentificatorVan
      identificatoren.push(
        await createPartijIdentificator(systeemId, {
          ...identificatorPayload,
          subIdentificatorVan: { uuid: kvkIdentificator.uuid },
        }),
      );
    } else {
      // niet_natuurlijk_persoon

      if (kvkIdentificator) {
        // update kvkIdentificator with identificeerdePartij
        identificatoren.push(
          await updatePartijIdentificator(
            systeemId,
            kvkIdentificator.uuid,
            identificatorPayload,
          ),
        );
      } else {
        // create kvkIdentificator
        identificatoren.push(
          await createPartijIdentificator(systeemId, identificatorPayload),
        );
      }
    }
  }

  return mapPartijToKlant(systeemId, partij, identificatoren);
}

const getPartijIdentificator = (
  systeemId: string,
  uuid: string,
): Promise<PartijIdentificator> =>
  fetchWithSysteemId(
    systeemId,
    klantinteractiesBaseUrl + `/partij-identificatoren/${uuid}`,
  )
    .then(throwIfNotOk)
    .then(parseJson);

const createPartijIdentificator = (
  systeemId: string,
  body: Omit<PartijIdentificator, "uuid">,
): Promise<PartijIdentificator> =>
  fetchWithSysteemId(
    systeemId,
    klantinteractiesBaseUrl + "/partij-identificatoren",
    {
      body: JSON.stringify(body),
      method: "POST",
      headers: {
        "content-type": "application/json",
      },
    },
  )
    .then(throwIfNotOk)
    .then(parseJson);

const updatePartijIdentificator = (
  systeemId: string,
  uuid: string,
  body: Omit<PartijIdentificator, "uuid">,
): Promise<PartijIdentificator> =>
  fetchWithSysteemId(
    systeemId,
    klantinteractiesBaseUrl + `/partij-identificatoren/${uuid}`,
    {
      body: JSON.stringify(body),
      method: "PUT",
      headers: {
        "content-type": "application/json",
      },
    },
  )
    .then(throwIfNotOk)
    .then(parseJson);

const findPartijIdentificator = async (
  systeemId: string,
  partijIdentificatorCodeSoortObjectId: string,
  partijIdentificatorObjectId: string,
): Promise<PartijIdentificator | null> =>
  fetchWithSysteemId(
    systeemId,
    klantinteractiesBaseUrl +
      `/partij-identificatoren?${new URLSearchParams({
        partijIdentificatorCodeSoortObjectId,
        partijIdentificatorObjectId,
      })}`,
  )
    .then(throwIfNotOk)
    .then(parseJson)
    .then((r) => parsePagination(r, (x) => x as PartijIdentificator))
    .then(enforceOneOrZero);

async function createPartij(
  systeemId: string,
  partijIdentificatie: { naam: string } | { contactnaam: Contactnaam | null },
  soortPartij: PartijTypes,
) {
  return fetchWithSysteemId(systeemId, klantinteractiesBaseUrl + "/partijen", {
    body: JSON.stringify({
      digitaleAdressen: [],
      betrokkenen: [],
      categorieRelaties: [],
      voorkeursDigitaalAdres: null,
      vertegenwoordigden: [],
      rekeningnummers: [],
      partijIdentificatoren: [],
      voorkeursRekeningnummer: null,
      indicatieGeheimhouding: false,
      indicatieActief: true,
      partijIdentificatie,
      soortPartij,
    }),
    method: "POST",
    headers: {
      "content-type": "application/json",
    },
  })
    .then(throwIfNotOk)
    .then(parseJson);
}

async function mapPartijToKlant(
  systeemId: string,
  partij: Partij,
  identificatoren?: PartijIdentificator[],
): Promise<Klant> {
  if (!identificatoren?.length) {
    const promises = partij.partijIdentificatoren.map(({ uuid }) =>
      getPartijIdentificator(systeemId, uuid),
    );
    identificatoren = await Promise.all(promises);
  }

  const getDigitaalAdressen = (type: DigitaalAdresTypes) =>
    partij._expand?.digitaleAdressen
      ?.filter((x) => x.adres && x.soortDigitaalAdres === type)
      .map((x) => x.adres || "") || [];

  const getIdentificator = (type: { codeSoortObjectId: CodeSoortObjectId }) =>
    identificatoren?.find(
      (x) =>
        x?.partijIdentificator?.objectId &&
        x?.partijIdentificator?.codeSoortObjectId == type.codeSoortObjectId,
    )?.partijIdentificator?.objectId;

  const getKvkIdentificator = async () => {
    const kvkIdentificatorUuid = identificatoren?.find(
      (x) =>
        x.subIdentificatorVan &&
        x.partijIdentificator.codeSoortObjectId ==
          identificatorTypes.vestiging.codeSoortObjectId,
    )?.subIdentificatorVan?.uuid;

    return kvkIdentificatorUuid
      ? (await getPartijIdentificator(systeemId, kvkIdentificatorUuid))
          ?.partijIdentificator.objectId
      : getIdentificator(identificatorTypes.nietNatuurlijkPersoonKvkNummer);
  };

  const ret: Klant = {
    _typeOfKlant: "klant" as const,
    klantnummer: partij.nummer || "",
    id: partij.uuid,
    url: partij.url,
    bedrijfsnaam: partij.partijIdentificatie?.naam,
    ...(partij.partijIdentificatie?.contactnaam || {}),
    telefoonnummers: getDigitaalAdressen(DigitaalAdresTypes.telefoonnummer),
    emailadressen: getDigitaalAdressen(DigitaalAdresTypes.email),
    bsn: getIdentificator(identificatorTypes.persoon),
    vestigingsnummer: getIdentificator(identificatorTypes.vestiging),
    kvkNummer: await getKvkIdentificator(),
  };

  return ret;
}

export async function useOpenKlant2() {
  // bepaal of de openklant api of de klantinteracties api gebruikt moet worden voor verwerken van contactmomenten en contactverzoeken
  // Fetch USE_KLANTCONTACTEN environment variable, wordt in sommige gevallen vervangen door flow te bepalen op basis van zaken
  const response = await fetch("/api/environment/use-klantinteracties");
  const { useKlantInteracties } = await response.json();
  return useKlantInteracties as boolean;
}

export const postOnderwerpobject = async (
  systeemId: string,
  data: OnderwerpObjectPostModel,
) => {
  const response = await fetchWithSysteemId(
    systeemId,
    `${klantinteractiesBaseUrl}/onderwerpobjecten`,
    {
      method: "POST",
      headers: {
        Accept: "application/json",
        "Content-Type": "application/json",
      },
      body: JSON.stringify(data),
    },
  );
  throwIfNotOk(response);
};

export enum KlantContactExpand {
  gingOverOnderwerpobjecten = "gingOverOnderwerpobjecten",
  hadBetrokkenen = "hadBetrokkenen",
  hadBetrokkenen_digitaleAdressen = "hadBetrokkenen.digitaleAdressen",
  leiddeTotInterneTaken = "leiddeTotInterneTaken",
  omvatteBijlagen = "omvatteBijlagen",
}

export function fetchKlantcontact({
  systeemId,
  expand,
  uuid,
}: {
  systeemId: string;
  uuid: string;
  expand?: KlantContactExpand[];
}) {
  const query = new URLSearchParams();
  expand && query.append("expand", expand.join(","));

  return fetchWithSysteemId(
    systeemId,
    `${klantinteractiesKlantcontacten}/${uuid}?${query}`,
  )
    .then(throwIfNotOk)
    .then(parseJson)
    .then((r) => r as ExpandedKlantContactApiViewmodel);
}

export function fetchKlantcontacten({
  expand,
  systeemIdentifier,
  ...params
}: {
  onderwerpobject__onderwerpobjectidentificatorObjectId?: string;
  hadBetrokkene__uuid?: string;
  systeemIdentifier: string;
  expand?: KlantContactExpand[];
}) {
  const query = new URLSearchParams(
    Object.entries(params).filter(([, value]) => value),
  );
  expand && query.append("expand", expand.join(","));

  return fetchWithSysteemId(
    systeemIdentifier,
    `${klantinteractiesKlantcontacten}?${query}`,
  )
    .then(throwIfNotOk)
    .then(parseJson)
    .then((r) =>
      parsePagination(r, (x) => x as ExpandedKlantContactApiViewmodel),
    );
}

export async function useMedewerkeremail(): Promise<boolean> {
  const response = await fetch("/api/environment/use-medewerkeremail");
  const { useMedewerkeremail } = await response.json();
  return useMedewerkeremail;
}
