import { fetchLoggedIn, getFormattedUtcDate, throwIfNotOk } from "@/services";
import type { NieuweKlant } from "@/stores/contactmoment";
// creating a klant will be done differently in the future. for now, jus reuse the type from the klant feature
import { KlantType } from "../shared/types";

export interface Contactverzoek {
  bronorganisatie: string; //verplicht in de api
  todo: {
    name: "contactverzoek";
    description: string;
    attendees: string[];
  };
}

export function saveContactverzoek(data: Contactverzoek) {
  const url = window.gatewayBaseUri + "/api/contactmomenten";
  const registratiedatum = getFormattedUtcDate();

  return fetchLoggedIn(url, {
    method: "POST",
    headers: {
      Accept: "application/json",
      "Content-Type": "application/json",
    },
    body: JSON.stringify({
      ...data,
      registratiedatum,
    }),
  })
    .then(throwIfNotOk)
    .then((r) => r.json() as Promise<{ id: string; url: string }>);
}

export function createKlant(klant: NieuweKlant) {
  const url = `${window.gatewayBaseUri}/api/klanten`;
  return fetchLoggedIn(url, {
    method: "POST",
    headers: {
      "content-type": "application/json",
    },
    body: JSON.stringify({
      ...klant,
      bronorganisatie: window.organisatieIds[0],
      // TODO: WAT MOET HIER IN KOMEN?
      klantnummer: "123",
      subjectType: KlantType.Persoon,
    }),
  })
    .then(throwIfNotOk)
    .then((r) => r.json());
}
