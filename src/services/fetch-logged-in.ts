type FetchArgs = Parameters<typeof fetch>;
type FetchReturn = ReturnType<typeof fetch>;

const empty = () => {};

const waitForLogin = {
  promise: Promise.resolve(),
  resolve: empty,
};

(function refreshPromise() {
  const promise = new Promise<void>((resolve) => {
    waitForLogin.resolve = resolve;
  });
  waitForLogin.promise = promise;
  // will keep refreshing the promise whenever it resolves,
  // which is done when succesfully logged in.
  // this causes all pending 401 requests to retry,
  // but new 401 requests to wait for the new promise.
  promise.finally(refreshPromise);
})();

export function handleLogin() {
  waitForLogin.resolve();
}

export function getHeader(init: RequestInit, key: string) {
  if (!(init.headers && init.headers instanceof Headers)) {
    init.headers = new Headers(init.headers);
  }
  return init.headers.get(key);
}

export function setHeader(init: RequestInit, key: string, value: string) {
  if (!(init.headers && init.headers instanceof Headers)) {
    init.headers = new Headers(init.headers);
  }
  init.headers.set(key, value);
}

export function fetchLoggedIn(...args: FetchArgs): FetchReturn {
  const init = args[1] || {};
  args[1] = init;

  setHeader(init, "is-api", "true");

  return fetch(...args).then((r) => {
    if (r.status === 401) {
      console.warn("session expired. waiting for user to log in...");
      return waitForLogin.promise.then(() => {
        console.log("user is logged in again, resuming...");
        return fetchLoggedIn(...args);
      });
    }
    return r;
  });
}
