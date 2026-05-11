import type { RouteRecordRaw } from "vue-router";
import RedirectPage from "./RedirectPage.vue";

export const meUrl = "/api/me";
export const loginUrl = "/api/challenge";
export const logoutUrl = "/api/logoff";
export const redirectUrl = "/redirect-to-login";
export const sessionStorageKey = "kiss_close";

export const redirectRoute = {
  component: RedirectPage,
  path: redirectUrl,
  beforeEnter() {
    // session storage is owned per tab.
    // we use this to check if we redirected to the login provider from this page.
    // this value is read in the LoginOverlay after a successful login.
    sessionStorage.setItem(sessionStorageKey, "true");
    window.location.href = loginUrl;
  },
} as RouteRecordRaw;
