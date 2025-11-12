import {
  parseJson,
  parsePagination,
  ResponseError,
  throwIfNotOk,
} from "@/services";
import type {
  Medewerker,
  NatuurlijkPersoon,
  NietNatuurlijkPersoon,
  OrganisatorischeEenheid,
  RolType,
  Vestiging,
  ZaakDetails,
  ZaakDocument,
  ZaakType,
} from "./types";
import {
  registryVersions,
  type Systeem,
} from "@/services/environment/fetch-systemen";
import { fetchWithSysteemId } from "@/services/fetch-with-systeem-id";

const zakenApiPrefix = `/api/zaken`;
const catalogiApiPrefix = `/api/catalogi`;
const documentenApiPrefix = `/api/documenten`;

const combineOverview = (multiple: ZaakDetails[][]) =>
  multiple
    .flat()
    .sort(
      (a, b) => (b.startdatum?.valueOf() || 0) - (a.startdatum?.valueOf() || 0),
    );

const fetchZaakOverview = (systeem: Systeem, query: URLSearchParams) =>
  fetchWithSysteemId(systeem.identifier, `${zakenApiPrefix}/zaken?${query}`)
    .then(throwIfNotOk)
    .then(parseJson)
    .then((json) =>
      parsePagination(json, (x) =>
        mapZaakDetails({ ...(x as any), zaaksysteemId: systeem.identifier }),
      ),
    )
    .then(({ page }) => page);

export function fetchZakenByBsn(systemen: Systeem[], bsn: string) {
  const query = new URLSearchParams([
    ["rol__betrokkeneIdentificatie__natuurlijkPersoon__inpBsn", bsn],
    ["ordering", "-startdatum"],
  ]);
  return Promise.all(
    systemen.map((systeem) => fetchZaakOverview(systeem, query)),
  ).then(combineOverview);
}

export function fetchZakenByZaaknummer(
  systemen: Systeem[],
  zaaknummer: string,
) {
  const query = new URLSearchParams({ identificatie: zaaknummer });
  return Promise.all(systemen.map((s) => fetchZaakOverview(s, query))).then(
    combineOverview,
  );
}

export const fetchZaakDetailsById = (id: string, systeem: Systeem) =>
  fetchWithSysteemId(systeem.identifier, `${zakenApiPrefix}/zaken/${id}`)
    .then(throwIfNotOk)
    .then(parseJson)
    .then((x) => mapZaakDetails({ ...x, zaaksysteemId: systeem.identifier }));

type ZaakBedrijfIdentifier = {
  vestigingsnummer?: string;
  rsin?: string;
  kvkNummer: string;
};

export const fetchZakenByKlantBedrijfIdentifier = (
  systemen: Systeem[],
  id: ZaakBedrijfIdentifier,
) =>
  Promise.all(
    systemen.map((systeem) => fetchZakenForSingleSysteem(systeem, id)),
  ).then(combineOverview);

const fetchZakenForSingleSysteem = async (
  systeem: Systeem,
  id: ZaakBedrijfIdentifier,
): Promise<ZaakDetails[]> => {
  const baseQuery = new URLSearchParams([["ordering", "-startdatum"]]);

  if (isVestigingIdentifier(id)) {
    return fetchZakenByVestiging(systeem, baseQuery, id.vestigingsnummer);
  }

  if (isNietNatuurlijkPersoonIdentifier(id)) {
    return fetchZakenByNietNatuurlijkPersoon(systeem, baseQuery, id);
  }

  // Unsupported identifier type
  return [];
};

const isVestigingIdentifier = (
  id: ZaakBedrijfIdentifier,
): id is ZaakBedrijfIdentifier & { vestigingsnummer: string } => {
  return "vestigingsnummer" in id && !!id.vestigingsnummer;
};

const isNietNatuurlijkPersoonIdentifier = (
  id: ZaakBedrijfIdentifier,
): id is ZaakBedrijfIdentifier & { rsin: string; kvkNummer: string } => {
  return "rsin" in id && !!id.rsin && !!id.kvkNummer;
};

const fetchZakenByVestiging = (
  systeem: Systeem,
  baseQuery: URLSearchParams,
  vestigingsnummer: string,
): Promise<ZaakDetails[]> => {
  const query = new URLSearchParams(baseQuery);
  query.set(
    "rol__betrokkeneIdentificatie__vestiging__vestigingsNummer",
    vestigingsnummer,
  );
  return fetchZaakOverview(systeem, query);
};

const fetchZakenByNietNatuurlijkPersoon = async (
  systeem: Systeem,
  baseQuery: URLSearchParams,
  id: ZaakBedrijfIdentifier & { rsin: string; kvkNummer: string },
): Promise<ZaakDetails[]> => {
  const [zakenByRsin, zakenByInnNnpKvk, zakenByKvkNummer] =
    await fetchZakenByMultipleNnpQueries(systeem, baseQuery, id);

  const filteredKvkResults = zakenByKvkNummer
    ? await filterKvkResultZaken(zakenByKvkNummer, id, systeem, [
        zakenByRsin,
        zakenByInnNnpKvk,
      ])
    : [];

  return [
    ...(zakenByRsin ?? []),
    ...(zakenByInnNnpKvk ?? []),
    ...filteredKvkResults,
  ];
};

const fetchZakenByMultipleNnpQueries = async (
  systeem: Systeem,
  baseQuery: URLSearchParams,
  id: { rsin: string; kvkNummer: string },
): Promise<
  [ZaakDetails[] | null, ZaakDetails[] | null, ZaakDetails[] | null]
> => {
  const rsinQuery = new URLSearchParams(baseQuery);
  rsinQuery.set(
    "rol__betrokkeneIdentificatie__nietNatuurlijkPersoon__innNnpId",
    id.rsin,
  );

  const innNnpKvkQuery = new URLSearchParams(baseQuery);
  innNnpKvkQuery.set(
    "rol__betrokkeneIdentificatie__nietNatuurlijkPersoon__innNnpId",
    id.kvkNummer,
  );

  const kvkQuery = new URLSearchParams(baseQuery);
  kvkQuery.set(
    "rol__betrokkeneIdentificatie__nietNatuurlijkPersoon__kvkNummer",
    id.kvkNummer,
  );

  // This call can create expected bad http requests
  // So wrap each individual fetch to deal with expected exceptions
  return Promise.all([
    handleExpectedError(fetchZaakOverview(systeem, rsinQuery)),
    handleExpectedError(fetchZaakOverview(systeem, innNnpKvkQuery)),
    handleExpectedError(fetchZaakOverview(systeem, kvkQuery)),
  ]);
};

const handleExpectedError = async (promise: Promise<ZaakDetails[]>) => {
  try {
    return await promise;
  } catch (error) {
    if (error instanceof ResponseError && error.response.status === 400) {
      // ignore this kind of http responses
      return null;
    }
    // For all other errors, re-throw to be caught
    throw error;
  }
};

const filterKvkResultZaken = async (
  zaken: ZaakDetails[],
  id: ZaakBedrijfIdentifier,
  systeem: Systeem,
  existingResults: (ZaakDetails[] | null)[],
): Promise<ZaakDetails[]> => {
  const filteredByCompany = await Promise.all(
    zaken.map(async (zaak) => {
      const rollen = await getRollen({
        url: zaak.url,
        zaaksysteemId: systeem.identifier,
      });

      const matchingRoles = rollen.filter(({ betrokkeneIdentificatie }) => {
        if (!betrokkeneIdentificatie) return false;

        // ignore if it is a vestiging
        if (
          "vestigingsNummer" in betrokkeneIdentificatie &&
          betrokkeneIdentificatie.vestigingsNummer
        )
          return false;

        if (
          // ignore if it has a kvkNummer AND
          "kvkNummer" in betrokkeneIdentificatie &&
          betrokkeneIdentificatie.kvkNummer &&
          // the kvkNummer is of another company
          betrokkeneIdentificatie.kvkNummer !== id.kvkNummer
        )
          return false;

        if (
          // ignore if it has a innNnpId AND
          "innNnpId" in betrokkeneIdentificatie &&
          betrokkeneIdentificatie.innNnpId &&
          // the value doesn't match our kvkNummer AND
          betrokkeneIdentificatie.innNnpId !== id.kvkNummer &&
          // the value doesn't match our rsin
          betrokkeneIdentificatie.innNnpId !== id.rsin
        )
          return false;

        // otherwise it is a match
        return true;
      });

      const zaakIsForTheRightCompany = matchingRoles.length > 0;
      return zaakIsForTheRightCompany ? zaak : null;
    }),
  );

  // Filter out nulls and duplicates that are already in existingResults
  return filteredByCompany.filter((kvkZaak): kvkZaak is ZaakDetails => {
    if (!kvkZaak) return false;

    // Check if already in any of the existing results
    return !existingResults.some(
      (existingResult) =>
        existingResult &&
        existingResult.some(
          (existingZaak) => existingZaak.uuid === kvkZaak.uuid,
        ),
    );
  });
};

const getNamePerRoltype = (rollen: Array<RolType> | null, roleNaam: string) => {
  const ONBEKEND = "Onbekend";

  if (!rollen) {
    return ONBEKEND;
  }

  //we gaan er in de interface vanuit dat een rol maar 1 keer voorkomt bij een zaak
  const rol = rollen.find(
    (rol: RolType) => rol.omschrijvingGeneriek === roleNaam,
  );

  if (!rol || !rol.betrokkeneIdentificatie) {
    return ONBEKEND;
  }

  if (rol.betrokkeneType === "natuurlijk_persoon") {
    const x = rol.betrokkeneIdentificatie as NatuurlijkPersoon;
    return [x.voornamen, x.voorvoegselGeslachtsnaam, x.geslachtsnaam]
      .filter(Boolean)
      .join(" ");
  } else if (rol.betrokkeneType === "niet_natuurlijk_persoon") {
    const x = rol.betrokkeneIdentificatie as NietNatuurlijkPersoon;
    return x.statutaireNaam;
  } else if (rol.betrokkeneType === "vestiging") {
    const x = rol.betrokkeneIdentificatie as Vestiging;
    const naam = Array.isArray(x.handelsnaam)
      ? x.handelsnaam.find(Boolean)
      : "";
    return [naam, x.vestigingsNummer].filter(Boolean).join(" ");
  } else if (rol.betrokkeneType === "organisatorische_eenheid") {
    const x = rol.betrokkeneIdentificatie as OrganisatorischeEenheid;
    return x.naam;
  } else if (rol.betrokkeneType === "medewerker") {
    const x = rol.betrokkeneIdentificatie as Medewerker;
    return [x.voorletters, x.voorvoegselAchternaam, x.achternaam]
      .filter(Boolean)
      .join(" ");
  }
  //

  return ONBEKEND;
};

const getStatus = async ({
  status,
  zaaksysteemId,
}: {
  status: string;
  zaaksysteemId: string;
}) => {
  const statusId = status?.split("/")?.pop();

  if (!statusId) return "";

  const statusType = await fetchWithSysteemId(
    zaaksysteemId,
    `${zakenApiPrefix}/statussen/${statusId}`,
  )
    .then(throwIfNotOk)
    .then((x) => x.json())
    .then((json) => json.statustype);

  const statusTypeUuid = statusType?.split("/")?.pop();

  if (!statusTypeUuid) return "";

  const statusOmschrijving = await fetchWithSysteemId(
    zaaksysteemId,
    `${catalogiApiPrefix}/statustypen/${statusTypeUuid}`,
  )
    .then(throwIfNotOk)
    .then((x) => x.json())
    .then((json) => json.omschrijving);

  return statusOmschrijving;
};

export const getDocumenten = async ({
  zaakUrl,
  systeemId,
}: {
  systeemId: string;
  zaakUrl: string;
}): Promise<Array<ZaakDocument>> => {
  const infoObjecten = await fetchWithSysteemId(
    systeemId,
    `${zakenApiPrefix}/zaakinformatieobjecten?${new URLSearchParams({ zaak: zaakUrl })}`,
  )
    .then(throwIfNotOk)
    .then((x) => x.json());

  if (Array.isArray(infoObjecten)) {
    const promises = infoObjecten.map(async (item: any) => {
      const id = item.informatieobject.split("/").pop();

      const docUrl = `${documentenApiPrefix}/enkelvoudiginformatieobjecten/${id}`;
      return fetchWithSysteemId(systeemId, docUrl)
        .then(throwIfNotOk) //todo 404 afvanengen?
        .then((x) => x.json())
        .then((x) => mapDocument(x, docUrl));
    });

    return await Promise.all(promises).then((r) => r.filter((x) => !!x));
  }

  return [];
};

const getRollen = async ({
  url,
  zaaksysteemId,
}: {
  url: string;
  zaaksysteemId: string;
}): Promise<Array<RolType>> => {
  // rollen is een gepagineerd resultaat. we verwachten maar twee rollen.
  // het lijkt extreem onwaarschijnlijk dat er meer dan 1 pagina met rollen zal zijn.
  // we kijken dus (voorlopig) alleen naar de eerste pagina

  let pageIndex = 0;
  const rollen: Array<RolType> = [];
  const rollenUrl = `${zakenApiPrefix}/rollen?zaak=${url}`;

  const getPage = async (url: string) => {
    const page = await fetchWithSysteemId(zaaksysteemId, url)
      .then(throwIfNotOk)
      .then((x) => x.json())
      .then((json) => parsePagination(json, async (x: any) => x as RolType));

    rollen.push(...page.page);
    if (page.next) {
      pageIndex++;
      const nextUrl = `${rollenUrl}&page=${pageIndex}`;
      await getPage(nextUrl);
    }
  };

  await getPage(rollenUrl);

  return rollen;
};

const getZaakType = ({
  zaaktype,
  zaaksysteemId,
}: {
  zaaktype: string;
  zaaksysteemId: string;
}): Promise<ZaakType> => {
  const zaaktypeid = zaaktype.split("/").pop();

  return fetchWithSysteemId(
    zaaksysteemId,
    `${catalogiApiPrefix}/zaaktypen/${zaaktypeid}`,
  )
    .then(throwIfNotOk)
    .then((x) => x.json())
    .then((json) => {
      return json;
    });
};

const mapZaakDetails = async (zaak: {
  uuid: string;
  identificatie: string;
  zaaksysteemId: string;
  zaaktype: string;
  toelichting: string;
  startdatum: string | undefined;
  url: string;
  omschrijving: string;
  status: string;
}) => {
  const zaakzaaktype = await getZaakType(zaak);

  const startdatum = zaak.startdatum ? new Date(zaak.startdatum) : undefined;

  const rollen = await getRollen(zaak);

  return {
    uuid: zaak.uuid,
    identificatie: zaak.identificatie,
    zaaksysteemId: zaak.zaaksysteemId,
    toelichting: zaak.toelichting,
    omschrijving: zaak.omschrijving,
    zaaktypeLabel: zaakzaaktype.onderwerp,
    zaaktypeOmschrijving: zaakzaaktype.omschrijving,
    status: await getStatus(zaak),
    behandelaar: getNamePerRoltype(rollen, "behandelaar"),
    aanvrager: getNamePerRoltype(rollen, "initiator"),
    startdatum,
    url: zaak.url,
  } as ZaakDetails;
};

const mapDocument = (rawDocumenten: any, url: string): ZaakDocument | null => {
  if (!rawDocumenten) return null;

  const doc = {
    id: rawDocumenten.identificatie,
    titel: rawDocumenten.titel,
    bestandsomvang: rawDocumenten.bestandsomvang,
    bestandsnaam: rawDocumenten.bestandsnaam,
    creatiedatum: new Date(rawDocumenten.creatiedatum),
    vertrouwelijkheidaanduiding: rawDocumenten.vertrouwelijkheidaanduiding,
    formaat: rawDocumenten.formaat,
    url,
  };
  return doc;
};
