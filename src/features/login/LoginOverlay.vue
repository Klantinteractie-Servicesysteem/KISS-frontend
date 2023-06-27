<template>
  <simple-spinner v-if="currentUserState.loading" />
  <template v-else>
    <slot v-if="initialized" :onLogout="onLogout"></slot>
    <dialog ref="dialogRef" @keyup.escape.prevent @keydown.escape.prevent>
      <a
        :href="redirectUrl"
        target="_blank"
        @click="onLinkClick"
        @keydown.enter="onLinkClick"
        >Je sessie is verlopen. Klik in het scherm om opnieuw in te loggen. Als
        je dit binnen {{ loginTimeoutInSeconds }} seconden doet, verlies je geen
        werk.</a
      >
    </dialog>
  </template>
</template>

<script lang="ts" setup>
import { watch, computed, ref } from "vue";
import { useCurrentUser } from "./service";
import SimpleSpinner from "@/components/SimpleSpinner.vue";
import { handleLogin } from "@/services";
import { loginUrl, redirectUrl, sessionStorageKey } from "./config";
import { useUserStore, type User } from "@/stores/user";
import { logoutUrl } from "./config";

let newTab: Window | null = null;

const loginTimeoutInSeconds = 60;

const messageTypes = {
  refresh: "refresh",
  closeTab: "closeTab",
} as const;

function tryCreateNewTab() {
  if (newTab && !newTab.closed) {
    newTab.focus();
    return true;
  }
  try {
    newTab = window.open(redirectUrl);
  } catch {
    // popups are probably blocked
  }

  return !!newTab;
}

function tryCloseTab() {
  if (newTab && !newTab.closed) {
    newTab.close();
  }
  newTab = null;
}

function refresh() {
  currentUserState.refresh();
}

// this channel is used to communicate between browser tabs/windows
const channel = new BroadcastChannel(
  // unique name per environment
  "kiss-close-tab-channel-" + window.location.host
);

channel.onmessage = (e) => {
  switch (e.data) {
    case messageTypes.closeTab:
      tryCloseTab();
      break;

    case messageTypes.refresh:
      refresh();
      break;
  }
};

const dialogRef = ref<HTMLDialogElement>();

const currentUserState = useCurrentUser();
const userStore = useUserStore();

const initialized = ref(false);

const isLoggedInRef = computed(
  () => currentUserState.success && currentUserState.data.isLoggedIn
);

const isLoadingRef = computed(() => currentUserState.loading);

function onLogin() {
  initialized.value = true;

  handleLogin();
  channel.postMessage(messageTypes.refresh);
  channel.postMessage(messageTypes.closeTab);

  // session storage is owned per tab.
  // this value is set on the /redirect-to-login page.
  // if the value is present, it means this tab got redirected from that page.
  // this means the user has KISS open in another tab, so we can close this one.
  // if the user allows popups, we can do this automatically from the parent.
  // just in case popups are blocked, we indicate to the user that they can do this themselves.
  if (sessionStorage.getItem(sessionStorageKey)) {
    sessionStorage.removeItem(sessionStorageKey);
    document.body.innerHTML =
      "<p>Je bent ingelogd. Je kunt dit tabblad sluiten.</p>";
    return;
  }

  if (dialogRef.value) {
    dialogRef.value.close();
  }
}

function onLogout(e: Event) {
  e.preventDefault();
  channel.postMessage(messageTypes.refresh);
  location.href = logoutUrl;
}

function onLinkClick(e: Event) {
  if (tryCreateNewTab()) {
    e.preventDefault();
  }
  // if we can't open a new tab, handle the link normally
}

function redirectToLogin() {
  window.location.href = loginUrl;
}

// in case of a session expiry, you have a minute to log in again.
// after that, we refresh the page.
// otherwise, an unauthorized person might be able to see sensitive data by inspecting the HTML with DevTools.
let currentLoginTimoutId: number | undefined;
function resetLoginTimeout() {
  if (currentLoginTimoutId) {
    clearTimeout(currentLoginTimoutId);
  }
  currentLoginTimoutId = setTimeout(() => {
    if (!isLoggedInRef.value) {
      location.reload();
    }
  }, loginTimeoutInSeconds * 1000);
}

watch(
  [isLoadingRef, isLoggedInRef, dialogRef, initialized],
  ([loading, isLoggedIn, dialog, isInitialized]) => {
    if (loading) return;
    if (isLoggedIn) {
      onLogin();
      return;
    }

    // not logged in
    if (!isInitialized) {
      // this is the first time you open this window.
      // we can immediately redirect to login.
      redirectToLogin();
      return;
    }

    // you were logged in, but got logged out in another window or your session expired
    // the dialog element should be in the dom by now, so it shouldn't be undefined
    // the if is just there for type safety
    if (!dialog) {
      console.error(
        "we expected a dialog in the dom, but it seems to be missing..."
      );
    }

    resetLoginTimeout();
    dialog?.showModal();
  }
);

const computedUser = computed<User>(() =>
  !currentUserState.success ? { isLoggedIn: false } : currentUserState.data
);

watch(computedUser, (u) => {
  userStore.setUser(u);
});
</script>

<style lang="scss" scoped>
dialog[open] {
  width: 100%;
  height: 100%;
  display: grid;
  align-content: stretch;
  justify-content: stretch;

  a {
    display: grid;
    justify-content: center;
    align-content: center;
  }
}
</style>
