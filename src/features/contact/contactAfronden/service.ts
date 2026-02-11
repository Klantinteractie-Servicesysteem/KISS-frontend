import { throwIfNotOk, ServiceResult } from "@/services";
import { fetchLoggedIn } from "@/services";

import { formatIsoDate } from "@/helpers/date";
import {
  ActorType,
  type ContactmomentContactVerzoek,
} from "@/stores/contactmoment/index";

import {
  TypeOrganisatorischeEenheid,
  type Kanaal,
  type Vraag,
} from "@/features/types";
import {
  isCheckboxVraag,
  isDropdownVraag,
  isInputVraag,
  isTextareaVraag,
} from "../service";
import { DigitaalAdresTypes } from "@/services/openklant2";
import type { ZaakDetails } from "@/features/contact/contactBehandelen/vraagBehandelen/klantgerelateerdeGegevensInzien/zaken/types";
import { koppelObject } from "@/services/openklant1";
import { fetchWithSysteemId } from "@/services/fetch-with-systeem-id";
import type {
  ContactverzoekData,
  DigitaalAdres,
  NewContactverzoek,
} from "@/features/contact/types";

//obsolete. api calls altijd vanuit /src/services of /src/apis. hier alleen nog busniesslogica afhandelen
const contactmomentenProxyRoot = "/api/contactmomenten";
const contactmomentenApiRoot = "/contactmomenten/api/v1";
const contactmomentenBaseUrl = `${contactmomentenProxyRoot}${contactmomentenApiRoot}`;

const klantcontactmomentenUrl = `${contactmomentenBaseUrl}/klantcontactmomenten`;

export const CONTACTVERZOEK_GEMAAKT = "Contactverzoek gemaakt";

export interface Gespreksresultaat {
  definitie: string;
}

//todo wordt alleen gebruikt i n ContactmomentAfhandeling.vue. verplaatsen naar daar?
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

//todo wordt alleen gebruikt i n ContactmomentAfhandeling.vue. verplaatsen naar daar?
export async function koppelKlant({
  systemId,
  klantUrl,
  contactmomentId,
}: {
  systemId: string;
  klantUrl: string;
  contactmomentId: string;
}) {
  return fetchWithSysteemId(systemId, klantcontactmomentenUrl, {
    method: "POST",
    headers: {
      "Content-Type": "application/json",
    },
    body: JSON.stringify({
      klant: klantUrl,
      contactmoment: contactmomentId,
      rol: "gesprekspartner",
    }),
  }).then(throwIfNotOk) as Promise<void>;
}

//todo wordt alleen gebruikt i n ContactmomentAfhandeling.vue. verplaatsen naar daar?
//te gebruiken om cotactverzoeken als internetaak op te slaan in een overige objecten register, wanneer er geen regiser compatibel met openklant 2 of hoger beschikbaar is.
export function saveContactverzoek({
  systemIdentifier,
  data,
  contactmomentUrl,
}: {
  systemIdentifier: string;
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

  return fetchWithSysteemId(systemIdentifier, url, {
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

//todo wordt alleen gebruikt i n ContactmomentAfhandeling.vue. verplaatsen naar daar?
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
      soortDigitaalAdres: DigitaalAdresTypes.email,
    });
  }
  if (data.telefoonnummer1) {
    digitaleAdressen.push({
      adres: data.telefoonnummer1,
      omschrijving: "telefoonnummer",
      soortDigitaalAdres: DigitaalAdresTypes.telefoonnummer,
    });
  }
  if (data.telefoonnummer2) {
    digitaleAdressen.push({
      adres: data.telefoonnummer2,
      omschrijving: data.omschrijvingTelefoonnummer2 || "telefoonnummer",
      soortDigitaalAdres: DigitaalAdresTypes.telefoonnummer,
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
    if (data.medewerker || data.medewerkeremail) {
      //voor een medewerker van een groep
      actor = {
        naam: data.medewerker?.achternaam || data.medewerkeremail || "",
        soortActor: "medewerker",
        identificatie:
          data.medewerker?.identificatie || data.medewerkeremail || "",
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
    if (data.medewerker || data.medewerkeremail) {
      //voor een medewerker van een afdeling
      actor = {
        naam: data.medewerker?.achternaam || data.medewerkeremail || "",
        soortActor: "medewerker",
        identificatie:
          data.medewerker?.identificatie || data.medewerkeremail || "",
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

//todo wordt alleen gebruikt i n ContactmomentAfhandeling.vue. verplaatsen naar daar?
export async function koppelZaakEnContactmoment(
  systeemId: string,
  zaak: ZaakDetails,
  contactmomentUrl: string,
) {
  try {
    await koppelObject(systeemId, {
      contactmoment: contactmomentUrl,
      object: zaak.url,
      objectType: "zaak",
    });
  } catch (e) {
    console.log("koppelZaakContactmoment in openklant", e);
  }
}

async function fetchAll(url: string): Promise<Kanaal[]> {
  return await fetchLoggedIn(url)
    .then(throwIfNotOk)
    .then((r) => r.json());
}

export function useKanalenKeuzeLijst() {
  return ServiceResult.fromFetcher(
    "/api/KanalenContactmomentKeuzelijst",
    fetchAll,
  );
}
