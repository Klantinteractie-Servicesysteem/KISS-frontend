import {
  throwIfNotOk,
  ServiceResult,
  parsePagination,
  parseJson,
} from "@/services";
import { fetchLoggedIn } from "@/services";
import type { Ref } from "vue";

import {
  type Gespreksresultaat,
  type ContactmomentObject,
  type Contactmoment,
  type ZaakContactmoment,
  type ObjectContactmoment,
  type ContactmomentDetails,
  type SaveContactmomentResponseModel,
} from "./types";

import { toRelativeProxyUrl } from "@/helpers/url";

import { formatIsoDate } from "@/helpers/date";
import {
  ActorType,
  type ContactmomentContactVerzoek,
} from "@/stores/contactmoment";

import {
  TypeOrganisatorischeEenheid,
  type Vraag,
  type ContactverzoekData,
  type DigitaalAdres,
  type NewContactverzoek,
} from "../components/types";
import {
  isCheckboxVraag,
  isDropdownVraag,
  isInputVraag,
  isTextareaVraag,
} from "../components/service";
import {
  enrichBetrokkeneWithKlantContact,
  enrichKlantcontactWithInterneTaak,
  fetchBetrokkene,
  mapToContactmomentViewModel,
  type ContactmomentViewModel,
} from "@/services/openklant2";

//obsolete. api calls altijd vanuit /src/services of /src/apis. hier alleen nog busniesslogica afhandelen
const contactmomentenProxyRoot = "/api/contactmomenten";
const contactmomentenApiRoot = "/contactmomenten/api/v1";
const contactmomentenBaseUrl = `${contactmomentenProxyRoot}${contactmomentenApiRoot}`;
const contactmomentDetails = "/api/contactmomentdetails";
const objectcontactmomentenUrl = `${contactmomentenBaseUrl}/objectcontactmomenten`;
const contactmomentenUrl = `${contactmomentenBaseUrl}/contactmomenten`;
const klantcontactmomentenUrl = `${contactmomentenBaseUrl}/klantcontactmomenten`;

const zaaksysteemProxyRoot = `/api/zaken`;
const zaaksysteemApiRoot = `/zaken/api/v1`;
const zaaksysteemBaseUri = `${zaaksysteemProxyRoot}${zaaksysteemApiRoot}`;
const zaakcontactmomentUrl = `${zaaksysteemBaseUri}/zaakcontactmomenten`;

const klantinteractiesProxyRoot = "/api/klantinteracties";
const klantinteractiesApiRoot = "/api/v1";
const klantinteractiesBaseUrl = `${klantinteractiesProxyRoot}${klantinteractiesApiRoot}`;
const klantinteractiesBetrokkenen = `${klantinteractiesBaseUrl}/betrokkenen`;

export const saveContactmoment = async (
  data: Contactmoment,
): Promise<SaveContactmomentResponseModel> => {
  const response = await postContactmoment(data);
  const responseBody = await response.json();

  throwIfNotOk(response);
  return { data: responseBody };
};

const postContactmoment = (data: Contactmoment): Promise<Response> => {
  return fetchLoggedIn(`/api/postcontactmomenten`, {
    method: "POST",
    headers: {
      Accept: "application/json",
      "Content-Type": "application/json",
    },
    body: JSON.stringify(data),
  });
};

export const CONTACTVERZOEK_GEMAAKT = "Contactverzoek gemaakt";

export const useGespreksResultaten = () => {
  const fetchBerichten = (url: string) =>
    fetchLoggedIn(url)
      .then((r) => {
        if (!r.ok) {
          throw new Error(
            "Er is een fout opgetreden bij het laden van de gespreksresultaten",
          );
        }
        return r.json();
      })
      .then((results: Array<Gespreksresultaat>) => {
        if (!Array.isArray(results))
          throw new Error("unexpected json result: " + JSON.stringify(results));
        const hasContactverzoekResultaat = results.some(
          ({ definitie }) => definitie === CONTACTVERZOEK_GEMAAKT,
        );
        if (!hasContactverzoekResultaat) {
          results.push({
            definitie: CONTACTVERZOEK_GEMAAKT,
          });
          results.sort((a, b) => a.definitie.localeCompare(b.definitie));
        }
        return results;
      });

  return ServiceResult.fromFetcher("/api/gespreksresultaten", fetchBerichten);
};

export const koppelObject = (data: ContactmomentObject) =>
  fetchLoggedIn(objectcontactmomentenUrl, {
    method: "POST",
    headers: {
      Accept: "application/json",
      "Content-Type": "application/json",
    },
    body: JSON.stringify(data),
  }).then(throwIfNotOk);

export const koppelZaakContactmoment = ({
  zaaksysteemId,
  url,
  contactmoment,
}: ZaakContactmoment) =>
  fetchLoggedIn(zaakcontactmomentUrl, {
    method: "POST",
    headers: {
      Accept: "application/json",
      "Content-Type": "application/json",
    },
    body: JSON.stringify({ zaaksysteemId, url, contactmoment }),
  }).then(throwIfNotOk);

export function koppelKlant({
  klantId,
  contactmomentId,
}: {
  klantId: string;
  contactmomentId: string;
}) {
  return fetchLoggedIn(klantcontactmomentenUrl, {
    method: "POST",
    headers: {
      "Content-Type": "application/json",
    },
    body: JSON.stringify({
      klant: klantId,
      contactmoment: contactmomentId,
      rol: "gesprekspartner",
    }),
  }).then(throwIfNotOk) as Promise<void>;
}

export function koppelBetrokkene({
  partijId,
  contactmomentId,
}: {
  partijId: string;
  contactmomentId: string;
}) {
  return fetchLoggedIn(klantinteractiesBetrokkenen, {
    method: "POST",
    headers: {
      "Content-Type": "application/json",
    },
    body: JSON.stringify({
      wasPartij: {
        uuid: partijId,
      },
      hadKlantcontact: {
        uuid: contactmomentId,
      },
      rol: "klant",
      initiator: true,
    }),
  }).then(throwIfNotOk) as Promise<void>;
}

export function useContactmomentenByKlantId(
  id: Ref<string>,
  gebruikKlantinteractiesApi: Ref<boolean | null>,
) {
  //een cackekey is nodig anders wordt alleen de CM's OF de CV's opgehaald
  //ze beginnen namelijk met dezelfde call naar partij
  //als die hetzelfde is dan wordt die uit de cache gehaald

  //om te voorkomen dat er al data opgehaald wordt voordat de juiste route bekend is, moet de cachekey een lege string retourneren
  //als er geen cachekey gebruikt wordt, moet de url een lege string retourneren
  const getCacheKey = () =>
    gebruikKlantinteractiesApi.value === null
      ? ""
      : id.value
        ? `${id.value}_contactmoment`
        : "";

  const fetchContactmomenten = async (
    url: string,
    gebruikKlantinteractiesApi: Ref<boolean | null>,
  ) => {
    if (gebruikKlantinteractiesApi.value === null) {
      return { count: null, page: [] };
    }

    if (gebruikKlantinteractiesApi.value) {
      return fetchBetrokkene(url)
        .then(enrichBetrokkeneWithKlantContact)
        .then(enrichKlantcontactWithInterneTaak) //necesarry to filter them out
        .then(mapToContactmomentViewModel);
    } else {
      return fetchLoggedIn(url)
        .then(throwIfNotOk)
        .then(parseJson)
        .then((p) => parsePagination(p, (x) => x as ContactmomentViewModel));
    }
  };

  return ServiceResult.fromFetcher(
    () => {

      if (gebruikKlantinteractiesApi.value === null) {
        return "";
      }

      if (!id.value) return "";

      // retourneer een url voor openklant 1 OF de klantInteracties api
      if (gebruikKlantinteractiesApi.value === true) {
        const searchParams = new URLSearchParams();
        searchParams.set("wasPartij__url", id.value);

        return `${klantinteractiesBetrokkenen}?${searchParams.toString()}`;
      } else {
        const searchParams = new URLSearchParams();
        searchParams.set("klant", id.value);
        searchParams.set("ordering", "-registratiedatum");
        searchParams.set("expand", "objectcontactmomenten");
        return `${contactmomentenUrl}?${searchParams.toString()}`;
      }
    },
    (u: string) => fetchContactmomenten(u, gebruikKlantinteractiesApi),
    { getUniqueId: getCacheKey },
  );
}

const fetchOk1Contactmomenten = (u: string) =>
  fetchLoggedIn(u)
    .then(throwIfNotOk)
    .then(parseJson)
    .then((p) => parsePagination(p, (x) => x as ContactmomentViewModel));

export const useContactmomentDetails = (url: () => string) =>
  ServiceResult.fromFetcher(
    () => {
      const u = url();
      if (!u) return "";
      const searchParams = new URLSearchParams();
      searchParams.set("id", u);
      return `${contactmomentDetails}?${searchParams.toString()}`;
    },
    (url) =>
      fetchLoggedIn(url).then((r) => {
        if (r.status === 404) return null;
        throwIfNotOk(r);
        return r.json() as Promise<ContactmomentDetails>;
      }),
  );

export function useContactmomentenByObjectUrl(url: Ref<string>) {
  const getUrl = () => {
    if (!url.value) return "";
    const params = new URLSearchParams();
    params.set("object", url.value);
    params.set("ordering", "-registratiedatum");
    params.set("expand", "objectcontactmomenten");
    return `${contactmomentenUrl}?${params}`;
  };

  return ServiceResult.fromFetcher(getUrl, fetchOk1Contactmomenten);
}

export function useContactmomentObject(getUrl: () => string) {
  return ServiceResult.fromFetcher(
    () => {
      const u = getUrl();
      if (!u) return "";
      return toRelativeProxyUrl(u, contactmomentenProxyRoot) || "";
    },
    (u) =>
      fetchLoggedIn(u)
        .then(throwIfNotOk)
        .then(parseJson) as Promise<ObjectContactmoment>,
  );
}

export function useContactmomentByUrl(getUrl: () => string) {
  return ServiceResult.fromFetcher(
    () => {
      const u = getUrl();
      if (!u) return "";
      const url = toRelativeProxyUrl(u, contactmomentenProxyRoot);
      if (!url) return "";
      const params = new URLSearchParams({
        expand: "objectcontactmomenten",
      });
      return `${url}?${params}`;
    },
    (u) =>
      fetchLoggedIn(u).then((r) => {
        if (r.status === 404) return null;
        throwIfNotOk(r);
        return r.json() as Promise<ContactmomentViewModel>;
      }),
  );
}

export function saveContactverzoek({
  data,
  contactmomentUrl,
}: {
  data: Omit<ContactverzoekData, "contactmoment">;
  contactmomentUrl: string;
  klantUrl?: string;
}) {
  const url = "/api/internetaak/api/v2/objects";

  const body: NewContactverzoek = {
    record: {
      // typeVersion wordt nu geconfigureerd en ingesteld in de backend.
      // Dit vereenvoudigt de frontend code en centraliseert de configuratie.
      startAt: formatIsoDate(data.registratiedatum),
      data: {
        ...data,
        contactmoment: contactmomentUrl,
      },
    },
  };

  return fetchLoggedIn(url, {
    method: "POST",
    headers: {
      Accept: "application/json",
      "Content-Type": "application/json",
    },
    body: JSON.stringify(body),
  })
    .then(throwIfNotOk)
    .then((r) => r.json() as Promise<{ url: string }>);
}

export function mapContactverzoekData({
  data,
  klantUrl,
}: {
  data: ContactmomentContactVerzoek;
  klantUrl?: string;
}): Omit<ContactverzoekData, "contactmoment"> {
  const now = new Date();
  const registratiedatum = now.toISOString();
  const digitaleAdressen = [] as DigitaalAdres[];
  if (data.emailadres) {
    digitaleAdressen.push({
      adres: data.emailadres,
      omschrijving: "e-mailadres",
      soortDigitaalAdres: "e-mailadres",
    });
  }
  if (data.telefoonnummer1) {
    digitaleAdressen.push({
      adres: data.telefoonnummer1,
      omschrijving: "telefoonnummer",
      soortDigitaalAdres: "telefoonnummer",
    });
  }
  if (data.telefoonnummer2) {
    digitaleAdressen.push({
      adres: data.telefoonnummer2,
      omschrijving: data.omschrijvingTelefoonnummer2 || "telefoonnummer",
      soortDigitaalAdres: "telefoonnummer",
    });
  }

  function formatVraagAntwoordForToelichting(vraagAntwoord: Vraag[]): string {
    return vraagAntwoord
      .map((va) => {
        if (isInputVraag(va)) {
          return `${va.description}: ${va.input}`;
        } else if (isTextareaVraag(va)) {
          return `${va.description}: ${va.textarea}`;
        } else if (isDropdownVraag(va)) {
          return `${va.description}: ${va.selectedDropdown}`;
        } else if (isCheckboxVraag(va)) {
          const selectedOptions = va.options
            .filter((_, index) => va.selectedCheckbox[index])
            .join(", ");
          return `${va.description}: ${selectedOptions}`;
        }
        return null;
      })
      .filter(Boolean)
      .join("\n");
  }

  const vragenToelichting =
    data.contactVerzoekVragenSet &&
    data.contactVerzoekVragenSet.vraagAntwoord &&
    data.contactVerzoekVragenSet.vraagAntwoord.length
      ? formatVraagAntwoordForToelichting(
          data.contactVerzoekVragenSet.vraagAntwoord,
        )
      : "";

  let verantwoordelijkheAfdeling = "";
  if (data.groep) {
    verantwoordelijkheAfdeling = data.groep.naam;
  } else if (data.afdeling) {
    verantwoordelijkheAfdeling = data.afdeling.naam;
  } else if (data.organisatorischeEenheidVanMedewerker) {
    verantwoordelijkheAfdeling =
      data.organisatorischeEenheidVanMedewerker.naam.split(": ")[1] || "";
  }

  // groep
  let actor = null;

  if (data.typeActor == ActorType.groep) {
    if (data.medewerker) {
      //voor een medewerker van een groep
      actor = {
        naam: data.medewerker.achternaam || "",
        soortActor: "medewerker",
        identificatie: data.medewerker?.identificatie || "",
        typeOrganisatorischeEenheid: TypeOrganisatorischeEenheid.Groep,
        naamOrganisatorischeEenheid: data.groep?.naam || "",
        identificatieOrganisatorischeEenheid: data.groep?.identificatie || "",
      };
    } else {
      //alleen een groep. geen medewerker geselecteerd
      actor = {
        naam: data.groep?.naam || "",
        soortActor: "organisatorische eenheid",
        identificatie: data.groep?.identificatie || "",
        typeOrganisatorischeEenheid: TypeOrganisatorischeEenheid.Groep,
      };
    }
  }
  if (data.typeActor == ActorType.afdeling) {
    if (data.medewerker) {
      //voor een medewerker van een afdeling
      actor = {
        naam: data.medewerker.achternaam || "",
        soortActor: "medewerker",
        identificatie: data.medewerker?.identificatie || "",
        typeOrganisatorischeEenheid: TypeOrganisatorischeEenheid.Afdeling,
        naamOrganisatorischeEenheid: data.afdeling?.naam || "",
        identificatieOrganisatorischeEenheid:
          data.afdeling?.identificatie || "",
      };
    } else {
      //alleen een afdeling. geen medewerker geselecteerd
      actor = {
        naam: data.afdeling?.naam || "",
        soortActor: "organisatorische eenheid",
        identificatie: data.afdeling?.identificatie || "",
        typeOrganisatorischeEenheid: TypeOrganisatorischeEenheid.Afdeling,
      };
    }
  }
  if (data.typeActor == ActorType.medewerker) {
    actor = {
      naam: data.medewerker?.achternaam || "",
      soortActor: "medewerker",
      identificatie: data.medewerker?.identificatie || "",
      typeOrganisatorischeEenheid:
        data.organisatorischeEenheidVanMedewerker?.naam
          ?.split(": ")[0]
          ?.toLowerCase() === "afdeling"
          ? TypeOrganisatorischeEenheid.Afdeling
          : TypeOrganisatorischeEenheid.Groep,
      naamOrganisatorischeEenheid:
        data.organisatorischeEenheidVanMedewerker?.naam.split(": ")[1] || "",
      identificatieOrganisatorischeEenheid:
        data.organisatorischeEenheidVanMedewerker?.identificatie || "",
    };
  }

  if (actor === null) {
    throw new Error("actor kan niet bepaald worden");
  }

  return {
    verantwoordelijkeAfdeling: verantwoordelijkheAfdeling,
    status: "te verwerken",
    registratiedatum,
    toelichting:
      data.interneToelichting +
      (vragenToelichting ? "\n\n" + vragenToelichting : ""),
    actor: actor,
    betrokkene: {
      rol: "klant",
      klant: klantUrl,
      persoonsnaam: {
        voornaam: data.voornaam,
        voorvoegselAchternaam: data.voorvoegselAchternaam,
        achternaam: data.achternaam,
      },
      organisatie: data.organisatie,
      digitaleAdressen,
    },
  };
}
