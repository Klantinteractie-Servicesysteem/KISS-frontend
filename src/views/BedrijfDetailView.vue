<template>
  <back-link />
  <utrecht-heading :level="1">Bedrijfsinformatie</utrecht-heading>
  <tab-list v-model="currentTab">
    <tab-list-item label="Contactgegevens">
      <template #default="{ setError, setLoading, setDisabled }">
        <klant-details
          :internalKlantId="internalKlantId"
          @no-data="setDisabled(true)"
          @load="setDisabled(false)"
          @loading="setLoading"
          @error="setError"
        />
      </template>
    </tab-list-item>
    <tab-list-item label="KvK-gegevens">
      <template #default="{ setError, setLoading }">
        <handelsregister-gegevens
          :internalKlantId="internalKlantId"
          @load="bedrijf = $event"
          @loading="setLoading"
          @error="setError"
        />
      </template>
    </tab-list-item>

    <tab-list-item label="Contactmomenten">
      <template #default="{ setError, setLoading, setDisabled }">
        <contactmomenten-for-klant-identificator
          v-if="bedrijf"
          :klant-identificator="bedrijf"
          @load="setDisabled(!$event?.length)"
          @loading="setLoading"
          @error="setError"
        />
      </template>
    </tab-list-item>

    <tab-list-item label="Zaken">
      <template #default="{ setError, setLoading, setDisabled }">
        <utrecht-heading :level="2"> Zaken </utrecht-heading>

        <zaken-for-klant
          v-if="bedrijf"
          :klant-identificator="bedrijf"
          :vraag="contactmomentStore.huidigContactmoment?.huidigeVraag"
          @load="setDisabled(!$event?.length)"
          @loading="setLoading"
          @error="setError"
        />
      </template>
    </tab-list-item>

    <tab-list-item label="Contactverzoeken">
      <template #default="{ setError, setLoading, setDisabled }">
        <contactverzoeken-for-klant-identificator
          v-if="bedrijf"
          :klant-identificator="bedrijf"
          @load="setDisabled(!$event?.length)"
          @loading="setLoading"
          @error="setError"
        />
      </template>
    </tab-list-item>
  </tab-list>
</template>

<script setup lang="ts">
import { ref } from "vue";
import { Heading as UtrechtHeading } from "@utrecht/component-library-vue";
import { useContactmomentStore } from "@/stores/contactmoment";
import { KlantDetails } from "@/features/klant/klant-details";
import { TabList, TabListItem } from "@/components/tabs";
import BackLink from "@/components/BackLink.vue";
import { HandelsregisterGegevens } from "@/features/bedrijf/bedrijf-details";
import type { Bedrijf } from "@/services/kvk";
import ContactverzoekenForKlantIdentificator from "@/features/contact/contactverzoek/overzicht/ContactverzoekenForKlantIdentificator.vue";
import ContactmomentenForKlantIdentificator from "@/features/contact/contactmoment/ContactmomentenForKlantIdentificator.vue";
import ZakenForKlant from "@/features/zaaksysteem/ZakenForKlant.vue";

defineProps<{ internalKlantId: string }>();
const contactmomentStore = useContactmomentStore();
const currentTab = ref("");
const bedrijf = ref<Bedrijf>();
</script>
