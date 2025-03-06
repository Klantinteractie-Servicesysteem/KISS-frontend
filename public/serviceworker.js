/// <reference no-default-lib="true"/>
/// <reference lib="ES2020" />
/// <reference lib="webworker" />

const TIMEOUT = 10000;

/**
 * @param {Request} r
 * @param {string} c
 * @returns
 */
function getCacheKey(r, c) {
  const url = new URL(r.url);
  url.searchParams.sort();
  return `${url.pathname}?${url.searchParams}||CLIENT-${c}||HEADERS||${
    r.headers
      .get("vary")
      ?.split(",")
      .map((hn) => [hn, r.headers.get(hn)])
      .filter(([k, v]) => !!k && !!v)
      .flat()
      .join("||") || ""
  }`;
}

/**
 * @type {Map<string, Promise<Response>>}
 */
const map = new Map();

/**
 *
 * @param {Request} request
 * @param {string} client
 * @returns
 */
function getResponse(request, client) {
  const key = getCacheKey(request, client);
  const cached = map.get(key);
  if (cached) return cached.then((r) => r.clone());
  const promise = fetch(request).then((r) => {
    if (!r.ok) {
      map.delete(key);
      return r;
    }
    setTimeout(() => {
      map.delete(key);
    }, TIMEOUT);
    const headers = new Headers(r.headers);
    headers.set("x-dedupe-at", new Date().valueOf().toString());
    headers.set("x-dedupe-key", key);
    return new Response(r.body, {
      status: r.status,
      statusText: r.statusText,
      headers,
    });
  });
  map.set(key, promise);
  return promise.then((r) => r.clone());
}

self.addEventListener("install", () => self.skipWaiting());

/** @type {ServiceWorkerGlobalScope} */
(self).addEventListener("fetch", (e) => {
  if (
    e.request.method.toLocaleLowerCase() !== "get" ||
    !e.request.headers.get("is-api")
  )
    return;
  e.respondWith(getResponse(e.request, e.clientId));
});
