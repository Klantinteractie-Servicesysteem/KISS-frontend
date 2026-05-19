//import { useCurrentUser } from "@/features/login";
import { useUserStore } from "@/stores/user";

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

export function setHeader(init: RequestInit, key: string, value: string) {
  if (!init.headers) {
    init.headers = {};
  }

  if (Array.isArray(init.headers)) {
    init.headers.push([key, value]);
  } else if (init.headers instanceof Headers) {
    init.headers.set(key, value);
  } else {
    init.headers[key] = value;
  }
}

export function fetchLoggedIn(...args: FetchArgs): FetchReturn {
  const init = args[1] || {};
  args[1] = init;

  setHeader(init, "is-api", "true");

  return fetch(...args).then((r) => {
    if (r.status === 401) {
      //if we are getting 401 results on ajax calls, the users session has ended
      //(or the user tries to do something he/she isn't supposed to do)

      // refetching the current user will ..
      // result in a user with .isloggedin set to false.
      // That will trigger a watcher in the LoginOverlay.
      // The LoginOverlay will make itself visible and
      // it will help the user to login again.

      const userStore = useUserStore();
      userStore.setSessionExpired();

      console.warn("session expired. waiting for user to log in...");
      return waitForLogin.promise.then(() => {
        console.log("user is logged in again, resuming...");
        return fetchLoggedIn(...args);
      });
    }
    return r;
  });
}
