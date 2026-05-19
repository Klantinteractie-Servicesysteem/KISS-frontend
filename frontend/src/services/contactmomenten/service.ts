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

export const fetchObjectContactmomenten = ({
  systeemIdentifier,
  contactmomentUrl,
}: {
  systeemIdentifier: string;
  contactmomentUrl: string;
}) =>
  fetchWithSysteemId(
    systeemIdentifier,
    `${contactmomentenBaseUrl}/objectcontactmomenten?${new URLSearchParams({ contactmoment: contactmomentUrl })}`,
  )
    .then(throwIfNotOk)
    .then(parseJson)
    .then((p) =>
      parsePagination(
        p,
        (x) =>
          x as { contactmoment: string; object: string; objectType: string },
      ),
    );

export async function fetchContactmomentenByObjectUrlOk1({
  systeemIdentifier,
  objectUrl,
}: {
  systeemIdentifier: string;
  objectUrl: string;
}) {
  // LET OP. OpenKlant1 ondersteunt rechtstreeks contactmomenten zoeken op object, maar de esuite niet
  const contactmomentIds = await fetchWithSysteemId(
    systeemIdentifier,
    `${contactmomentenBaseUrl}/objectcontactmomenten?${new URLSearchParams({ object: objectUrl })}`,
  )
    .then(throwIfNotOk)

    .then(parseJson)
    .then((p) =>
      parsePagination(p, (x) =>
        (x as { contactmoment: string }).contactmoment.split("/").pop(),
      ),
    );

  const result = {
    ...contactmomentIds,
    page: [] as ContactmomentViewModelOk1[],
  };

  for (const id of contactmomentIds.page) {
    if (id) {
      const cm = await fetchWithSysteemId(
        systeemIdentifier,
        `${contactmomentenUrl}/${id}`,
      )
        .then(throwIfNotOk)
        .then(parseJson);
      result.page.push(cm);
    }
  }

  result.page.sort(
    (a, b) =>
      new Date(b.registratiedatum).valueOf() -
      new Date(a.registratiedatum).valueOf(),
  );

  return result;
}
