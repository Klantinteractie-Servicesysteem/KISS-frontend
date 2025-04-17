import { parseJson, throwIfNotOk } from "..";
import { fetchWithSysteemId } from "../fetch-with-systeem-id";
import { parsePagination } from "../zgw";

export function fetchInternetakenByKlantIdFromObjecten({
  klantUrl,
  systeemId,
}: {
  klantUrl: string;
  systeemId: string;
}) {
  const url = new URL("/api/internetaak/api/v2/objects", location.origin);
  url.searchParams.set("ordering", "-record__data__registratiedatum");
  url.searchParams.set("pageSize", "10");
  url.searchParams.set("data_attr", `betrokkene__klant__exact__${klantUrl}`);

  return fetchWithSysteemId(systeemId, url.toString())
    .then(throwIfNotOk)
    .then(parseJson)
    .then((x) => parsePagination(x, (o) => o as any));
}
