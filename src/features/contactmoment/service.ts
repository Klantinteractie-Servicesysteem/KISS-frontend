import { formatDateOnly, formatTimeOnly } from "@/helpers/date";
import {
  throwIfNotOk,
  ServiceResult,
  type Paginated,
  parsePagination,
  parseJson,
} from "@/services";
import { fetchLoggedIn } from "@/services";
import type { Ref } from "vue";

import type {
  Gespreksresultaat,
  ContactmomentObject,
  Contactmoment,
  ContactverzoekDetail,
  ZaakContactmoment,
  ObjectContactmoment,
  // KlantContactmoment,
} from "./types";
import type { ContactmomentViewModel } from "../shared/types";
import { toRelativeProxyUrl } from "@/helpers/url";

const contactmomentenProxyRoot = "/api/contactmomenten";
const contactmomentenApiRoot = "/contactmomenten/api/v1";
const contactmomentenBaseUrl = `${contactmomentenProxyRoot}${contactmomentenApiRoot}`;
const objectcontactmomentenUrl = `${contactmomentenBaseUrl}/objectcontactmomenten`;
const klantcontactmomentenUrl = `${contactmomentenBaseUrl}/klantcontactmomenten`;

const zaaksysteemProxyRoot = `/api/zaken`;
const zaaksysteemApiRoot = `/zaken/api/v1`;
const zaaksysteemBaseUri = `${zaaksysteemProxyRoot}${zaaksysteemApiRoot}`;
const zaakcontactmomentUrl = `${zaaksysteemBaseUri}/zaakcontactmomenten`;

export const saveContactmoment = (
  data: Contactmoment
): Promise<{ url: string; gespreksId: string }> =>
  fetchLoggedIn(`/api/postcontactmomenten`, {
    method: "POST",
    headers: {
      Accept: "application/json",
      "Content-Type": "application/json",
    },
    body: JSON.stringify(data),
  })
    .then(throwIfNotOk)
    .then((r) => r.json());

export const useGespreksResultaten = () => {
  const fetchBerichten = (url: string) =>
    fetchLoggedIn(url)
      .then((r) => {
        if (!r.ok) {
          throw new Error(
            "Er is een fout opgetreden bij het laden van de gespreksresultaten"
          );
        }
        return r.json();
      })
      .then((results) => {
        if (!Array.isArray(results))
          throw new Error("unexpected json result: " + JSON.stringify(results));
        return results as Array<Gespreksresultaat>;
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

export const koppelZaakContactmoment = (data: ZaakContactmoment) =>
  fetchLoggedIn(zaakcontactmomentUrl, {
    method: "POST",
    headers: {
      Accept: "application/json",
      "Content-Type": "application/json",
    },
    body: JSON.stringify(data),
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

export function useContactverzoekenByKlantId(
  id: Ref<string>,
  page: Ref<number>
) {
  function getUrl() {
    return "not implemented";
    const url = new URL("/api/klantcontactmomenten", location.href);
    url.searchParams.set(
      "_order[embedded.contactmoment.registratiedatum]",
      "desc"
    );
    url.searchParams.append("extend[]", "medewerker");
    url.searchParams.append("extend[]", "embedded._self.owner");
    url.searchParams.append("extend[]", "embedded.contactmoment.todo");
    url.searchParams.set("_limit", "10");
    url.searchParams.set("_page", page.value.toString());
    url.searchParams.set("embedded.klant._self.id", id.value);
    url.searchParams.set("embedded.contactmoment.todo", "IS NOT NULL");
    return url.toString();
  }

  return ServiceResult.fromFetcher(getUrl, fetchContactverzoeken, {
    getUniqueId() {
      return getUrl() + "contactverzoek";
    },
  });
}

const fetchObject = (u: string) =>
  fetchLoggedIn(u)
    .then(throwIfNotOk)
    .then(parseJson) as Promise<ObjectContactmoment>;

const fetchContactmoment = (u: string) =>
  fetchLoggedIn(u)
    .then(throwIfNotOk)
    .then(parseJson)
    .then(async (cm) => {
      const { objectcontactmomenten } = cm || {};
      if (!Array.isArray(objectcontactmomenten)) return cm;

      const promises = objectcontactmomenten.map((x: string) => {
        const oUrl = toRelativeProxyUrl(x, contactmomentenProxyRoot);
        if (!oUrl) throw new Error("invalid url: " + x);
        return fetchObject(oUrl);
      });

      const objects = await Promise.all(promises);

      const zaken = objects
        .filter((x) => x.objectType === "zaak")
        .map((x) => x.object);

      return {
        ...cm,
        zaken,
      } as ContactmomentViewModel;
    });

const fetchContactmomenten = (u: string) =>
  fetchLoggedIn(u)
    .then(throwIfNotOk)
    .then(parseJson)
    .then((p) =>
      parsePagination(p, (x: any) => {
        const i = toRelativeProxyUrl(
          x?.contactmoment,
          contactmomentenProxyRoot
        );
        if (!i) throw new Error("invalide url: " + x?.contactmoment);
        return fetchContactmoment(i);
      })
    );

export function useContactmomentenByKlantId(
  id: Ref<string>
  // page: Ref<number>
) {
  function getUrl() {
    const searchParams = new URLSearchParams();
    searchParams.set("klant", id.value);
    return `${klantcontactmomentenUrl}?${searchParams.toString()}`;
  }

  return ServiceResult.fromFetcher(getUrl, fetchContactmomenten);
}

function fetchContactverzoeken(url: string): Promise<Paginated<any>> {
  return Promise.reject("not implemented");
  return fetchLoggedIn(url)
    .then(throwIfNotOk)
    .then((r) => r.json())
    .then((x) => parsePagination(x, mapContactverzoekDetail))
    .then((paginated) => {
      const page = paginated.page.filter((p) => p !== undefined);

      return { ...paginated, page };
    });
}

const mapContactverzoekDetail = (
  rawContactverzoek: any
): ContactverzoekDetail | undefined => {
  const contactmoment = rawContactverzoek.embedded.contactmoment;
  const todo = rawContactverzoek.embedded.contactmoment.embedded.todo;

  return {
    id: contactmoment.id,
    datum: formatDateOnly(new Date(contactmoment.registratiedatum)),
    status: todo.completed ? "Gesloten" : "Open",
    behandelaar: todo.attendees?.[0] ?? "-",
    afgerond: todo.completed ? formatDateOnly(new Date(todo.completed)) : "-",
    starttijd: formatTimeOnly(new Date(contactmoment.registratiedatum)),
    aanmaker: contactmoment["_self"].owner,
    notitie: todo.description,
    primaireVraagWeergave: contactmoment.primaireVraagWeergave,
    afwijkendOnderwerp: contactmoment.afwijkendOnderwerp,
  };
};

export function useContactmomentenByObjectUrl(url: Ref<string>) {
  const getUrl = () => {
    if (!url.value) return "";
    const params = new URLSearchParams();
    params.set("object", url.value);
    return `${objectcontactmomentenUrl}?${params}`;
  };

  return ServiceResult.fromFetcher(getUrl, fetchContactmomenten);
}
