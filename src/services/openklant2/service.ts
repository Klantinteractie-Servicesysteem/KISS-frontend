import {
  fetchLoggedIn,
  enforceOneOrZero,
  parseJson,
  parsePagination,
  throwIfNotOk,
  type PaginatedResult,
} from "@/services";

import {
  type ContactmomentViewModel,
  type BetrokkeneMetKlantContact as BetrokkeneWithKlantContact,
  type ExpandedKlantContactApiViewmodel,
  type ContactverzoekViewmodel,
  type InternetaakApiViewModel,
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
  type onderwerpObjectPostModel,
} from "./types";

import type { ContactverzoekData } from "../../features/contact/components/types";
import type { Klant } from "../openklant/types";
import type { Vraag } from "@/stores/contactmoment";

const klantinteractiesProxyRoot = "/api/klantinteracties";
const klantinteractiesApiRoot = "/api/v1";
const klantinteractiesBaseUrl = `${klantinteractiesProxyRoot}${klantinteractiesApiRoot}`;
const klantinteractiesKlantcontacten = `${klantinteractiesBaseUrl}/klantcontacten`;
const klantinteractiesInterneTaken = `${klantinteractiesBaseUrl}/internetaken`;
const klantinteractiesActoren = `${klantinteractiesBaseUrl}/actoren`;
const klantinteractiesDigitaleadressen = `${klantinteractiesBaseUrl}/digitaleadressen`;
const klantinteractiesBetrokkenen = `${klantinteractiesBaseUrl}/betrokkenen`;

////////////////////////////////////////////
// contactmomenten
export function mapToContactmomentViewModel(
  value: PaginatedResult<BetrokkeneWithKlantContact>,
) {
  const viewmodel = value.page.map((x) => {
    const medewerker = x.klantContact?.hadBetrokkenActoren?.find(
      (x: { soortActor: string }) => x.soortActor === "medewerker",
    );
    return {
      url: x.klantContact.url,
      registratiedatum: x.klantContact?.plaatsgevondenOp,
      kanaal: x.klantContact?.kanaal,
      tekst: x.klantContact?.inhoud,
      objectcontactmomenten: [], //wordt uitgesteld. besproken in https://github.com/Klantinteractie-Servicesysteem/KISS-frontend/issues/800
      medewerkerIdentificatie: {
        identificatie: medewerker?.actorIdentificator?.objectId || "",
        voorletters: "",
        achternaam: medewerker?.naam || "",
        voorvoegselAchternaam: "",
      },
    };
  });

  const paginatedContactenviewmodel: PaginatedResult<ContactmomentViewModel> = {
    next: value.next,
    previous: value.previous,
    count: value.count,
    page: viewmodel,
  };

  return paginatedContactenviewmodel;
}

////////////////////////////////////////////
// contactmomenten and contactverzoeken
export async function enrichBetrokkeneWithKlantContact(
  value: PaginatedResult<BetrokkeneWithKlantContact>,
): Promise<PaginatedResult<BetrokkeneWithKlantContact>> {
  for (const betrokkene of value.page) {
    const searchParams = new URLSearchParams();
    searchParams.set("hadBetrokkene__uuid", betrokkene.uuid);
    const url = `${klantinteractiesKlantcontacten}?${searchParams.toString()}`;

    await fetchLoggedIn(url)
      .then(throwIfNotOk)
      .then(parseJson)
      .then((p) => parsePagination(p, (x) => x))
      .then((d) => {
        if (d.page.length >= 1) {
          betrokkene.klantContact = d
            .page[0] as ExpandedKlantContactApiViewmodel; // er is altijd maar 1 contact bij een betrokkeke!
        }
      });
  }
  return value;
}

// te implementeren bij PC-317 Klantcontacten bij een Zaak tonen
export async function enrichKlantcontactWithZaak(
  value: PaginatedResult<BetrokkeneWithKlantContact>,
): Promise<PaginatedResult<BetrokkeneWithKlantContact>> {
  throw new Error("not implemented (enrichKlantcontactWithZaak)");
  // bij het klantcontact zit een lijst onderwerpobjecten
  // haal daar de zaken uit en voeg deze toe aan
  // "gingOverOnderwerpobjecten": [
  //   {
  //       "uuid": "22c0925d-6b83-4679-b88c-7a8dec7dfda9",
  //       "url": "https://openklant.dev.kiss-demo.nl/klantinteracties/api/v1/onderwerpobjecten/22c0925d-6b83-4679-b88c-7a8dec7dfda9"
  //   }
  // ],
  return value;
}

////////////////////////////////////////////
// contactverzoeken
export async function enrichKlantcontactWithInterneTaak(
  value: PaginatedResult<BetrokkeneWithKlantContact>,
): Promise<PaginatedResult<BetrokkeneWithKlantContact>> {
  const fetchTasks = value.page.map((value) => {
    const searchParams = new URLSearchParams();
    searchParams.set("klantcontact__uuid", value.klantContact.uuid);
    const url = `${klantinteractiesInterneTaken}?${searchParams.toString()}`;
    return fetchLoggedIn(url)
      .then(throwIfNotOk)
      .then(parseJson)
      .then((p) => parsePagination(p, (x) => x))
      .then((d) => {
        if (d.page.length >= 1) {
          value.klantContact.internetaak = d.page[0] as InternetaakApiViewModel; //we mogen er vanuit gaan dat er 1 'hoofd interen tak' is bj een contact moment.
          // het model ondersteunt meerdere vervolg contacten, maar daar houden we binnen kiss nog geen rekening mee.
        }
      });
  });

  await Promise.all(fetchTasks);

  return value;
}

export function filterOutContactmomenten(
  value: PaginatedResult<BetrokkeneWithKlantContact>,
): PaginatedResult<BetrokkeneWithKlantContact> {
  const filtered = value.page.filter((item) => item?.klantContact?.internetaak);
  return {
    next: value.next,
    previous: value.previous,
    count: value.count - value.page.length + filtered.length, //het totaal aantal verminderd met het aantal uitgefilterde items
    page: filtered,
  };
}

export function mapToContactverzoekViewModel(
  value: PaginatedResult<BetrokkeneWithKlantContact>,
): PaginatedResult<ContactverzoekViewmodel> {
  const viewmodel = value.page.map((x) => {
    return {
      url: x.klantContact.internetaak.url,
      medewerker:
        x.klantContact.hadBetrokkenActoren &&
        x.klantContact.hadBetrokkenActoren.length > 0
          ? x.klantContact.hadBetrokkenActoren[0].naam
          : "",
      onderwerp: x.klantContact.onderwerp,
      toelichting: x.klantContact.inhoud,
      record: {
        startAt: x.klantContact.internetaak.toegewezenOp,
        data: {
          status: x.klantContact.internetaak.status,
          contactmoment: x.klantContact.url,
          registratiedatum: x.klantContact.plaatsgevondenOp,
          datumVerwerkt: x.klantContact.internetaak.afgehandeldOp,
          toelichting: x.klantContact.internetaak.toelichting,
          actor: {
            naam: x.klantContact.internetaak?.actor?.naam,
            soortActor: x.klantContact.internetaak?.actor?.soortActor,
            identificatie: "",
          },

          betrokkene: {
            rol: "klant",
            persoonsnaam: x.contactnaam,
            digitaleAdressen: x.digitaleAdressenExpanded,
          },

          verantwoordelijkeAfdeling: "", //todo: waar komt dit vandaan?
        },
      },
    } as ContactverzoekViewmodel;
  });

  const paginatedContactenviewmodel: PaginatedResult<ContactverzoekViewmodel> =
    {
      next: value.next,
      previous: value.previous,
      count: value.count,
      page: viewmodel,
    };

  return paginatedContactenviewmodel;
}

export async function enrichInterneTakenWithActoren(
  value: PaginatedResult<BetrokkeneWithKlantContact>,
): Promise<PaginatedResult<BetrokkeneWithKlantContact>> {
  for (const betrokkeneWithKlantcontact of value.page) {
    const actoren =
      betrokkeneWithKlantcontact?.klantContact?.internetaak
        ?.toegewezenAanActoren;

    //we halen alle actoren op en kiezen dan de eerste medewerker. als er geen medewerkers bij staan de erste organisatie
    //wordt naar verwachting tzt aangepast, dan gaan we gewoon alle actoren bij de internetak tonen
    const actorenDetails: Array<ActorApiViewModel> = [];

    const actorenFetchTasks = actoren.map((actor) => {
      const url = `${klantinteractiesActoren}/${actor.uuid}`;
      return fetchLoggedIn(url)
        .then(throwIfNotOk)
        .then(parseJson)
        .then((d) => {
          actorenDetails.push(d as ActorApiViewModel);
        });
    });

    await Promise.all(actorenFetchTasks);

    const medewerkerActor = actorenDetails.find(
      (x) => x.soortActor === "medewerker",
    );
    if (medewerkerActor) {
      betrokkeneWithKlantcontact.klantContact.internetaak.actor =
        medewerkerActor as ActorApiViewModel;
    } else {
      const organisatorischerEenheidActor = actorenDetails.find(
        (x) => x.soortActor === "organisatorische_eenheid",
      );
      betrokkeneWithKlantcontact.klantContact.internetaak.actor =
        organisatorischerEenheidActor as ActorApiViewModel;
    }
  }

  return value;
}

export async function enrichBetrokkeneWithDigitaleAdressen(
  value: PaginatedResult<BetrokkeneWithKlantContact>,
): Promise<PaginatedResult<BetrokkeneWithKlantContact>> {
  for (const betrokkeneWithKlantcontact of value.page) {
    betrokkeneWithKlantcontact.digitaleAdressenExpanded = [];
    const digitaleAdressen = betrokkeneWithKlantcontact?.digitaleAdressen;

    const fetchTasks = digitaleAdressen.map((digitaalAdres) => {
      const url = `${klantinteractiesDigitaleadressen}/${digitaalAdres.uuid}?`;
      return fetchLoggedIn(url)
        .then(throwIfNotOk)
        .then(parseJson)
        .then(async (d) => {
          betrokkeneWithKlantcontact.digitaleAdressenExpanded.push({
            adres: d.adres,
            soortDigitaalAdres: d.soortDigitaalAdres,
            omschrijving: d.omschrijving,
          });
        });
    });

    await Promise.all(fetchTasks);
  }

  return value;
}

export function fetchBetrokkene(url: string) {
  return fetchLoggedIn(url)
    .then(throwIfNotOk)
    .then(parseJson)
    .then((p) => parsePagination(p, (x) => x as BetrokkeneWithKlantContact));
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
  fullName,
  identificatie,
  typeOrganisatorischeEenheid,
}: {
  fullName: string;
  identificatie: string;
  typeOrganisatorischeEenheid: "afdeling" | "groep" | undefined;
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

  const response = await fetchLoggedIn(klantinteractiesActoren, {
    method: "POST",
    headers: {
      Accept: "application/json",
      "Content-Type": "application/json",
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
      soortDigitaalAdres:
        adres.soortDigitaalAdres === "telefoonnummer" ? "telnr" : "email",
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
  soortDigitaalAdres: string;
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

  const getIdentificator = (type: { codeSoortObjectId: string }) =>
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

export const postOnderwerpobject = async (data: onderwerpObjectPostModel) => {
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
