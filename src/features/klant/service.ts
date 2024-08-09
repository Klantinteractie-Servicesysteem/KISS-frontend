import {
  ServiceResult,
  fetchLoggedIn,
  parsePagination,
  throwIfNotOk,
  parseJson,
  enforceOneOrZero,
  type PaginatedResult,
} from "@/services";
import { mutate } from "swrv";
import type { Ref } from "vue";

import {
  type UpdateContactgegevensParams,
  type Klant,
  KlantType,
} from "./types";
import { nanoid } from "nanoid";
import type { BedrijfIdentifier } from "./bedrijf/types";

const klantinteractiesBaseUrl = "/api/klantinteracties/api/v1";

type IdentificatorType = {
  codeRegister: string;
  codeSoortObjectId: string;
  codeObjecttype: string;
};

type Partij = {
  nummer?: string;
  uuid: string;
  url: string;
  partijIdentificatoren: { uuid: string }[];
  _expand?: {
    digitaleAdressen?: { adres?: string; soortDigitaalAdres?: string }[];
  };
};

// TODO in toekomstige story: waardes overleggen met Maykin en INFO
const identificatorTypes = {
  persoon: {
    codeRegister: "brp",
    codeSoortObjectId: "bsn",
    codeObjecttype: "inp",
  },
  // TODO in PC-341
  // vestiging: {
  //   codeRegister: "KVK",
  //   codeSoortObjectId: "Vestigingsnummer",
  //   codeObjecttype: "Vestiging",
  // },
  // nietNatuurlijkPersoon: {
  //   codeRegister: "KVK",
  //   codeSoortObjectId: "InnNnpId",
  //   codeObjecttype: "INGESCHREVEN NIET-NATUURLIJK PERSOON",
  // },
} as const satisfies Record<string, IdentificatorType>;

const partijTypes = {
  persoon: "persoon",
  organisatie: "organisatie",
  contactpersoon: "contactpersoon",
} as const;

type PartijType = (typeof partijTypes)[keyof typeof partijTypes];

const digitaalAdresTypes = {
  email: "e-mailadres",
  telefoonnummer: "telefoonnummer",
} as const;

type QueryParam = [string, string][];

type FieldParams = {
  email: string;
  telefoonnummer: string;
};

export function createKlantQuery<K extends KlantSearchField>(
  args: KlantSearch<K>,
): KlantSearch<K> {
  return args;
}

export type KlantSearchField = keyof FieldParams;

type QueryDictionary = {
  [K in KlantSearchField]: (search: FieldParams[K]) => QueryParam;
};

const queryDictionary: QueryDictionary = {
  email: (search) => [["emailadres", search]],
  telefoonnummer: (search) => [["telefoonnummer", search]],
};

export type KlantSearch<K extends KlantSearchField> = {
  field: K;
  query: FieldParams[K];
};

function getQueryParams<K extends KlantSearchField>(params: KlantSearch<K>) {
  return queryDictionary[params.field](params.query) as ReturnType<
    QueryDictionary[K]
  >;
}

type KlantSearchParameters<K extends KlantSearchField = KlantSearchField> = {
  query: Ref<KlantSearch<K> | undefined>;
  page: Ref<number | undefined>;
  subjectType?: KlantType;
};

const klantRootUrl = `${document.location.origin}/api/klanten/api/v1/klanten`;

function getKlantSearchUrl<K extends KlantSearchField>(
  search: KlantSearch<K> | undefined,
  subjectType: KlantType,
  page: number | undefined,
) {
  if (!search?.query) return "";

  const url = new URL(klantRootUrl);
  url.searchParams.set("page", page?.toString() ?? "1");
  url.searchParams.append("subjectType", subjectType);

  getQueryParams(search).forEach((tuple) => {
    url.searchParams.set(...tuple);
  });

  return url.toString();
}

function mapKlant(obj: any): Klant {
  const { subjectIdentificatie, url } = obj ?? {};
  const { inpBsn, vestigingsNummer, innNnpId } = subjectIdentificatie ?? {};
  const urlSplit: string[] = url?.split("/") ?? [];

  return {
    ...obj,
    id: urlSplit[urlSplit.length - 1],
    _typeOfKlant: "klant",
    bsn: inpBsn,
    vestigingsnummer: vestigingsNummer,
    url: url,
    nietNatuurlijkPersoonIdentifier: innNnpId,
  };
}

function searchKlanten(url: string): Promise<PaginatedResult<Klant>> {
  return fetchLoggedIn(url)
    .then(throwIfNotOk)
    .then(parseJson)
    .then((j) => parsePagination(j, mapKlant))
    .then((p) => {
      p.page.forEach((klant) => {
        const idUrl = getKlantIdUrl(klant.id);
        if (idUrl) {
          mutate(idUrl, klant);
        }
      });
      return p;
    });
}

function getKlantIdUrl(id?: string) {
  if (!id) return "";
  const url = new URL(`${klantRootUrl}/${id}`);
  return url.toString();
}

const searchSingleKlant = (url: string) =>
  searchKlanten(url).then(enforceOneOrZero);

function fetchKlantById(url: string) {
  return fetchLoggedIn(url).then(throwIfNotOk).then(parseJson).then(mapKlant);
}

export function useKlantById(id: Ref<string>) {
  return ServiceResult.fromFetcher(
    () => getKlantIdUrl(id.value),
    fetchKlantById,
  );
}

const getValidIdentificatie = ({ subjectType, subjectIdentificatie }: any) => {
  if (subjectType === KlantType.Persoon)
    return subjectIdentificatie || { inpBsn: "" };

  const { handelsnaam, ...rest } = subjectIdentificatie ?? {};
  if (Array.isArray(handelsnaam) && handelsnaam.length)
    return subjectIdentificatie;
  return rest;
};

function updateContactgegevens({
  id,
  telefoonnummer,
  emailadres,
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
          telefoonnummer,
          emailadres,
        }),
      }),
    )
    .then(throwIfNotOk)
    .then(parseJson)
    .then(({ telefoonnummer, emailadres }) => ({
      id,
      telefoonnummer,
      emailadres,
    }));
}

export function useSearchKlanten<K extends KlantSearchField>({
  query,
  page,
  subjectType,
}: KlantSearchParameters<K>) {
  const getUrl = () =>
    getKlantSearchUrl(
      query.value,
      subjectType ?? KlantType.Persoon,
      page.value,
    );
  return ServiceResult.fromFetcher(getUrl, searchKlanten);
}

export function useUpdateContactGegevens() {
  return ServiceResult.fromSubmitter(updateContactgegevens);
}

export async function ensureKlantForBsn({ bsn }: { bsn: string }) {
  if (!bsn) throw new Error();

  const klant =
    (await getSingleKlantByIdentificator(
      partijTypes.persoon,
      identificatorTypes.persoon,
      bsn,
    )) ??
    (await createKlant(partijTypes.persoon, identificatorTypes.persoon, bsn));

  const idUrl = getKlantIdUrl(klant.id);
  mutate(idUrl, klant);

  return klant;
}

const getUrlVoorGetKlantById = (
  bedrijfSearchParameter: BedrijfIdentifier | undefined,
) => {
  if (!bedrijfSearchParameter) {
    return "";
  }

  if (
    "vestigingsnummer" in bedrijfSearchParameter &&
    bedrijfSearchParameter.vestigingsnummer
  ) {
    const url = new URL(klantRootUrl);
    url.searchParams.set(
      "subjectVestiging__vestigingsNummer",
      bedrijfSearchParameter.vestigingsnummer,
    );
    url.searchParams.set("subjectType", KlantType.Bedrijf);
    return url.toString();
  }

  if (
    "nietNatuurlijkPersoonIdentifier" in bedrijfSearchParameter &&
    bedrijfSearchParameter.nietNatuurlijkPersoonIdentifier
  ) {
    const url = new URL(klantRootUrl);
    url.searchParams.set(
      "subjectNietNatuurlijkPersoon__innNnpId",
      bedrijfSearchParameter.nietNatuurlijkPersoonIdentifier,
    );
    url.searchParams.set("subjectType", KlantType.NietNatuurlijkPersoon);
    return url.toString();
  }

  return "";
};

export const useKlantByIdentifier = (
  getId: () => BedrijfIdentifier | undefined,
) => {
  const getUrl = () => getUrlVoorGetKlantById(getId());

  const getUniqueId = () => {
    const url = getUrl();
    return url && url + "_single";
  };

  return ServiceResult.fromFetcher(getUrl, searchSingleKlant, {
    getUniqueId,
  });
};

//maak een klant aan in het klanten register als die nog niet bestaat
//bijvoorbeeld om een contactmoment voor een in de kvk opgezocht bedrijf op te kunnen slaan
export async function ensureKlantForBedrijfIdentifier(
  {
    bedrijfsnaam,
    identifier,
  }: {
    bedrijfsnaam: string;
    identifier: BedrijfIdentifier;
  },
  bronorganisatie: string,
) {
  const url = getUrlVoorGetKlantById(identifier);
  const uniqueId = url && url + "_single";

  if (!url || !uniqueId) throw new Error();

  const first = await searchSingleKlant(url);

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
  } else if (
    "nietNatuurlijkPersoonIdentifier" in identifier &&
    identifier.nietNatuurlijkPersoonIdentifier
  ) {
    //als we niet te maken hebben met een vestiging
    //dan gebruiken we afhankelijk van de mogelijkheden van de gerbuite registers
    subjectType = KlantType.NietNatuurlijkPersoon;
    subjectIdentificatie = {
      innNnpId: identifier.nietNatuurlijkPersoonIdentifier,
    };
  }

  if (!subjectType || !subjectIdentificatie) {
    throw new Error("Kan geen klant aanmaken zonder identificatie");
  }

  const response = await fetchLoggedIn(klantRootUrl, {
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
  });

  if (!response.ok) throw new Error();

  const json = await response.json();
  const newKlant = mapKlant(json);
  const idUrl = getKlantIdUrl(newKlant.id);

  mutate(idUrl, newKlant);
  mutate(uniqueId, newKlant);

  return newKlant;
}

const getSingleKlantByIdentificator = (
  soortPartij: PartijType,
  identificatorType: IdentificatorType,
  objectId: string,
) =>
  fetchLoggedIn(
    klantinteractiesBaseUrl +
      "/partijen?" +
      new URLSearchParams({
        soortPartij,
        partijIdentificatorCodeSoortObjectId:
          identificatorType.codeSoortObjectId,
        partijIdentificatorObjectId: objectId,
        expand: "digitaleAdressen",
      }),
  )
    .then(throwIfNotOk)
    .then(parseJson)
    .then((r) => parsePagination(r, (x) => mapPartijToKlant(x as Partij)))
    .then(enforceOneOrZero);

async function mapPartijToKlant(partij: Partij): Promise<Klant> {
  const promises = partij.partijIdentificatoren.map(({ uuid }) =>
    getPartijIdentificator(uuid),
  );
  const identificatoren = await Promise.all(promises);

  const getDigitaalAdres = (type: string) =>
    partij._expand?.digitaleAdressen?.find(
      (x) => x.adres && x.soortDigitaalAdres === type,
    )?.adres;

  const getIdentificator = (type: { codeSoortObjectId: string }) =>
    identificatoren.find(
      (x) =>
        x?.partijIdentificator?.objectId &&
        x?.partijIdentificator?.codeSoortObjectId == type.codeSoortObjectId,
    )?.partijIdentificator?.objectId;

  return {
    _typeOfKlant: "klant",
    klantnummer: partij.nummer || "",
    id: partij.uuid,
    url: partij.url,
    telefoonnummer: getDigitaalAdres(digitaalAdresTypes.telefoonnummer),
    emailadres: getDigitaalAdres(digitaalAdresTypes.email),
    bsn: getIdentificator(identificatorTypes.persoon),
    // TODO in PC-341
    // vestigingsnummer: getIdentificator(identificatorTypes.vestiging),
    // TODO in PC-341
    // nietNatuurlijkPersoonIdentifier: getIdentificator(
    //   identificatorTypes.nietNatuurlijkPersoon,
    // ),
  };
}

const createPartij = (partij: { soortPartij: PartijType }): Promise<Partij> =>
  fetchLoggedIn(klantinteractiesBaseUrl + "/partijen", {
    body: JSON.stringify(partij),
    headers: {
      "content-type": "application/json",
    },
  })
    .then(throwIfNotOk)
    .then(parseJson);

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

const createKlant = (
  soortPartij: PartijType,
  identificatorType: IdentificatorType,
  objectId: string,
) =>
  createPartij({ soortPartij }).then((partij) =>
    createPartijIdentificator({
      identificeerdePartij: {
        url: partij.url,
        uuid: partij.uuid,
      },
      partijIdentificator: {
        ...identificatorType,
        objectId,
      },
    }).then(() => mapPartijToKlant(partij)),
  );
