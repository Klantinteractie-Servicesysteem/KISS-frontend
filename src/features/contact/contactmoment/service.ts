import {
  throwIfNotOk,
  ServiceResult,
  parsePagination,
  parseJson,
} from "@/services";
import { fetchLoggedIn } from "@/services";

import {
  type Gespreksresultaat,
  type ObjectContactmoment,
  type ContactmomentDetails,
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
  DigitaalAdresTypes,
  fetchKlantcontacten,
  KlantContactExpand,
  mapKlantContactToContactmomentViewModel,
  type ContactmomentViewModel,
} from "@/services/openklant2";
import type { ZaakDetails } from "@/features/zaaksysteem/types";
import { voegContactmomentToeAanZaak } from "@/services/openzaak";
import { koppelObject } from "@/services/openklant1";
import { fetchWithSysteemId } from "@/services/fetch-with-systeem-id";
import {
  registryVersions,
  type Systeem,
} from "@/services/environment/fetch-systemen";

//obsolete. api calls altijd vanuit /src/services of /src/apis. hier alleen nog busniesslogica afhandelen
const contactmomentenProxyRoot = "/api/contactmomenten";
const contactmomentenApiRoot = "/contactmomenten/api/v1";
const contactmomentenBaseUrl = `${contactmomentenProxyRoot}${contactmomentenApiRoot}`;
const contactmomentDetails = "/api/contactmomentdetails";

const contactmomentenUrl = `${contactmomentenBaseUrl}/contactmomenten`;
const klantcontactmomentenUrl = `${contactmomentenBaseUrl}/klantcontactmomenten`;

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

export function koppelKlant({
  systemId,
  klantId,
  contactmomentId,
}: {
  systemId: string;
  klantId: string;
  contactmomentId: string;
}) {
  return fetchWithSysteemId(systemId, klantcontactmomentenUrl, {
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

export function fetchContactmomentenByObjectUrl(systeem: Systeem, url: string) {
  if (systeem.registryVersion === registryVersions.ok2) {
    // OK2
    const id = url.split("/").at(-1);
    if (!id) return Promise.reject("missing id");

    return fetchKlantcontacten({
      systeemIdentifier: systeem.identifier,
      onderwerpobject__onderwerpobjectidentificatorObjectId: id,
      expand: [KlantContactExpand.gingOverOnderwerpobjecten],
    }).then((paginated) => ({
      ...paginated,
      page: paginated.page.map((x) => ({
        ...mapKlantContactToContactmomentViewModel(x),
        systeemId: systeem.identifier,
      })),
    }));
  }

  // OK1
  const params = new URLSearchParams();
  params.set("object", url);
  params.set("ordering", "-registratiedatum");
  params.set("expand", "objectcontactmomenten");

  return fetchWithSysteemId(
    systeem.identifier,
    `${contactmomentenUrl}?${params}`,
  )
    .then(throwIfNotOk)
    .then(parseJson)
    .then((p) =>
      parsePagination(p, (x) => ({
        ...(x as ContactmomentViewModel),
        systeemId: systeem.identifier,
      })),
    );
}

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

export async function koppelZaakEnContactmoment(
  zaak: ZaakDetails,
  contactmomentUrl: string,
) {
  // dit is voorlopige, hopelijk tijdelijke, code om uit te proberen of dit een nuttige manier is om met de instabiliteit van openzaak en openklant om te gaan
  // derhalve bewust nog niet geoptimaliseerd
  await addContactmomentToZaak(contactmomentUrl, zaak.url, zaak.zaaksysteemId);

  // voorgaande gaat vaak mis, maar geeft dan bijna altijd ten onterechte een error response.
  // de data is dan wel correct opgeslagen
  // wellicht een timing issue. voor de zekerheid even wachten
  try {
    setTimeout(
      async () =>
        await koppelObject({
          contactmoment: contactmomentUrl,
          object: zaak.self,
          objectType: "zaak",
        }),
      1000,
    );
  } catch (e) {
    console.log("koppelZaakContactmoment in openklant", e);
  }
}
export async function addContactmomentToZaak(
  contactmomentUrl: string,
  zaakUrl: string,
  zaaksysteemId: string,
) {
  try {
    await voegContactmomentToeAanZaak(
      {
        contactmoment: contactmomentUrl,
        zaak: zaakUrl,
      },
      zaaksysteemId,
    );
  } catch (e) {
    try {
      console.log(
        "voegContactmomentToeAanZaak in openzaak attempt 1 failed",
        e,
      );
      await voegContactmomentToeAanZaak(
        {
          contactmoment: contactmomentUrl,
          zaak: zaakUrl,
        },
        zaaksysteemId,
      );
    } catch (e) {
      try {
        console.log(
          "voegContactmomentToeAanZaak in openzaak attempt 2 failed",
          e,
        );
        await voegContactmomentToeAanZaak(
          {
            contactmoment: contactmomentUrl,
            zaak: zaakUrl,
          },
          zaaksysteemId,
        );
      } catch (e) {
        console.log(
          "voegContactmomentToeAanZaak in openzaak attempt 3 failed",
          e,
        );
      }
    }
  }
}
