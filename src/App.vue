<template>
  <switchable-store-provider>
    <the-toast-section />
    <div
      :class="[
        'app-layout',
        {
          'contactmoment-loopt': contactmomentStore.contactmomentLoopt,
          'hide-sidebar': route.meta.hideSidebar,
        },
      ]"
    >
      <the-header />
      <the-sidebar />
      <main>
        <router-view>
          <template #default="{ Component }">
            <back-link-provider>
              <component :is="Component" />
            </back-link-provider>
          </template>
        </router-view>
        <div v-if="buildInfo" class="versienummer">
          Versie {{ buildInfo.version }} | Commit {{ buildInfo.commit }}
        </div>
      </main>
    </div>
  </switchable-store-provider>
</template>

<script setup lang="ts">
import { RouterView, useRoute } from "vue-router";
import { useContactmomentStore } from "@/stores/contactmoment/index";
import TheToastSection from "@/components/TheToastSection.vue";
import TheSidebar from "./layout/TheSidebar.vue";
import TheHeader from "./layout/TheHeader.vue";
import { SwitchableStoreProvider } from "./stores/switchable-store";
import BackLinkProvider from "./components/BackLinkProvider.vue";
import { fetchLoggedIn, parseJson, throwIfNotOk } from "./services";
import { useLoader } from "./services/use-loader";

const contactmomentStore = useContactmomentStore();
const route = useRoute();

const { data: buildInfo } = useLoader(() =>
  fetchLoggedIn("/api/environment/build-info")
    .then(throwIfNotOk)
    .then(parseJson)
    .then(({ buildInfo }: { buildInfo?: string }) => {
      const [version, commit] = buildInfo?.split("+") || [];
      return {
        version: version || "onbekend",
        commit: commit?.replace(/^\./, "") || "onbekend",
      };
    })
    .catch(() => ({
      version: "onbekend",
      commit: "onbekend",
    })),
);
</script>

<style lang="scss">
/* Design Tokens */
@use "@utrecht/component-library-css";
@use "@/assets/design-tokens";
@use "@/assets/fonts/fonts.css";
@use "@/assets/reset.css";
@use "@/assets/main";
</style>
