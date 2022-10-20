// ONLY use this during development, to fix existing test data without a subjectType
import {
  fetchLoggedIn,
  throwIfNotOk,
  parseJson,
  parsePagination,
} from "@/services";
import { KlantType } from "../shared/types";

const klantRootUrl = `${window.gatewayBaseUri}/api/klanten`;

const urlForKLantenWithoutSubjectType = (function getAllKlantenUrl() {
  const url = new URL(klantRootUrl);
  url.searchParams.append("fields[]", "klantnummer");
  url.searchParams.append("fields[]", "bronorganisatie");
  url.searchParams.append("subjectType", "IS NULL");
  return url.toString();
})();

type MinimalKlant = {
  id: string;
  klantnummer: string;
  bronorganisatie: string;
};

function fixSubjectType({
  id,
  klantnummer,
  bronorganisatie,
}: MinimalKlant): Promise<Response> {
  const url = klantRootUrl + "/" + id;
  return fetchLoggedIn(url, {
    method: "PUT",
    headers: {
      "Content-Type": "application/json",
    },
    body: JSON.stringify({
      klantnummer,
      bronorganisatie,
      subjectType: KlantType.Persoon,
    }),
  }).then(throwIfNotOk);
}

export function fixAllSubjectTypes() {
  return fetchLoggedIn(urlForKLantenWithoutSubjectType)
    .then(throwIfNotOk)
    .then(parseJson)
    .then((json) => parsePagination(json, (o) => o as MinimalKlant))
    .then(async (pagination) => {
      await Promise.all(pagination.page.map(fixSubjectType));
      if (pagination.totalPages > 1) {
        // this means there are more records without a subjectType
        await fixAllSubjectTypes();
      }
    });
}
