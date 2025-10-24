<template>
  <simple-spinner v-if="loading" />
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
import { ref, watch } from "vue";
import SimpleSpinner from "@/components/SimpleSpinner.vue";
import { handleLogin } from "@/services";
import { loginUrl, redirectUrl, sessionStorageKey } from "./config";
import { useUserStore } from "@/stores/user";
import { logoutUrl } from "./config";
import { meUrl } from "./config";
import { fetchUser } from "./service";

let newTab: Window | null = null;

const loading = ref<boolean>(true);

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

async function refreshUser() {
  try {
    loading.value = true;
    const fetchedUser = await fetchUser(meUrl);
    userStore.setUser(fetchedUser);
    initialized.value = true;
  } finally {
    loading.value = false;
  }
}

// this channel is used to communicate between browser tabs/windows
const channel = new BroadcastChannel(
  // unique name per environment
  "kiss-close-tab-channel-" + window.location.host,
);

channel.onmessage = async (e) => {
  switch (e.data) {
    case messageTypes.closeTab:
      tryCloseTab();
      break;
    case messageTypes.refresh:
      await refreshUser();
      break;
  }
};

const dialogRef = ref<HTMLDialogElement>();

const userStore = useUserStore();

const initialized = ref(false);

function onLogin() {
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
let currentLoginTimoutId: number | NodeJS.Timeout;
function resetLoginTimeout() {
  if (currentLoginTimoutId) {
    clearTimeout(currentLoginTimoutId);
  }
  currentLoginTimoutId = setTimeout(() => {
    if (!userStore.user.isLoggedIn) {
      location.reload();
    }
  }, loginTimeoutInSeconds * 1000);
}

watch(
  [() => userStore.user.isLoggedIn, initialized],
  ([isLoggedIn], [wasLoggedIn]) => {
    if (isLoggedIn) {
      onLogin();
      return;
    }

    // not logged in
    if (!wasLoggedIn) {
      // this is the first time you open this window.
      // we can immediately redirect to login.
      redirectToLogin();
      return;
    }

    // you were logged in, but got logged out in another window or your session expired
    // the dialog element should be in the dom by now, so it shouldn't be undefined
    // the if is just there for type safety
    if (!dialogRef.value) {
      console.error(
        "we expected a dialog in the dom, but it seems to be missing...",
      );
    }
    resetLoginTimeout();
    dialogRef.value?.showModal();
  },
);

refreshUser();
</script>

<style lang="scss" scoped>
dialog[open] {
  width: 100%;
  height: 100%;
  display: grid;
  place-content: stretch stretch;

  a {
    display: grid;
    place-content: center center;
  }
}
</style>
