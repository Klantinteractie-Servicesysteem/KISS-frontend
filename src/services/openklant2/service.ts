import {
  fetchLoggedIn,
  enforceOneOrZero,
  parseJson,
  parsePagination,
  throwIfNotOk,
} from "@/services";

import {
  type ContactmomentViewModel,
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
} from "./types";

import type { ContactverzoekData } from "../../features/contact/components/types";
import type { Klant } from "../openklant/types";
import type { Vraag } from "@/stores/contactmoment"; 

const klantinteractiesProxyRoot = "/api/klantinteracties";
const klantinteractiesApiRoot = "/api/v1";
const klantinteractiesBaseUrl = `${klantinteractiesProxyRoot}${klantinteractiesApiRoot}`;
const klantinteractiesKlantcontacten = `${klantinteractiesBaseUrl}/klantcontacten`;
const klantinteractiesActoren = `${klantinteractiesBaseUrl}/actoren`;
const klantinteractiesDigitaleadressen = `${klantinteractiesBaseUrl}/digitaleadressen`;
const klantinteractiesBetrokkenen = `${klantinteractiesBaseUrl}/betrokkenen`;

////////////////////////////////////////////
// contactmomenten
export function mapKlantContactToContactmomentViewModel(
  klantContact: ExpandedKlantContactApiViewmodel,
) {
  const medewerker = klantContact.hadBetrokkenActoren?.find(
    (x) => x.soortActor === "medewerker",
  );
  const vm: ContactmomentViewModel = {
    url: klantContact.url,
    registratiedatum: klantContact.plaatsgevondenOp,
    kanaal: klantContact?.kanaal,
    tekst: klantContact?.inhoud,
    objectcontactmomenten:
      klantContact._expand?.gingOverOnderwerpobjecten?.map((o) => ({
        objectType: o.onderwerpobjectidentificator.codeObjecttype,
        contactmoment: o.klantcontact.uuid,
        object: o.onderwerpobjectidentificator.objectId,
      })) || [],
    medewerkerIdentificatie: {
      identificatie: medewerker?.actoridentificator?.objectId || "",
      voorletters: "",
      achternaam: medewerker?.naam || "",
      voorvoegselAchternaam: "",
    },
  };
  return vm;
}

////////////////////////////////////////////
// contactmomenten and contactverzoeken
export async function enrichBetrokkeneWithKlantContact(
  value: Betrokkene[],
  expand?: KlantContactExpand[],
): Promise<BetrokkeneMetKlantContact[]> {
  for (const betrokkene of value) {
    const klantContactId = betrokkene.hadKlantcontact?.uuid;
    if (!klantContactId) {
      continue;
    }
    const klantcontact = await fetchKlantcontact({
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

    const actorenFetchTasks = actoren.map((actor) => fetchActor(actor.uuid));

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

export function fetchActor(id: string) {
  return fetchLoggedIn(`${klantinteractiesActoren}/${id}`)
    .then(throwIfNotOk)
    .then(parseJson)
    .then((d) => d as ActorApiViewModel);
}

export async function enrichBetrokkeneWithDigitaleAdressen(
  value: BetrokkeneMetKlantContact[],
): Promise<BetrokkeneMetKlantContact[]> {
  for (const betrokkeneWithKlantcontact of value) {
    const fetchTasks = betrokkeneWithKlantcontact.digitaleAdressen.map(
      (digitaalAdres) => {
        const url = `${klantinteractiesDigitaleadressen}/${digitaalAdres.uuid}?`;
        return fetchLoggedIn(url)
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

export function fetchBetrokkenen(params: { wasPartij__url: string, pageSize: string }) {
  const query = new URLSearchParams(params);
  return fetchLoggedIn(`${klantinteractiesBetrokkenen}?${query}`)
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

export function saveBetrokkene({
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
}): Promise<{ uuid: string }> {
  const voorletters = voornaam ? voornaam.charAt(0) : undefined;

  return fetchLoggedIn(klantinteractiesBetrokkenen, {
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

  const response = await postInternetaak(interneTaak);
  const responseBody = await response.json();

  throwIfNotOk(response);
  return { data: responseBody };
};

const postInternetaak = (data: InternetaakPostModel): Promise<Response> => {
  return fetchLoggedIn(`/api/postinternetaak`, {
    method: "POST",
    headers: {
      Accept: "application/json",
      "Content-Type": "application/json",
    },
    body: JSON.stringify(data),
  });
};

export const ensureActoren = async (
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
    const actor = await getActorById(id);
    if (actor.results.length === 0) {
      return await postActor({
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

export async function getActorById(identificatie: string): Promise<any> {
  const url = `${klantinteractiesActoren}?actoridentificatorObjectId=${identificatie}`;
  const response = await fetchLoggedIn(url, {
    method: "GET",
    headers: {
      Accept: "application/json",
    },
  });

  throwIfNotOk(response);
  return await response.json();
}

 
export type OrganizationalUnitType = "afdeling" | "groep"; 

 
const organizationalUnitMap: Record<OrganizationalUnitType, { codeObjecttype: string; soortActor: string }> = {
  afdeling: { codeObjecttype: "afd", soortActor: "organisatorische_eenheid" },
  groep: { codeObjecttype: "grp", soortActor: "organisatorische_eenheid" }
};

async function mapActor({
  fullName,
  identificatie,
  typeOrganisatorischeEenheid,
}: {
  fullName: string;
  identificatie: string;
  typeOrganisatorischeEenheid: OrganizationalUnitType | undefined;  
}) {
  const medewerkerEmailEnabled = await useMedewerkeremail();  
 
  const codeRegister = medewerkerEmailEnabled ? "handmatig" : "obj";
  const codeSoortObjectId = medewerkerEmailEnabled ? "email" : "idf";
 
  const unitInfo = typeOrganisatorischeEenheid && organizationalUnitMap[typeOrganisatorischeEenheid] || 
                  { codeObjecttype: "mdw", soortActor: "medewerker" };
  
  return {
    naam: fullName,
    soortActor: unitInfo.soortActor,
    indicatieActief: true,
    actoridentificator: {
      objectId: identificatie,
      codeObjecttype: unitInfo.codeObjecttype,
      codeRegister,
      codeSoortObjectId,
    },
  };
}
 
export async function postActor({
  fullName,
  identificatie,
  typeOrganisatorischeEenheid,
}: {
  fullName: string;
  identificatie: string;
  typeOrganisatorischeEenheid: OrganizationalUnitType | undefined;
}): Promise<string> {
  const parsedModel = await mapActor({ fullName, identificatie, typeOrganisatorischeEenheid });

  const response = await fetchLoggedIn(klantinteractiesActoren, {
    method: "POST",
    headers: {
      Accept: "application/json",
      "Content-Type": "application/json"
    },
    body: JSON.stringify(parsedModel),
  });

  throwIfNotOk(response);
  const jsonResponse = await response.json();
  return jsonResponse.uuid;

}

export const saveKlantContact = async (
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

  const response = await postKlantContact(klantcontactPostModel);
  const responseBody = await response.json();

  throwIfNotOk(response);
  return { data: responseBody };
};

const postKlantContact = (data: KlantContactPostmodel): Promise<Response> => {
  return fetchLoggedIn(`/api/postklantcontacten`, {
    method: "POST",
    headers: {
      Accept: "application/json",
      "Content-Type": "application/json",
    },
    body: JSON.stringify(data),
  });
};

export const saveDigitaleAdressen = async (
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

    const savedAdres = await postDigitaalAdres(postBody);
    savedAdressen.push(savedAdres);
  }

  return savedAdressen;
};

const postDigitaalAdres = async (data: {
  verstrektDoorBetrokkene: { uuid: string };
  verstrektDoorPartij?: { uuid: string } | null;
  adres: string;
  soortDigitaalAdres: DigitaalAdresTypes | undefined;
  omschrijving: string;
}): Promise<{ uuid: string; url: string }> => {
  const response = await fetchLoggedIn(klantinteractiesDigitaleadressen, {
    method: "POST",
    headers: {
      Accept: "application/json",
      "Content-Type": "application/json",
    },
    body: JSON.stringify(data),
  });

  const responseBody = await response.json();
  throwIfNotOk(response);
  return { uuid: responseBody.uuid, url: responseBody.url };
};

//-----------------------------------------------------------------------------------------------------------

export const fetchKlantByIdOk2 = (uuid: string) => {
  return fetchLoggedIn(
    `${klantinteractiesBaseUrl}/partijen/${uuid}?${new URLSearchParams({ expand: "digitaleAdressen" })}`,
  )
    .then(throwIfNotOk)
    .then(parseJson)
    .then(mapPartijToKlant);
};

export function findKlantByIdentifier(
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

  return fetchLoggedIn(`${klantinteractiesBaseUrl}/partijen?${searchParams}`)
    .then(throwIfNotOk)
    .then(parseJson)
    .then((r) => parsePagination(r, (x) => mapPartijToKlant(x as Partij)))
    .then(enforceOneOrZero);
}

export async function createKlant(
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

  const partij = await createPartij(partijIdentificatie, soortPartij);

  const identificators = [
    await createPartijIdentificator({
      identificeerdePartij: {
        url: partij.url,
        uuid: partij.uuid,
      },
      partijIdentificator,
    }),
  ];

  if (kvkNummer) {
    const kvkIdentificator = await createPartijIdentificator({
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

  return mapPartijToKlant(partij, identificators);
}

const createPartijIdentificator = (body: {
  identificeerdePartij: {
    url: string;
    uuid: string;
  };
  partijIdentificator: IdentificatorType & {
    objectId: string;
  };
}) =>
  fetchLoggedIn(klantinteractiesBaseUrl + "/partij-identificatoren", {
    body: JSON.stringify(body),
    method: "POST",
    headers: {
      "content-type": "application/json",
    },
  })
    .then(throwIfNotOk)
    .then(parseJson);

const getPartijIdentificator = (uuid: string) =>
  fetchLoggedIn(klantinteractiesBaseUrl + "/partij-identificatoren/" + uuid)
    .then(throwIfNotOk)
    .then(parseJson);

function createPartij(
  partijIdentificatie: { naam: string } | { contactnaam: Contactnaam | null },
  soortPartij: PartijTypes,
) {
  return fetchLoggedIn(klantinteractiesBaseUrl + "/partijen", {
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
  partij: Partij,
  identificatoren?: any[],
): Promise<Klant> {
  if (!identificatoren?.length) {
    const promises = partij.partijIdentificatoren.map(({ uuid }) =>
      getPartijIdentificator(uuid),
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

export function searchKlantenByDigitaalAdres(
  query:
    | {
      telefoonnummer: string;
      partijType: PartijTypes;
    }
    | {
      email: string;
      partijType: PartijTypes;
    },
) {
  let key: DigitaalAdresTypes, value: string;

  if ("telefoonnummer" in query) {
    key = DigitaalAdresTypes.telefoonnummer;
    value = query.telefoonnummer;
  } else {
    key = DigitaalAdresTypes.email;
    value = query.email;
  }

  const searchParams = new URLSearchParams();
  searchParams.append("verstrektDoorPartij__soortPartij", query.partijType);
  searchParams.append("soortDigitaalAdres", key);
  searchParams.append("adres__icontains", value);

  const url = klantinteractiesBaseUrl + "/digitaleadressen?" + searchParams;

  return (
    fetchLoggedIn(url)
      .then(throwIfNotOk)
      .then(parseJson)
      .then(
        ({
          results,
        }: {
          results: { verstrektDoorPartij: { uuid: string } }[];
        }) => {
          const partijIds = results.map((x) => x.verstrektDoorPartij.uuid);
          const uniquePartijIds = [...new Set(partijIds)];
          const promises = uniquePartijIds.map(fetchKlantByIdOk2);
          return Promise.all(promises);
        },
      )
      // TIJDELIJK: de filters werken nog niet in OpenKlant 2.1, dat komt in een nieuwe release
      // daarom filteren we hier handmatig
      .then((klanten) =>
        klanten.filter((klant) => {
          const isBedrijf =
            !!klant.kvkNummer || !!klant.vestigingsnummer || !!klant.rsin;
          if (!isBedrijf) return false;
          const matchesEmail =
            key === DigitaalAdresTypes.email &&
            klant.emailadressen.some((adres: string | string[]) =>
              adres.includes(value),
            );
          const matchesTelefoon =
            key === DigitaalAdresTypes.telefoonnummer &&
            klant.telefoonnummers.some((adres: string | string[]) =>
              adres.includes(value),
            );
          return matchesEmail || matchesTelefoon;
        }),
      )
  );
}

export async function useOpenKlant2() {
  // bepaal of de openklant api of de klantinteracties api gebruikt moet worden voor verwerken van contactmomenten en contactverzoeken
  // Fetch USE_KLANTCONTACTEN environment variable, wordt in sommige gevallen vervangen door flow te bepalen op basis van zaken
  const response = await fetch("/api/environment/use-klantinteracties");
  const { useKlantInteracties } = await response.json();
  return useKlantInteracties as boolean;
}

export const postOnderwerpobject = async (data: OnderwerpObjectPostModel) => {
  const response = await fetchLoggedIn(
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
  expand,
  uuid,
}: {
  uuid: string;
  expand?: KlantContactExpand[];
}) {
  const query = new URLSearchParams();
  expand && query.append("expand", expand.join(","));

  return fetchLoggedIn(`${klantinteractiesKlantcontacten}/${uuid}?${query}`)
    .then(throwIfNotOk)
    .then(parseJson)
    .then((r) => r as ExpandedKlantContactApiViewmodel);
}

export function fetchKlantcontacten({
  expand,
  ...params
}: {
  onderwerpobject__onderwerpobjectidentificatorObjectId?: string;
  hadBetrokkene__uuid?: string;
  expand?: KlantContactExpand[];
} = {}) {
  // Build query
  const query = new URLSearchParams();
  // Add all parameters to the query string
  for (const [key, value] of Object.entries(params)) {
    if (value) {
      query.set(key, value);
    }
  }
  // Add expand (if any)
  if (expand?.length) {
    query.set("expand", expand.join(","));
  }

  return fetchLoggedIn(`${klantinteractiesKlantcontacten}?${query}`)
    .then(throwIfNotOk)
    .then(parseJson)
    .then((r) =>
      parsePagination(r, (x) => x as ExpandedKlantContactApiViewmodel),
    );
}

export async function useMedewerkeremail(): Promise<boolean> {
  const response = await fetch("/api/environment/use-medewerkeremail");
  const { useMedewerkeremail } = await response.json();
  return useMedewerkeremail as boolean;
}
