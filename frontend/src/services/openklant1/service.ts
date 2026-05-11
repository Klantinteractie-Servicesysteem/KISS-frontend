import {
  enforceOneOrZero,
  fetchLoggedIn,
  parseJson,
  parsePagination,
  throwIfNotOk,
  ServiceResult,
  type PaginatedResult,
} from "@/services";
import { mutate } from "swrv";
import type {
  ContactmomentObject,
  SaveContactmomentResponseModel,
  UpdateContactgegevensParams,
} from "./types";
import { KlantType } from "./types";
import { nanoid } from "nanoid";
import type { BedrijfIdentifier as BedrijfIdentifierOpenKlant1 } from "./types";
import type { KlantBedrijfIdentifier as BedrijfIdentifierOpenKlant2 } from "../openklant2/types.js";
import type {
  Contactmoment,
  Klant,
  KlantIdentificator,
} from "../openklant/types";
import { toRelativeProxyUrl } from "@/helpers/url";
import { fetchWithSysteemId } from "../fetch-with-systeem-id";

const klantenBaseUrl = "/api/klanten/api/v1/klanten";

const contactmomentenProxyRoot = "/api/contactmomenten";
const contactmomentenApiRoot = "/contactmomenten/api/v1";
const contactmomentenBaseUrl = `${contactmomentenProxyRoot}${contactmomentenApiRoot}`;
const objectcontactmomentenUrl = `${contactmomentenBaseUrl}/objectcontactmomenten`;
const klantRootUrl = new URL(document.location.href);
klantRootUrl.pathname = klantenBaseUrl;

function searchKlanten(
  systeemId: string,
  url: string,
): Promise<PaginatedResult<Klant>> {
  return fetchWithSysteemId(systeemId, url)
    .then(throwIfNotOk)
    .then(parseJson)
    .then((j) => parsePagination(j, mapKlant))
    .then((p) => {
      p.page.forEach((klant) => {
        const idUrl = getKlantIdUrl(klant.id);
        if (idUrl) {
          mutate(idUrl, klant);
        }
        const bsnUrl = getUrlVoorPersoon(klant.bsn);

        if (bsnUrl) {
          mutate(bsnUrl, klant);
        }
      });
      return p;
    });
}

function mapKlant(obj: any): Klant {
  const { subjectIdentificatie, url, emailadres, telefoonnummer } = obj ?? {};
  const { inpBsn, vestigingsNummer, innNnpId } = subjectIdentificatie ?? {};
  const urlSplit: string[] = url?.split("/") ?? [];

  return {
    ...obj,
    id: urlSplit[urlSplit.length - 1],
    _typeOfKlant: "klant",
    bsn: inpBsn,
    vestigingsnummer: vestigingsNummer,
    url: url,
    kvkNummer: innNnpId,
    emailadressen: emailadres ? [emailadres] : [],
    telefoonnummers: telefoonnummer ? [telefoonnummer] : [],
  };
}

function getKlantIdUrl(id?: string) {
  if (!id) return "";
  const url = new URL(`${klantRootUrl}/${id}`);
  return url.toString();
}

const searchSingleKlant = (systeemId: string, url: string) =>
  searchKlanten(systeemId, url).then(enforceOneOrZero);

const getSingleBsnSearchId = (bsn: string | undefined) => {
  const url = getUrlVoorPersoon(bsn);
  if (!url) return url;
  return url + "_single";
};

export function fetchKlantByIdOk1(systeemId: string, id: string) {
  const url = getKlantIdUrl(id);
  return fetchWithSysteemId(systeemId, url)
    .then(throwIfNotOk)
    .then(parseJson)
    .then(mapKlant);
}

const getValidIdentificatie = ({ subjectType, subjectIdentificatie }: any) => {
  if (subjectType === KlantType.Persoon)
    return subjectIdentificatie || { inpBsn: "" };

  const { handelsnaam, ...rest } = subjectIdentificatie ?? {};
  if (Array.isArray(handelsnaam) && handelsnaam.length)
    return subjectIdentificatie;
  return rest;
};

//momenteel niet in gebruik
function updateContactgegevens({
  id,
  telefoonnummers,
  emailadressen,
}: UpdateContactgegevensParams): Promise<UpdateContactgegevensParams> {
  const endpoint = klantRootUrl + "/" + id;
  return fetchLoggedIn(endpoint)
    .then(throwIfNotOk)
    .then(parseJson)
    .then((klant) =>
      fetchLoggedIn(endpoint, {
        method: "PUT",
        headers: {
          "Content-Type": "application/json",
        },
        body: JSON.stringify({
          ...klant,
          subjectIdentificatie: getValidIdentificatie(klant),
          telefoonnummers,
          emailadressen,
        }),
      }),
    )
    .then(throwIfNotOk)
    .then(parseJson)
    .then(({ telefoonnummers, emailadressen }) => ({
      id,
      telefoonnummers,
      emailadressen,
    }));
}

export function useUpdateContactGegevens() {
  return ServiceResult.fromSubmitter(updateContactgegevens);
}

export async function ensureOk1Klant(
  systeemId: string,
  {
    bsn,
  }: {
    bsn: string;
  },
  bronorganisatie: string,
) {
  const bsnUrl = getUrlVoorPersoon(bsn);
  const singleBsnId = getSingleBsnSearchId(bsn);

  if (!bsnUrl || !singleBsnId) throw new Error();

  const first = await searchSingleKlant(systeemId, bsnUrl);

  if (first) {
    mutate(singleBsnId, first);
    const idUrl = getKlantIdUrl(first.id);
    mutate(idUrl, first);
    return first;
  }
  const response = await fetchWithSysteemId(systeemId, klantenBaseUrl, {
    method: "POST",
    headers: { "content-type": "application/json" },
    body: JSON.stringify({
      bronorganisatie,
      klantnummer: nanoid(8),
      subjectIdentificatie: { inpBsn: bsn },
      subjectType: KlantType.Persoon,
    }),
  });

  if (!response.ok) throw new Error();

  const json = await response.json();
  const newKlant = mapKlant(json);
  const idUrl = getKlantIdUrl(newKlant.id);

  mutate(idUrl, newKlant);
  mutate(singleBsnId, newKlant);

  return newKlant;
}

//todo vervangen worden door getUrlVoorBedrijf
const getUrlVoorGetKlantById = (
  bedrijfSearchParameter: BedrijfIdentifierOpenKlant1 | undefined,
) => {
  if (!bedrijfSearchParameter) {
    return "";
  }

  const url = new URL(klantRootUrl);

  if (
    "vestigingsnummer" in bedrijfSearchParameter &&
    bedrijfSearchParameter.vestigingsnummer
  ) {
    url.searchParams.set(
      "subjectVestiging__vestigingsNummer",
      bedrijfSearchParameter.vestigingsnummer,
    );
    url.searchParams.set("subjectType", KlantType.Bedrijf);
    return url.toString();
  }
  if (
    "kvkNummer" in bedrijfSearchParameter &&
    bedrijfSearchParameter.kvkNummer
  ) {
    url.searchParams.set(
      "subjectNietNatuurlijkPersoon__innNnpId",
      bedrijfSearchParameter.kvkNummer,
    );
    url.searchParams.set("subjectType", KlantType.NietNatuurlijkPersoon);
    return url.toString();
  }

  return "";
};

export const useKlantByIdentifier = async (
  systeemId: string,
  getId: () => BedrijfIdentifierOpenKlant1 | undefined,
) => {
  const getUrl = () => getUrlVoorGetKlantById(getId());

  return await searchSingleKlant(systeemId, getUrl());
};

export const fetchKlantByKlantIdentificatorOk1 = (
  systeemId: string,
  klantidentificator: KlantIdentificator,
) =>
  searchSingleKlant(
    systeemId,
    klantidentificator.bsn
      ? getUrlVoorPersoon(klantidentificator.bsn)
      : getUrlVoorBedrijf(klantidentificator),
  );

function getUrlVoorPersoon(bsn?: string) {
  if (!bsn) return "";
  const url = new URL(klantRootUrl);
  url.searchParams.set("subjectNatuurlijkPersoon__inpBsn", bsn);
  return url.toString();
}

const getUrlVoorBedrijf = (klantIdentificator: KlantIdentificator) => {
  if (!klantIdentificator) {
    return "";
  }

  const url = new URL(klantRootUrl);

  if (klantIdentificator.vestigingsnummer) {
    url.searchParams.set(
      "subjectVestiging__vestigingsNummer",
      klantIdentificator.vestigingsnummer,
    );
    url.searchParams.set("subjectType", KlantType.Bedrijf);
    return url.toString();
  }

  if (klantIdentificator.kvkNummer) {
    url.searchParams.set(
      "subjectNietNatuurlijkPersoon__innNnpId",
      klantIdentificator.kvkNummer,
    );
    url.searchParams.set("subjectType", KlantType.NietNatuurlijkPersoon);
    return url.toString();
  }

  return "";
};

export function mapBedrijfsIdentifier(
  bedrijfIdentifierOpenKlant2: BedrijfIdentifierOpenKlant2,
): BedrijfIdentifierOpenKlant1 {
  return {
    vestigingsnummer:
      "vestigingsnummer" in bedrijfIdentifierOpenKlant2
        ? bedrijfIdentifierOpenKlant2.vestigingsnummer
        : "",

    //als esuite dan kvk nr gebruiken
    kvkNummer:
      "kvkNummer" in bedrijfIdentifierOpenKlant2
        ? bedrijfIdentifierOpenKlant2.kvkNummer
        : "",
  };
}

//maak een klant aan in het klanten register als die nog niet bestaat
//bijvoorbeeld om een contactmoment voor een in de kvk opgezocht bedrijf op te kunnen slaan
export async function ensureKlantForBedrijfIdentifier(
  systeemId: string,
  {
    bedrijfsnaam,
    identifier,
  }: {
    bedrijfsnaam: string;
    identifier: BedrijfIdentifierOpenKlant1;
  },
  bronorganisatie: string,
) {
  const url = getUrlVoorGetKlantById(identifier);
  const uniqueId = url && url + "_single";

  if (!url || !uniqueId) throw new Error();

  const first = await searchSingleKlant(systeemId, url);

  if (first) {
    mutate(uniqueId, first);
    const idUrl = getKlantIdUrl(first.id);
    mutate(idUrl, first);
    return first;
  }

  let subjectType: KlantType | null = null;
  let subjectIdentificatie = {};
  //afhankelijk van het type 'bedrijf' slaan we andere gegevens op
  if ("vestigingsnummer" in identifier && identifier.vestigingsnummer) {
    subjectType = KlantType.Bedrijf;
    subjectIdentificatie = { vestigingsNummer: identifier.vestigingsnummer };
  } else if ("kvkNummer" in identifier && identifier.kvkNummer) {
    //als we niet te maken hebben met een vestiging
    //dan gebruiken we afhankelijk van de mogelijkheden van de gerbuite registers
    subjectType = KlantType.NietNatuurlijkPersoon;
    subjectIdentificatie = {
      innNnpId: identifier.kvkNummer,
    };
  }

  if (!subjectType || !subjectIdentificatie) {
    throw new Error("Kan geen klant aanmaken zonder identificatie");
  }

  const response = await fetchWithSysteemId(
    systeemId,
    klantRootUrl.toString(),
    {
      method: "POST",
      headers: {
        "content-type": "application/json",
      },
      body: JSON.stringify({
        bronorganisatie,
        // TODO: WAT MOET HIER IN KOMEN?
        klantnummer: nanoid(8),
        subjectIdentificatie: subjectIdentificatie,
        subjectType: subjectType, ///
        bedrijfsnaam,
      }),
    },
  );

  if (!response.ok) throw new Error();

  const json = await response.json();
  const newKlant = mapKlant(json);
  const idUrl = getKlantIdUrl(newKlant.id);

  mutate(idUrl, newKlant);
  mutate(uniqueId, newKlant);

  return newKlant;
}

export function createKlant({
  telefoonnummer = "",
  emailadres = "",
  bronorganisatie = "",
}) {
  if (!bronorganisatie) return Promise.reject();
  if (!telefoonnummer && !emailadres) return Promise.reject();

  return fetchLoggedIn(klantRootUrl, {
    method: "POST",
    headers: {
      "content-type": "application/json",
    },
    body: JSON.stringify({
      bronorganisatie,
      emailadres,
      telefoonnummer,
      // TODO: WAT MOET HIER IN KOMEN?
      klantnummer: nanoid(8),
      subjectType: KlantType.Persoon,
      subjectIdentificatie: { inpBsn: "" },
    }),
  })
    .then(throwIfNotOk)
    .then(parseJson)
    .then(mapKlant)
    .then((newKlant) => {
      const idUrl = getKlantIdUrl(newKlant.id);
      mutate(idUrl, newKlant);
      return newKlant;
    });
}

export const koppelObject = (systeemId: string, data: ContactmomentObject) =>
  fetchWithSysteemId(systeemId, objectcontactmomentenUrl, {
    method: "POST",
    headers: {
      Accept: "application/json",
      "Content-Type": "application/json",
    },
    body: JSON.stringify(data),
  }).then(throwIfNotOk);

const nullForStatusCodes =
  (...statusCodes: number[]) =>
  (r: Response) => {
    if (statusCodes.includes(r.status)) return null;
    throwIfNotOk(r);
    return r;
  };

export async function enrichContactverzoekObjectWithContactmoment(
  systeemId: string,
  contactverzoekObject: any,
) {
  const url = contactverzoekObject.record.data.contactmoment;
  const [contactmoment, details, objects] = await Promise.all([
    fetchContactmomentByUrl(systeemId, url),
    fetchDetailsByUrl(systeemId, url),
    fetchObjectsByContactmomentUrl(systeemId, url),
  ]);

  return {
    contactverzoekObject,
    contactmoment: {
      ...(contactmoment ?? {}),
      objectcontactmomenten:
        // de esuite voegt de objectcontactmomenten wel toe aan een lijst met contacten,
        // maar niet aan een enkel contactmoment. daarom halen we ze hier expliciet op
        contactmoment?.objectcontactmomenten || objects.page,
    },
    details,
  };
}

function fetchContactmomentByUrl(systeemId: string, url: string) {
  const path = toRelativeProxyUrl(url, contactmomentenProxyRoot);
  if (!path) {
    throw new Error();
  }
  return (
    fetchWithSysteemId(
      systeemId,
      `${path}?${new URLSearchParams({ expand: "objectcontactmomenten" })}`,
    )
      // de esuite heeft een ingewikkelde autorisatiestructuur.
      // als je niet geautoriseerd bent voor een specifiek contact,
      // zie je deze netjes in het overzicht maar krijg je een 403 als je het specifieke contact ophaalt.
      // we willen niet dat de hele lijst met contactverzoeken hier op klapt dus geven in dat scenario null terug.
      .then(nullForStatusCodes(404, 403))
      .then((r) => r?.json())
  );
}

function fetchDetailsByUrl(systeemId: string, url: string) {
  return fetchWithSysteemId(
    systeemId,
    `/api/contactmomentdetails?${new URLSearchParams({ id: url })}`,
  )
    .then(nullForStatusCodes(404))
    .then((r) => r?.json());
}

function fetchObjectsByContactmomentUrl(systeemId: string, url: string) {
  return fetchWithSysteemId(
    systeemId,
    `${objectcontactmomentenUrl}?${new URLSearchParams({ contactmoment: url })}`,
  )
    .then((r) => r?.json())
    .then((x) => parsePagination(x, (o) => o as unknown));
}

export const saveContactmoment = async (
  systemIdentifier: string,
  data: Contactmoment,
): Promise<SaveContactmomentResponseModel> => {
  const response = await postContactmoment(systemIdentifier, data);
  const responseBody = await response.json();

  throwIfNotOk(response);
  return { data: responseBody };
};

const postContactmoment = (
  systemIdentifier: string,
  data: Contactmoment,
): Promise<Response> => {
  return fetchWithSysteemId(systemIdentifier, `/api/postcontactmomenten`, {
    method: "POST",
    headers: {
      Accept: "application/json",
      "Content-Type": "application/json",
    },
    body: JSON.stringify(data),
  });
};
