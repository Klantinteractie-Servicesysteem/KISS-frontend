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
        <router-view />
      </main>
    </div>
  </switchable-store-provider>
</template>

<script setup lang="ts">
import { RouterView, useRoute } from "vue-router";
import { useContactmomentStore } from "@/stores/contactmoment";
import TheToastSection from "@/components/TheToastSection.vue";
import TheSidebar from "./layout/TheSidebar.vue";
import TheHeader from "./layout/TheHeader.vue";
import { SwitchableStoreProvider } from "./stores/switchable-store";

const contactmomentStore = useContactmomentStore();
const route = useRoute();
</script>

<style lang="scss">
/* Design Tokens */
@import "@utrecht/component-library-css";
@import "@/assets/design-tokens";
@import "@/assets/fonts/fonts.css";
@import "@/assets/reset.css";
@import "@/assets/main";
</style>
