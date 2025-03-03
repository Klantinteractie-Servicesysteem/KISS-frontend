import {
  fetchLoggedIn,
  enforceOneOrZero,
  parseJson,
  parsePagination,
  throwIfNotOk,
} from "@/services";

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
  type IdentificatorType,
  type Contactnaam,
  type Partij,
  DigitaalAdresTypes,
  type OnderwerpObjectPostModel,
  type Betrokkene,
  CodeSoortObjectId,
  type KlantBedrijfIdentifier,
} from "./types";

import type { ContactverzoekData } from "../../features/contact/components/types";
import type { Klant } from "../openklant/types";
import type { Vraag } from "@/stores/contactmoment";
import { fetchSystemen, registryVersions } from "../environment/fetch-systemen";
import { fetchWithSysteemId } from "../fetch-with-systeem-id";

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

    //we halen alle actoren op en kiezen dan de eerste medewerker. als er geen medewerkers bij staan de erste organisatie
    //wordt naar verwachting tzt aangepast, dan gaan we gewoon alle actoren bij de internetak tonen

    const actorenFetchTasks = actoren.map((actor) =>
      fetchActor(systeemId, actor.uuid),
    );

    const actorenDetails = await Promise.all(actorenFetchTasks);

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
    const fetchTasks = betrokkeneWithKlantcontact.digitaleAdressen.map(
      (digitaalAdres) => {
        const url = `${klantinteractiesDigitaleadressen}/${digitaalAdres.uuid}?`;
        return fetchWithSysteemId(systeemId, url)
          .then(throwIfNotOk)
          .then(parseJson)
          .then((d) => d as DigitaalAdresApiViewModel);
      },
    );

    betrokkeneWithKlantcontact.expandedDigitaleAdressen =
      await Promise.all(fetchTasks);
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
  adres,
  page = 1,
  expand = [],
}: {
  adres: string;
  page: number;
  expand: DigitaleAdressenExpand[];
}) {
  const params = new URLSearchParams({ adres, page: page.toString() });
  expand?.length && params.set("expand", expand.join(","));
  return fetchLoggedIn(klantinteractiesDigitaleadressen + "?" + params)
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
    const actor = await getActorById(systeemId, id);
    if (actor.results.length === 0) {
      return await postActor({
        systeemId: systeemId,
        fullName: name,
        identificatie: id,
        typeOrganisatorischeEenheid: type ?? undefined,
      });
    }
    return actor.results[0].uuid;
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

export async function getActorById(
  systeemId: string,
  identificatie: string,
): Promise<any> {
  const url = `${klantinteractiesActoren}?actoridentificatorObjectId=${identificatie}`;
  const response = await fetchWithSysteemId(systeemId, url, {
    method: "GET",
    headers: {
      Accept: "application/json",
    },
  });

  throwIfNotOk(response);
  return await response.json();
}

function mapActorType(
  typeOrganisatorischeEenheid: "groep" | "afdeling" | undefined,
) {
  switch (typeOrganisatorischeEenheid) {
    case "afdeling":
      return {
        codeObjecttype: "afd",
        codeRegister: "obj",
        codeSoortObjectId: "idf",
        soortActor: "organisatorische_eenheid",
      };
    case "groep":
      return {
        codeObjecttype: "grp",
        codeRegister: "obj",
        codeSoortObjectId: "idf",
        soortActor: "organisatorische_eenheid",
      };
    default:
      return {
        codeObjecttype: "mdw",
        codeRegister: "obj",
        codeSoortObjectId: "idf",
        soortActor: "medewerker",
      };
  }
}

export async function postActor({
  systeemId,
  fullName,
  identificatie,
  typeOrganisatorischeEenheid,
}: {
  fullName: string;
  identificatie: string;
  typeOrganisatorischeEenheid: "afdeling" | "groep" | undefined;
  systeemId: string;
}): Promise<string> {
  const { codeObjecttype, codeRegister, codeSoortObjectId, soortActor } =
    mapActorType(typeOrganisatorischeEenheid);

  const parsedModel = {
    naam: fullName,
    soortActor,
    indicatieActief: true,
    actoridentificator: {
      objectId: identificatie,
      codeObjecttype,
      codeRegister,
      codeSoortObjectId,
    },
  };

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

export const saveKlantContact = async (
  systemIdentifier: string,
  vraag: Vraag,
): Promise<SaveKlantContactResponseModel> => {
  const klantcontactPostModel: KlantContactPostmodel = {
    kanaal: vraag.kanaal,
    onderwerp:
      vraag.vraag?.title === "anders"
        ? vraag.specifiekevraag
        : vraag.vraag
          ? vraag.specifiekevraag
            ? `${vraag.vraag.title} (${vraag.specifiekevraag})`
            : vraag.vraag.title
          : vraag.specifiekevraag,
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
    (await findKlantByIdentifier(systeemId, parameters)) ??
    (await createKlant(systeemId, parameters))
  );
};

export function findKlantByIdentifier(
  systeemId: string,
  query:
    | {
        vestigingsnummer: string;
      }
    | {
        rsin: string;
        kvkNummer?: string;
      }
    | {
        bsn: string;
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
  } else {
    soortPartij = PartijTypes.organisatie;

    if ("vestigingsnummer" in query) {
      partijIdentificator__codeSoortObjectId =
        identificatorTypes.vestiging.codeSoortObjectId;
      partijIdentificator__objectId = query.vestigingsnummer;
    } else if ("rsin" in query) {
      partijIdentificator__codeSoortObjectId =
        identificatorTypes.nietNatuurlijkPersoonRsin.codeSoortObjectId;
      partijIdentificator__objectId = query.rsin;
    } else {
      //toegeovegd om types te alignen.. is dee route wenselijk???

      partijIdentificator__codeSoortObjectId =
        identificatorTypes.nietNatuurlijkPersoonKvkNummer.codeSoortObjectId;
      partijIdentificator__objectId = query.kvkNummer;
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
    .then(enforceOneOrZero);
}

export async function createKlant(
  systeemId: string,
  parameters:
    | {
        vestigingsnummer: string;
      }
    | {
        rsin: string;
        kvkNummer?: string;
      }
    | {
        bsn: string;
      }
    | {
        kvkNummer: string;
      },
) {
  let partijIdentificatie, partijIdentificator, soortPartij, kvkNummer;
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
    } else if ("rsin" in parameters && parameters.rsin) {
      partijIdentificator = {
        ...identificatorTypes.nietNatuurlijkPersoonRsin,
        objectId: parameters.rsin,
      };
      kvkNummer = parameters.kvkNummer;
    }
  }

  if (!partijIdentificator) throw new Error("");

  const partij = await createPartij(
    systeemId,
    partijIdentificatie,
    soortPartij,
  );

  const identificators = [
    await createPartijIdentificator(systeemId, {
      identificeerdePartij: {
        url: partij.url,
        uuid: partij.uuid,
      },
      partijIdentificator,
    }),
  ];

  if (kvkNummer) {
    const kvkIdentificator = await createPartijIdentificator(systeemId, {
      identificeerdePartij: {
        url: partij.url,
        uuid: partij.uuid,
      },
      partijIdentificator: {
        ...identificatorTypes.nietNatuurlijkPersoonKvkNummer,
        objectId: kvkNummer,
      },
    });
    identificators.push(kvkIdentificator);
  }

  return mapPartijToKlant(systeemId, partij, identificators);
}

const createPartijIdentificator = (
  systeemId: string,
  body: {
    identificeerdePartij: {
      url: string;
      uuid: string;
    };
    partijIdentificator: IdentificatorType & {
      objectId: string;
    };
  },
) =>
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

const getPartijIdentificator = (systeemId: string, uuid: string) =>
  fetchWithSysteemId(
    systeemId,
    klantinteractiesBaseUrl + "/partij-identificatoren/" + uuid,
  )
    .then(throwIfNotOk)
    .then(parseJson);

function createPartij(
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
  identificatoren?: any[],
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
    kvkNummer: getIdentificator(
      identificatorTypes.nietNatuurlijkPersoonKvkNummer,
    ),
    rsin: getIdentificator(identificatorTypes.nietNatuurlijkPersoonRsin),
  };

  return ret;
}

/** bepaal of de openklant api of de klantinteracties api gebruikt moet worden voor verwerken van contactmomenten en contactverzoeken
 * @deprecated use fetchSystemen in stead
 */
export const useOpenKlant2 = () =>
  fetchSystemen().then(
    (systemen) =>
      systemen.find((x) => x.isDefault)?.registryVersion ===
      registryVersions.ok2,
  );

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

export { fetchWithSysteemId };
