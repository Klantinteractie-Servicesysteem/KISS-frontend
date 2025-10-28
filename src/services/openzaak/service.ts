import { parseJson, throwIfNotOk } from "..";
import { fetchWithSysteemId } from "../fetch-with-systeem-id";
import type { ZaakContactmoment } from "./types";

const zakenApiPrefix = `/api/zaken`;

export const voegContactmomentToeAanZaak = (
  { contactmoment, zaak }: ZaakContactmoment,
  zaaksysteemId: string,
) =>
  fetchWithSysteemId(zaaksysteemId, `${zakenApiPrefix}/zaakcontactmomenten`, {
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

  return fetchWithSysteemId(systeemId, `${zakenApiPrefix}/zaken/${id}`)
    .then(throwIfNotOk)
    .then(parseJson)
    .then(({ identificatie }) => identificatie as string);
};
