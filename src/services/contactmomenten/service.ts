import { throwIfNotOk, parseJson } from "..";
import { fetchWithSysteemId } from "../fetch-with-systeem-id";
import type { ContactmomentViewModel } from "../openklant2";
import { parsePagination } from "../zgw";

const contactmomentenProxyRoot = "/api/contactmomenten";
const contactmomentenApiRoot = "/contactmomenten/api/v1";
const contactmomentenBaseUrl = `${contactmomentenProxyRoot}${contactmomentenApiRoot}`;
const contactmomentenUrl = `${contactmomentenBaseUrl}/contactmomenten`;

export function fetchContactmomentenByKlantUrlOk1({
  systeemIdentifier,
  klantUrl,
}: {
  systeemIdentifier: string;
  klantUrl: string;
}) {
  const searchParams = new URLSearchParams();
  searchParams.set("klant", klantUrl);
  searchParams.set("ordering", "-registratiedatum");
  searchParams.set("expand", "objectcontactmomenten");

  return fetchWithSysteemId(
    systeemIdentifier,
    `${contactmomentenUrl}?${searchParams.toString()}`,
  )
    .then(throwIfNotOk)
    .then(parseJson)
    .then((p) =>
      parsePagination(p, (x) => ({
        ...(x as ContactmomentViewModel),
        systeemId: systeemIdentifier,
      })),
    );
}
