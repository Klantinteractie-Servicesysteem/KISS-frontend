import { DateTime } from "luxon";
import {
  fetchLoggedIn,
  parsePagination,
  ServiceResult,
  throwIfNotOk,
  type Paginated,
  type ServiceData,
} from "@/services";
import type { Zaak } from "./types";
import type { ZaakDetails } from "./types";

type Roltype = "behandelaar" | "initiator";

const getNamePerRoltype = (zaak: any, roltype: Roltype): string => {
  const behandelaar = zaak.embedded.rollen.find(
    (rol: any) =>
      rol.betrokkeneType === "medewerker" &&
      rol.omschrijvingGeneriek === roltype
  );

  const identificatie = behandelaar?.embedded?.betrokkeneIdentificatie;

  if (!identificatie) return "Onbekend";

  const voornaam = identificatie.voornamen ?? "";
  const tussenvoegsel = identificatie.voorvoegselGeslachtsnaam ?? "";
  const achternaam = identificatie.geslachtsnaam ?? "";

  return `${voornaam} ${tussenvoegsel} ${achternaam}`;
};

function parseZaak(zaak: any): Zaak {
  const startdatum = new Date(zaak.startdatum);
  const fataleDatum = DateTime.fromJSDate(startdatum)
    .plus({
      days: parseInt(zaak.embedded.zaaktype.doorlooptijd, 10),
    })
    .toJSDate();

  return {
    identificatie: zaak.identificatie,
    id: zaak.id,
    startdatum,
    url: zaak.url,
    zaaktype: zaak.embedded.zaaktype.omschrijving,
    registratiedatum: startdatum,
    status: zaak.embedded.status.statustoelichting,
    fataleDatum,
    behandelaar: getNamePerRoltype(zaak, "behandelaar"),
    toelichting: zaak.toelichting,
  };
}

export function useZaaksysteemService() {
  if (!window.gatewayBaseUri) {
    console.error("gatewayBaseUri missing");
  }

  const zaaksysteemBaseUri = `${window.gatewayBaseUri}/api/zaken`;

  const findByZaak = (zaaknummer: string) => {
    const url = `${zaaksysteemBaseUri}?identificatie=${zaaknummer}&extend[]=all`;
    return fetchLoggedIn(url)
      .then(throwIfNotOk)
      .then((x) => x.json())
      .then((json) => parsePagination(json, parseZaak));
  };

  const findByBsn = (bsn: string) => {
    const getFindByBsnURL = () => {
      if (!bsn) return "";

      return `${zaaksysteemBaseUri}?rollen.betrokkeneIdentificatie.inpBsn=${bsn}&extend[]=zaaktype`;
    };

    const getZaakByBsn = (url: string): Promise<Paginated<Zaak>> =>
      fetchLoggedIn(url)
        .then(throwIfNotOk)
        .then((x) => x.json())
        .then((json) => parsePagination(json, parseZaak));

    const withoutFetcher = () => getZaakByBsn(getFindByBsnURL());

    const withFetcher = () =>
      ServiceResult.fromFetcher(getFindByBsnURL, getZaakByBsn);

    return { withoutFetcher, withFetcher };
  };

  const getZaak = (id: string): ServiceData<ZaakDetails> => {
    function get(url: string): Promise<ZaakDetails> {
      return fetchLoggedIn(url)
        .then(throwIfNotOk)
        .then((x) => x.json())
        .then((zaak) => {
          const startdatum = new Date(zaak.startdatum);
          const fataleDatum = DateTime.fromJSDate(startdatum)
            .plus({
              days: parseInt(zaak.embedded.zaaktype.doorlooptijd, 10),
            })
            .toJSDate();
          const streefDatum = DateTime.fromJSDate(startdatum)
            .plus({
              days: parseInt(zaak.embedded.zaaktype.servicenorm, 10),
            })
            .toJSDate();

          return {
            ...zaak,
            zaaktype: zaak.embedded.zaaktype.id,
            zaaktypeLabel: zaak.embedded.zaaktype.onderwerp,
            status: zaak.embedded.status.statustoelichting,
            behandelaar: getNamePerRoltype(zaak, "behandelaar"),
            aanvrager: getNamePerRoltype(zaak, "initiator"),
            startdatum: startdatum,
            fataleDatum: fataleDatum,
            streefDatum: streefDatum,
            indienDatum: zaak.publicatiedatum ?? "Onbekend",
            registratieDatum: new Date(zaak.registratiedatum),
          } as ZaakDetails;
        });
    }

    return ServiceResult.fromFetcher(
      `${zaaksysteemBaseUri}/${id}?extend[]=all`,
      get
    );
  };

  return {
    findByZaak,
    findByBsn,
    getZaak,
  };
}

export async function updateToelichting(
  zaak: ZaakDetails,
  toelichting: string
): Promise<Zaak> {
  const url = `${window.gatewayBaseUri}/api/zaken/${zaak.id}`;
  const res = await fetchLoggedIn(url, {
    method: "PUT",
    headers: {
      Accept: "application/json",
      "Content-Type": "application/json",
    },
    body: JSON.stringify({
      bronorganisatie: zaak.bronorganisatie,
      startdatum: zaak.startdatum,
      verantwoordelijkeOrganisatie: zaak.verantwoordelijkeOrganisatie,
      zaaktype: zaak.zaaktype,
      toelichting: toelichting,
    }),
  });

  if (!res.ok)
    throw new Error(`Expected to update toelichting: ${res.status.toString()}`);

  return res.json();
}
