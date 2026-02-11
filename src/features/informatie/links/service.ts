import { fetchLoggedIn, ServiceResult, throwIfNotOk } from "@/services";

const linksUrl = (function () {
  return "/api/links";
})();

function fetchLinks(url: string) {
  return fetchLoggedIn(url)
    .then(throwIfNotOk)
    .then((r) => r.json());
}

async function fetchAllLinks(url: string) {
  const first = await fetchLinks(url);

  return first;
}

export function useLinks() {
  return ServiceResult.fromFetcher(linksUrl, fetchAllLinks);
}
