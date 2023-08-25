import {
  throwIfNotOk,
  ServiceResult,
  parsePagination,
  parseJson,
} from "@/services";
import { fetchLoggedIn } from "@/services";
import type { Ref } from "vue";

import type {
  Gespreksresultaat,
  ContactmomentObject,
  Contactmoment,
  ZaakContactmoment,
  ObjectContactmoment,
  ContactmomentDetails,
} from "./types";
import type { ContactmomentViewModel } from "../shared/types";
import { toRelativeProxyUrl } from "@/helpers/url";

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

export const saveContactmoment = (
  data: Contactmoment
): Promise<{ url: string; gespreksId: string }> => {
  const x = data;

  return fetchLoggedIn(`/api/postcontactmomenten`, {
    method: "POST",
    headers: {
      Accept: "application/json",
      "Content-Type": "application/json",
    },
    body: JSON.stringify(data),
  })
    .then(throwIfNotOk)
    .then((r) => r.json());
};

export const CONTACTVERZOEK_GEMAAKT = "Contactverzoek gemaakt";

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
      .then((results: Array<Gespreksresultaat>) => {
        if (!Array.isArray(results))
          throw new Error("unexpected json result: " + JSON.stringify(results));
        const hasContactverzoekResultaat = results.some(
          ({ definitie }) => definitie === CONTACTVERZOEK_GEMAAKT
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

const fetchContactmomenten = (u: string) =>
  fetchLoggedIn(u)
    .then(throwIfNotOk)
    .then(parseJson)
    .then((p) => parsePagination(p, (x) => x as ContactmomentViewModel));

export function useContactmomentenByKlantId(id: Ref<string>) {
  function getUrl() {
    if (!id.value) return "";
    const searchParams = new URLSearchParams();
    searchParams.set("klant", id.value);
    searchParams.set("ordering", "-registratiedatum");
    searchParams.set("expand", "objectcontactmomenten");
    return `${contactmomentenUrl}?${searchParams.toString()}`;
  }

  return ServiceResult.fromFetcher(getUrl, fetchContactmomenten);
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
      })
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

  return ServiceResult.fromFetcher(getUrl, fetchContactmomenten);
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
        .then(parseJson) as Promise<ObjectContactmoment>
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
      fetchLoggedIn(u)
        .then(throwIfNotOk)
        .then(parseJson)
        .then((r) => r as ContactmomentViewModel)
  );
}
