import { parseJson, throwIfNotOk } from "..";
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
    .then(throwIfNotOk)
    .then(parseJson)
    .then(({ identificatie }) => identificatie as string);
};
