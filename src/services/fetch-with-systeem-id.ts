import { fetchLoggedIn, getHeader, setHeader } from "./fetch-logged-in";
const HEADER_NAME = "systemIdentifier";
const VARY = "vary";
const VARY_SEPERATOR = ",";

export function fetchWithSysteemId(
  systemIdentifier: string | undefined,
  url: string,
  request: RequestInit = {},
) {
  if (systemIdentifier) {
    setHeader(request, HEADER_NAME, systemIdentifier);
    const vary = getHeader(request, VARY)?.split(VARY_SEPERATOR) || [];
    setHeader(request, VARY, [...vary, HEADER_NAME].join(VARY_SEPERATOR));
  }
  return fetchLoggedIn(url, request);
}
