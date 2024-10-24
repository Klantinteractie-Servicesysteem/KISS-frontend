import { throwIfNotOk } from "..";
import { fetchLoggedIn, setHeader } from "../fetch-logged-in";
import type { ZaakContactmoment } from "./types";

const zaaksysteemProxyRoot = `/api/zaken`;
const zaaksysteemApiRoot = `/zaken/api/v1`;
const zaaksysteemBaseUri = `${zaaksysteemProxyRoot}${zaaksysteemApiRoot}`;
const zaakcontactmomentUrl = `${zaaksysteemBaseUri}/zaakcontactmomenten`;

export function fetchWithZaaksysteemId(
  zaaksysteemId: string | undefined,
  url: string,
  request: RequestInit = {},
) {
  if (zaaksysteemId) {
    setHeader(request, "ZaaksysteemId", zaaksysteemId);
  }
  return fetchLoggedIn(url, request);
}

export const voegContactmomentToeAanZaak = (
  { contactmoment, zaak }: ZaakContactmoment,
  zaaksysteemId: string,
) =>
  fetchWithZaaksysteemId(zaaksysteemId, zaakcontactmomentUrl, {
    method: "POST",
    headers: {
      Accept: "application/json",
      "Content-Type": "application/json",
    },
    body: JSON.stringify({ contactmoment, zaak }),
  }).then(throwIfNotOk);
