import { throwIfNotOk, parseJson } from "..";
import { fetchWithSysteemId } from "../fetch-with-systeem-id";
import { parsePagination } from "../zgw";

const contactmomentenProxyRoot = "/api/contactmomenten";
const contactmomentenApiRoot = "/contactmomenten/api/v1";
const contactmomentenBaseUrl = `${contactmomentenProxyRoot}${contactmomentenApiRoot}`;
const contactmomentenUrl = `${contactmomentenBaseUrl}/contactmomenten`;

export interface ContactmomentViewModelOk1 {
  url: string;
  registratiedatum: string;
  kanaal: string;
  tekst: string;
  objectcontactmomenten: {
    object: string;
    objectType: string;
    contactmoment: string;
  }[];
  medewerkerIdentificatie: {
    identificatie: string;
    achternaam: string;
    voorletters: string;
    voorvoegselAchternaam: string;
  };
}

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
    .then((p) => parsePagination(p, (x) => x as ContactmomentViewModelOk1));
}

export function fetchContactmomentenByObjectUrlOk1({
  systeemIdentifier,
  objectUrl,
}: {
  systeemIdentifier: string;
  objectUrl: string;
}) {
  const params = new URLSearchParams();
  params.set("object", objectUrl);
  params.set("ordering", "-registratiedatum");
  params.set("expand", "objectcontactmomenten");

  return fetchWithSysteemId(
    systeemIdentifier,
    `${contactmomentenUrl}?${params}`,
  )
    .then(throwIfNotOk)
    .then(parseJson)
    .then((p) => parsePagination(p, (x) => x as ContactmomentViewModelOk1));
}
