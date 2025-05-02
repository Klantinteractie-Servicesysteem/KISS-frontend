import {
  fetchLoggedIn,
  parseJson,
  throwIfNotOk,
  type Paginated,
  defaultPagination,
  FriendlyError,
} from "@/services";

import type {
  Bedrijf,
  BedrijfIdentifier,
  BedrijfSearchOptions,
  KvkPagination,
  KvkVestiging,
} from "./types";

const zoekenUrl = "/api/kvk/v2/zoeken";
const vestigingsprofielenUrl = "/api/kvk/v1/vestigingsprofielen/";

export async function searchBedrijvenInHandelsRegister(
  query: BedrijfIdentifier | BedrijfSearchOptions,
  page?: number,
) {
  const searchParams = new URLSearchParams();

  if ("vestigingsnummer" in query && query.vestigingsnummer) {
    searchParams.set("vestigingsnummer", query.vestigingsnummer);
    searchParams.set("type", "hoofdvestiging");
    searchParams.append("type", "nevenvestiging");
  } else if ("postcodeHuisnummer" in query) {
    const {
      postcode: { numbers, digits },
      huisnummer,
    } = query.postcodeHuisnummer;
    searchParams.set("postcode", numbers + digits);
    searchParams.set("huisnummer", huisnummer);
  } else if ("kvkNummer" in query && query.kvkNummer) {
    searchParams.set("kvkNummer", query.kvkNummer);
  } else if ("handelsnaam" in query) {
    searchParams.set("naam", query.handelsnaam);
  }

  if (page) {
    searchParams.set("pagina", page.toString());
  }

  const url = `${zoekenUrl}?${searchParams}`;

  return fetchLoggedIn(url).then(async (r) => {
    if (r.status === 404) {
      const body = await r.json();
      if (hasFoutCode(body, "IPD5200")) return defaultPagination([]);
    }
    if (r.status === 400) {
      throw new FriendlyError("Invalide zoekopdracht");
    }
    throwIfNotOk(r);
    const body = await r.json();
    return parseKvkPagination(body);
  });
}

export function searchBedrijvenInHandelsRegisterByVestigingEnKvkNummer(
  vestigingsnummer: string,
  kvkNummer: string,
  page?: number,
) {
  const searchParams = new URLSearchParams();
  searchParams.set("kvkNummer", kvkNummer);
  searchParams.set("vestigingsnummer", vestigingsnummer);
  searchParams.set("type", "hoofdvestiging");
  searchParams.append("type", "nevenvestiging");

  return searchBedrijfInHandelsRegister(page, searchParams);
}

export function searchBedrijvenInHandelsRegisterByRsin(
  rsin: string,
  page?: number,
) {
  const searchParams = new URLSearchParams();

  if (rsin.length === 9) {
    //ok2
    searchParams.set("rsin", rsin);
  } else {
    //e-suite variant van ok1
    searchParams.set("kvkNummer", rsin);
  }

  searchParams.set("type", "rechtspersoon");

  return searchBedrijfInHandelsRegister(page, searchParams);
}

//-----------------------------------------------------------

function searchBedrijfInHandelsRegister(
  page: number | undefined,
  searchParams: URLSearchParams,
) {
  if (page) {
    searchParams.set("pagina", page.toString());
  }

  return searchInHandelsRegister(searchParams);
}

async function searchInHandelsRegister(searchParams: URLSearchParams) {
  const url = `${zoekenUrl}?${searchParams}`;

  return fetchLoggedIn(url).then(async (r) => {
    if (r.status === 404) {
      const body = await r.json();
      if (hasFoutCode(body, "IPD5200")) return defaultPagination([]);
    }
    if (r.status === 400) {
      throw new FriendlyError("Invalide zoekopdracht");
    }
    throwIfNotOk(r);
    const body = await r.json();
    return parseKvkPagination(body);
  });
}

const parseKvkPagination = async ({
  pagina,
  resultatenPerPagina,
  totaal,
  resultaten,
}: KvkPagination): Promise<Paginated<Bedrijf>> => {
  return {
    page: await Promise.all(resultaten.map((x) => mapHandelsRegister(x))),
    pageNumber: pagina,
    totalRecords: totaal,
    pageSize: resultatenPerPagina,
    totalPages: totaal === 0 ? 0 : Math.ceil(totaal / resultatenPerPagina),
  };
};

async function mapHandelsRegister(json: any): Promise<Bedrijf> {
  const { vestigingsnummer, kvkNummer, naam, adres, type } = json ?? {};

  const { binnenlandsAdres, buitenlandsAdres } = adres ?? {};

  const { straatnaam, plaats } = binnenlandsAdres ?? {};

  const { straatHuisnummer, postcodeWoonplaats } = buitenlandsAdres ?? {};

  let vestiging, rsin;

  if (vestigingsnummer) {
    try {
      vestiging = await fetchVestiging(getVestingUrl(vestigingsnummer));
    } catch (e) {
      console.error(e);
    }
  } else {
    // als er geen verstiging is dan gaan we ervan uit dat het een
    // niet natuurlijk persoon betreft waarvan we de RSIN proberen te achterhalen
    try {
      rsin = await fetchRsin(kvkNummer);
    } catch (e) {
      console.error(e);
    }
  }

  const merged = {
    _typeOfKlant: "bedrijf" as const,
    type,
    kvkNummer,
    vestigingsnummer,
    straatnaam: straatnaam || straatHuisnummer,
    woonplaats: plaats || postcodeWoonplaats,
    ...(vestiging ?? {}),
    rsin,
    bedrijfsnaam: naam,
  };

  return merged;
}

////

const fetchVestiging = (url: string) =>
  fetchLoggedIn(url).then(throwIfNotOk).then(parseJson).then(mapVestiging);

const getVestingUrl = (vestigingsnummer?: string) => {
  if (!vestigingsnummer) return "";
  return vestigingsprofielenUrl + vestigingsnummer;
};

const mapVestiging = ({
  vestigingsnummer,
  kvkNummer,
  eersteHandelsnaam,
  adressen,
}: any): KvkVestiging => {
  const { huisnummerToevoeging, postcode, huisnummer, huisletter } =
    adressen?.find((x: any) => x?.type === "bezoekadres") ??
    adressen?.[0] ??
    {};

  return {
    vestigingsnummer,
    kvkNummer,
    handelsnaam: eersteHandelsnaam,
    huisnummertoevoeging: huisnummerToevoeging,
    postcode,
    huisnummer,
    huisletter,
  };
};

const fetchRsin = (kvkNummer: string) =>
  fetchLoggedIn(`/api/kvk/v1/basisprofielen/${kvkNummer}/eigenaar`)
    .then(throwIfNotOk)
    .then(parseJson)
    .then(({ rsin = "" } = {}) => rsin as string);

const hasFoutCode = (body: unknown, code: string) => {
  if (
    body &&
    typeof body === "object" &&
    "fout" in body &&
    Array.isArray(body.fout)
  ) {
    return body.fout.some((x) => x?.code === code);
  }
  return false;
};
