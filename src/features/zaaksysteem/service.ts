import {
  fetchLoggedIn,
  parsePagination,
  ServiceResult,
  throwIfNotOk,
  type PaginatedResult,
  parseJson,
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
  ZaakPreview,
  ZaakType,
} from "./types";
import type { Ref } from "vue";
import { mutate } from "swrv";
import { toRelativeProxyUrl } from "@/helpers/url";

const zakenProxyRoot = "/api/zaken";

export const useZakenByBsn = (bsn: Ref<string>) => {
  const getUrl = () => {
    if (!bsn.value) return "";
    const url = new URL(location.href);
    url.pathname = zaaksysteemBaseUri + "/zaken";
    url.searchParams.set(
      "rol__betrokkeneIdentificatie__natuurlijkPersoon__inpBsn",
      bsn.value
    );
    return url.toString();
  };

  return ServiceResult.fromFetcher(getUrl, overviewFetcher);
};

export const useZakenPreviewByUrl = (url: Ref<string>) => {
  const getUrl = () => {
    return toRelativeProxyUrl(url.value, zakenProxyRoot) ?? "";
  };

  const fetchPreview = (url: string): Promise<any> =>
    fetchLoggedIn(url)
      .then(throwIfNotOk)
      .then((x) => x.json())
      .then((json) => mapZaakDetailsPreview(json));

  return ServiceResult.fromFetcher(getUrl, fetchPreview, {
    getUniqueId() {
      const url = getUrl();
      return url && `${url}_preview`;
    },
  });
};

export const useZakenByZaaknummer = (zaaknummer: Ref<string>) => {
  const getUrl = () => {
    if (!zaaknummer.value) return "";
    return `${zaaksysteemBaseUri}/zaken?identificatie=${zaaknummer.value}`;
  };
  return ServiceResult.fromFetcher(getUrl, overviewFetcher);
};

const singleZaakFetcher = function fetcher(url: string): Promise<ZaakDetails> {
  return fetchLoggedIn(url)
    .then(throwIfNotOk)
    .then((x) => x.json())
    .then(mapZaakDetails);
};

export const useZaakById = (id: Ref<string>) => {
  const getUrl = () => getZaakUrl(id.value);
  return ServiceResult.fromFetcher(getUrl, singleZaakFetcher);
};

export const useZakenByVestigingsnummer = (vestigingsnummer: Ref<string>) => {
  const getUrl = () => {
    if (!vestigingsnummer.value) return "";
    const url = new URL(location.href);
    url.pathname = zaaksysteemBaseUri + "/rollen";
    url.searchParams.set(
      "betrokkeneIdentificatie__vestiging__vestigingsNummer",
      vestigingsnummer.value
    );
    return url.toString();
  };

  const fetcher = (url: string) =>
    fetchLoggedIn(url)
      .then(throwIfNotOk)
      .then(parseJson)
      .then((r) =>
        parsePagination(r, async (x) => {
          const split = (x as RolType)?.zaak?.split("/");
          if (!Array.isArray(split) || !split.length) {
            throw new Error(
              "kan url van zaak niet ophalen obv rol: " + x && JSON.stringify(x)
            );
          }
          const id = split[split.length - 1];
          const url = getZaakUrl(id);

          const result = await singleZaakFetcher(url);
          mutate(url, result);
          return result;
        })
      );

  return ServiceResult.fromFetcher(getUrl, fetcher);
};

const getNamePerRoltype = (rollen: Array<RolType> | null, roleNaam: string) => {
  const ONBEKEND = "Onbekend";

  if (!rollen) {
    return ONBEKEND;
  }

  //we gaan er in de interface vanuit dat een rol maar 1 keer voorkomt bij een zaak
  const rol = rollen.find(
    (rol: RolType) => rol.omschrijvingGeneriek === roleNaam
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

const getStatus = async (statusUrl: string) => {
  const statusId = statusUrl?.split("/")?.pop();

  if (!statusId) return "";

  const statusType = await fetchLoggedIn(
    `${zaaksysteemBaseUri}/statussen/${statusId}`
  )
    .then(throwIfNotOk)
    .then((x) => x.json())
    .then((json) => json.statustype);

  const statusTypeUuid = statusType?.split("/")?.pop();

  if (!statusTypeUuid) return "";

  const statusTypeUrl = `/api/zaken/catalogi/api/v1/statustypen/${statusTypeUuid}`;

  const statusOmschrijving = await fetchLoggedIn(statusTypeUrl)
    .then(throwIfNotOk)
    .then((x) => x.json())
    .then((json) => json.omschrijving);

  return statusOmschrijving;
};

const getDocumenten = async (
  zaakurl: string
): Promise<Array<ZaakDocument | null>> => {
  const infoObjecten = await fetchLoggedIn(
    `${zaaksysteemBaseUri}/zaakinformatieobjecten?zaak=${zaakurl}`
  )
    .then(throwIfNotOk)
    .then((x) => x.json());

  if (Array.isArray(infoObjecten)) {
    const promises = infoObjecten.map(async (item: any) => {
      const id = item.informatieobject.split("/").pop();

      const docUrl = `${documentenBaseUri}/enkelvoudiginformatieobjecten/${id}`;
      return fetchLoggedIn(docUrl)
        .then(throwIfNotOk) //todo 404 afvanengen?
        .then((x) => x.json())
        .then((x) => mapDocument(x, docUrl));
    });

    return await Promise.all(promises);
  }

  return [];
};

const getRollen = async (zaakurl: string): Promise<Array<RolType>> => {
  // rollen is een gepagineerd resultaat. we verwachten maar twee rollen.
  // het lijkt extreem onwaarschijnlijk dat er meer dan 1 pagina met rollen zal zijn.
  // we kijken dus (voorlopig) alleen naar de eerste pagina

  let pageIndex = 0;
  const rollen: Array<RolType> = [];
  const rollenUrl = `${zaaksysteemBaseUri}/rollen?zaak=${zaakurl}`;

  const getPage = async (url: string) => {
    const page = await fetchLoggedIn(url)
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

const getZaakType = (zaaktype: string): Promise<ZaakType> => {
  const zaaktypeid = zaaktype.split("/").pop();
  const url = `/api/zaken/catalogi/api/v1/zaaktypen/${zaaktypeid}`;

  return fetchLoggedIn(url)
    .then(throwIfNotOk)
    .then((x) => x.json())
    .then((json) => {
      return json;
    });
};

const getZaakUrl = (id: string) => {
  if (!id) return "";
  return `${zaaksysteemBaseUri}/zaken/${id}`;
};

const mapZaakDetails = async (zaak: any) => {
  const zaakzaaktype = await getZaakType(zaak.zaaktype);

  const startdatum = zaak.startdatum ? new Date(zaak.startdatum) : undefined;

  // voorlopig disabled: openzaakbrondata is niet conform de standaard
  // const doorlooptijd = parseInt(
  //   zaakzaaktype.doorlooptijd
  //     ? zaakzaaktype.doorlooptijd.replace(/^\D+/g, "")
  //     : "0",
  //   10
  // );

  // const servicenorm = parseInt(
  //   zaakzaaktype.servicenorm
  //     ? zaakzaaktype.servicenorm.replace(/^\D+/g, "")
  //     : "0",
  //   10
  // );

  // const fataleDatum =
  //   startdatum &&
  //   DateTime.fromJSDate(startdatum)
  //     .plus({
  //       days: isNaN(doorlooptijd) ? 0 : doorlooptijd,
  //     })
  //     .toJSDate();

  // const streefDatum =
  //   startdatum &&
  //   DateTime.fromJSDate(startdatum)
  //     .plus({
  //       days: isNaN(servicenorm) ? 0 : servicenorm,
  //     })
  //     .toJSDate();

  const documenten = await getDocumenten(zaak.url);

  const rollen = await getRollen(zaak.url);

  const id = zaak.url.split("/").pop();

  return {
    ...zaak,
    id: id,
    zaaktype: zaakzaaktype.id,
    zaaktypeLabel: zaakzaaktype.onderwerp,
    zaaktypeOmschrijving: zaakzaaktype.omschrijving,
    status: await getStatus(zaak.status),
    behandelaar: getNamePerRoltype(rollen, "behandelaar"),
    aanvrager: getNamePerRoltype(rollen, "initiator"),
    startdatum,
    // fataleDatum: fataleDatum, voorlopig niet tonen: openzaakbrondata is niet conform de standaard
    // streefDatum: streefDatum, voorlopig niet tonen: openzaakbrondata is niet conform de standaard
    indienDatum: zaak.publicatiedatum && new Date(zaak.publicatiedatum),
    registratieDatum: zaak.registratiedatum && new Date(zaak.registratiedatum),
    self: zaak.url,
    documenten: documenten,
    omschrijving: zaak.omschrijving,
  } as ZaakDetails;
};

const mapZaakDetailsPreview = async (zaak: any) => {
  const zaakzaaktype = await getZaakType(zaak.zaaktype);

  return {
    identificatie: zaak.identificatie,
    zaaktypeLabel: zaakzaaktype.onderwerp,
    status: await getStatus(zaak.status),
  } as ZaakPreview;
};

const zaaksysteemBaseUri = `/api/zaken/zaken/api/v1`;
const documentenBaseUri = `/api/documenten/documenten/api/v1`;

const overviewFetcher = (url: string): Promise<PaginatedResult<ZaakDetails>> =>
  fetchLoggedIn(url)
    .then(throwIfNotOk)
    .then((x) => x.json())
    .then((json) => parsePagination(json, mapZaakDetails))
    .then((zaken) => {
      zaken.page.forEach((zaak) => {
        mutate(getZaakUrl(zaak.id), zaak);
      });
      return zaken;
    });

export async function updateToelichting(
  zaak: ZaakDetails,
  toelichting: string
): Promise<void> {
  const url = getZaakUrl(zaak.id);
  const res = await fetchLoggedIn(url, {
    method: "PATCH",
    headers: {
      Accept: "application/json",
      "Content-Type": "application/json",
    },
    body: JSON.stringify({
      toelichting: toelichting,
    }),
  });

  if (!res.ok)
    throw new Error(`Expected to update toelichting: ${res.status.toString()}`);
}

const mapDocument = (rawDocumenten: any, xx: string): ZaakDocument | null => {
  if (!rawDocumenten) return null;

  const doc = {
    id: rawDocumenten.identificatie,
    titel: rawDocumenten.titel,
    bestandsomvang: rawDocumenten.bestandsomvang,
    creatiedatum: new Date(rawDocumenten.creatiedatum),
    vertrouwelijkheidaanduiding: rawDocumenten.vertrouwelijkheidaanduiding,
    formaat: rawDocumenten.formaat,
    downloadUrl: xx + "/download?versie=1",
  };
  return doc;
};
