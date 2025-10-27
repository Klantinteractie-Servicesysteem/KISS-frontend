import { parseJson, ResponseError, throwIfNotOk } from "..";
import { fetchWithSysteemId } from "../fetch-with-systeem-id";
import type { ZaakContactmoment } from "./types";

const zaaksysteemProxyRoot = `/api/zaken`;
const zaaksysteemApiRoot = `/zaken/api/v1`;
const zaaksysteemBaseUri = `${zaaksysteemProxyRoot}${zaaksysteemApiRoot}`;
const zaakcontactmomentUrl = `${zaaksysteemBaseUri}/zaakcontactmomenten`;

export const voegContactmomentToeAanZaak = (
  { contactmoment, zaak }: ZaakContactmoment,
  zaaksysteemId: string,
) =>
  fetchWithSysteemId(zaaksysteemId, zaakcontactmomentUrl, {
    method: "POST",
    headers: {
      Accept: "application/json",
      "Content-Type": "application/json",
    },
    body: JSON.stringify({ contactmoment, zaak }),
  }).then(throwIfNotOk);

export const fetchZaakIdentificatieByUrlOrId = (
  systeemId: string,
  urlOrId: string,
) => {
  const id = urlOrId.split("/").at(-1) || urlOrId;

  return fetchWithSysteemId(systeemId, `${zaaksysteemBaseUri}/zaken/${id}`)
    .then((response: Response) => {
      // since it's possible to use one openklant instance with mutiple openzaak instances,
      // we don't know in which zaaksysteem to look for a zaak belonging to a contactverzoek
      // so we have to try them all and ignore 404's
      if (!response.ok && response.status != 404) {
        throw new ResponseError(response.statusText, response);
      }
      return response as Response & { ok: true };
    })
    .then(parseJson)
    .then(({ identificatie }) => identificatie as string);
};
