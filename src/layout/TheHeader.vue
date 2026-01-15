<template>
  <login-overlay>
    <template #default="{ onLogout }">
      <header
        :class="{
          contactmomentLoopt: contactmomentStore.contactmomentLoopt,
          'hide-sidebar': $route.meta.hideSidebar,
        }"
      >
        <global-search
          class="search-bar"
          v-if="(isKcm || isKennisbank) && route.meta.showSearch"
        />

        <nav>
          <ul>
            <template v-if="isKcm">
              <li
                v-if="
                  contactmomentStore.contactmomentLoopt && route.meta.showNav
                "
              >
                <router-link :to="{ name: 'contactverzoeken' }"
                  ><span>Contactverzoeken</span></router-link
                >
              </li>
              <li
                v-if="
                  contactmomentStore.contactmomentLoopt && route.meta.showNav
                "
              >
                <router-link :to="{ name: 'personen' }"
                  ><span>Personen</span></router-link
                >
              </li>
              <li
                v-if="
                  contactmomentStore.contactmomentLoopt && route.meta.showNav
                "
              >
                <router-link :to="{ name: 'bedrijven' }"
                  ><span>Bedrijven</span></router-link
                >
              </li>
              <li
                v-if="
                  contactmomentStore.contactmomentLoopt && route.meta.showNav
                "
              >
                <router-link :to="{ name: 'zaken' }"
                  ><span>Zaken</span></router-link
                >
              </li>
            </template>
            <template v-if="isKcm || isRedacteur || canBeheer">
              <li v-if="route.meta.showNav">
                <router-link :to="{ name: 'home' }">
                  <span>Nieuws en werkinstructies</span>
                  <span
                    v-if="featuredWerkberichtenCount"
                    class="featured-indicator"
                    >{{
                      featuredWerkberichtenCount < 10
                        ? featuredWerkberichtenCount
                        : "9+"
                    }}</span
                  >
                </router-link>
              </li>
              <li v-if="route.meta.showNav">
                <router-link :to="{ name: 'links' }"
                  ><span>Links</span></router-link
                >
              </li>
            </template>
            <li
              v-if="
                canBeheer &&
                !contactmomentStore.contactmomentLoopt &&
                route.meta.showNav
              "
            >
              <router-link :to="{ name: 'Beheer' }">
                <span>Beheer</span>
              </router-link>
            </li>
            <li class="log-out">
              <a
                :href="logoutUrl"
                @click="onLogout"
                @keydown.enter="onLogout"
                class="icon-before exit"
                >Uitloggen</a
              >
            </li>
          </ul>
        </nav>
      </header>
    </template>
  </login-overlay>
</template>

<script lang="ts" setup>
import { fetchFeaturedWerkberichten } from "@/features/werkbericht";
import { useContactmomentStore } from "@/stores/contactmoment";
import { useRoute } from "vue-router";
import { LoginOverlay, logoutUrl } from "../features/login";
import GlobalSearch from "../features/search/GlobalSearch.vue";
import { computed } from "vue";
import { useUserStore } from "@/stores/user";
import { storeToRefs } from "pinia";
import { useLoader } from "@/services";

const route = useRoute();

const userStore = useUserStore();
const { user } = storeToRefs(userStore);

const contactmomentStore = useContactmomentStore();

const { data: featuredWerkberichtenCount } = useLoader(() => {
  if (userStore.user.isKcm || userStore.user.isRedacteur) {
    return fetchFeaturedWerkberichten();
  }
});

const canBeheer = computed(
  () => user.value.isLoggedIn && user.value.permissions.includes("Beheer"),
);

const isRedacteur = computed(
  () => user.value.isLoggedIn && user.value.isRedacteur,
);

const isKcm = computed(() => user.value.isLoggedIn && user.value.isKcm);
const isKennisbank = computed(
  () => user.value.isLoggedIn && user.value.isKennisbank,
);
</script>

<style lang="scss" scoped>
header {
  background-color: var(--color-primary);
  display: grid;
  grid-template-areas:
    "bar bar"
    "results scroll"
    "expand expand"
    "nav nav";
  grid-template-columns: auto 6rem;
  align-items: center;

  &:has(.search-results) {
    background-color: var(--color-secondary);
  }

  .log-out {
    margin-inline-start: auto;
  }
}

nav {
  grid-area: nav;
}

nav ul {
  width: 100%;
  background-color: var(--color-primary);
  display: flex;
  flex-wrap: wrap;
  gap: var(--spacing-default);
  padding-block: var(--spacing-small);
  list-style: none;

  li {
    margin-inline: var(--spacing-large);
  }

  a {
    display: flex;
    gap: var(--spacing-small);
    block-size: 1.8rem;
    align-items: center;
    text-decoration: none;
    color: var(--color-white);

    &.router-link-active > :first-child {
      border-bottom: 2px solid var(--color-white);
    }
  }

  .featured-indicator {
    display: flex;
    align-items: center;
    justify-content: center;
    color: var(--color-black);
    block-size: 100%;
    inline-size: 1.8rem;
    background-color: var(--color-error);
    border-radius: calc(var(--spacing-large) / 2);
  }
}
</style>
