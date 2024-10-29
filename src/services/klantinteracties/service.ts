import {
  fetchLoggedIn,
  parseJson,
  parsePagination,
  throwIfNotOk,
  type PaginatedResult,
} from "@/services";

import type {
  ContactmomentViewModel,
  BetrokkeneMetKlantContact as BetrokkeneWithKlantContact,
  ExpandedKlantContactApiViewmodel,
  ContactverzoekViewmodel,
  InternetaakApiViewModel,
  ActorApiViewModel,
  InternetaakPostModel,
  SaveInterneTaakResponseModel,
  KlantContactPostmodel,
  SaveKlantContactResponseModel,
  DigitaalAdresApiViewModel
} from "./types";

import type { Contactmoment } from "../../features/contact/contactmoment/types";
import type { ContactverzoekData } from "../../features/contact/components/types";

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
            wasPartij: x.wasPartij
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
      }
    }),
  })
    .then(throwIfNotOk)
    .then((response) => response.json()) 
    .then((data) => ({
      uuid: data.uuid, 
    }));
}

export const saveInternetaak = async (
  data: InternetaakPostModel,
): Promise<SaveInterneTaakResponseModel> => {
  const response = await postInternetaak(data); 
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

export const mapContactmomentToInternetaak = (
  contactmoment: Contactmoment, 
): InternetaakPostModel => {

  return {
    nummer: "",  
    gevraagdeHandeling: "Contact opnemen met betrokkene", 
    aanleidinggevendKlantcontact: {
      uuid: contactmoment.uuid
    },
    toegewezenAanActoren: [], 
    toelichting: contactmoment.toelichting,  
    status: "te_verwerken"
  };
};

export const enrichInterneTaakWithActoren = async (
  interneTaak: InternetaakPostModel,
  actorData: ContactverzoekData["actor"]
) => {
  const actoren = await ensureActoren(actorData);

  actoren.forEach((actor) => {
    interneTaak.toegewezenAanActoren.push(actor);
  });
};

const ensureActoren = async (actorData: ContactverzoekData["actor"]) => {
  const {
    identificatie,
    naam,
    typeOrganisatorischeEenheid,
    naamOrganisatorischeEenheid,
    identificatieOrganisatorischeEenheid,
  } = actorData;

  const getOrCreateActor = async (name: string, id: string, type?: "afdeling" | "groep" | undefined) => {
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

  const actoren = [];

  // als zowel een afdeling/groep als medewerker is geselecteerd
  if (naamOrganisatorischeEenheid && identificatieOrganisatorischeEenheid) {
    const actorUuid = await getOrCreateActor(naam, identificatie, undefined); // medewerker zonder organisatorische eenheid
    const organisatorischeActorUuid = await getOrCreateActor(
      naamOrganisatorischeEenheid,
      identificatieOrganisatorischeEenheid,
      typeOrganisatorischeEenheid
    );
    if (actorUuid) actoren.push({ uuid: actorUuid });
    if (organisatorischeActorUuid) actoren.push({ uuid: organisatorischeActorUuid });
  } else {
    // als alleen een afdeling/groep is geselecteerd
    const actorUuid = await getOrCreateActor(naam, identificatie, typeOrganisatorischeEenheid);
    if (actorUuid) actoren.push({ uuid: actorUuid });
  }

  return actoren;
};

export async function getActorById(identificatie: string): Promise<any> {  const url = `${klantinteractiesActoren}?actoridentificatorObjectId=${identificatie}`;
  const response = await fetchLoggedIn(url, {
    method: "GET",
    headers: {
      Accept: "application/json",
    },
  });

  throwIfNotOk(response); 
  return await response.json(); 
}

function mapActorType(typeOrganisatorischeEenheid: "groep" | "afdeling" | undefined) {
  switch (typeOrganisatorischeEenheid) {
    case "afdeling":
      return { codeObjecttype: "afd", codeRegister: "obj", codeSoortObjectId: "idf", soortActor: "organisatorische_eenheid" };
    case "groep":
      return { codeObjecttype: "grp", codeRegister: "obj", codeSoortObjectId: "idf", soortActor: "organisatorische_eenheid" };
    default:
      return { codeObjecttype: "mdw", codeRegister: "obj", codeSoortObjectId: "idf", soortActor: "medewerker" };
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
  const { codeObjecttype, codeRegister, codeSoortObjectId, soortActor } = mapActorType(typeOrganisatorischeEenheid);

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
  data: KlantContactPostmodel,
): Promise<SaveKlantContactResponseModel> => {
  const response = await postKlantContact(data);
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
  verstrektDoorPartijUuid?: string,
): Promise<Array<{ uuid: string; url: string }>> => {
  const savedAdressen: Array<{ uuid: string; url: string }> = [];

  for (const adres of digitaleAdressen) {
    const postBody = {
      verstrektDoorBetrokkene: { uuid: verstrektDoorBetrokkeneUuid },
      verstrektDoorPartij: null, 
      adres: adres.adres,
      soortDigitaalAdres: adres.soortDigitaalAdres === 'telefoonnummer'
        ? 'telnr'
        : 'email', 
      omschrijving: adres.omschrijving || "onbekend", 
    };

    const savedAdres = await postDigitaalAdres(postBody);
    savedAdressen.push(savedAdres);
  }

  return savedAdressen;
};

const postDigitaalAdres = async (
  data: {
    verstrektDoorBetrokkene: { uuid: string };
    verstrektDoorPartij?: { uuid: string } | null;
    adres: string;
    soortDigitaalAdres: string;
    omschrijving: string;
  }
): Promise<{ uuid: string; url: string }> => {
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