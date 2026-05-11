import { fetchLoggedIn, setHeader } from "./fetch-logged-in";

export function fetchWithSysteemId(
  systemIdentifier: string | undefined,
  url: string,
  request: RequestInit = {},
) {
  if (systemIdentifier) {
    setHeader(request, "systemIdentifier", systemIdentifier);
  }
  return fetchLoggedIn(url, request);
}
