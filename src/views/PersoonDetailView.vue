<template>
  <back-link />

  <utrecht-heading :level="1">Persoonsinformatie</utrecht-heading>
  <tab-list v-model="activeTab">
    <tab-list-item label="Contactgegevens">
      <template #default="{ setError, setDisabled }">
        <klant-details
          :internalKlantId="internalKlantId"
          @no-data="setDisabled(true)"
          @load="setDisabled(false)"
          @error="setError"
        />
      </template>
    </tab-list-item>

    <tab-list-item label="BRP gegevens">
      <template #default="{ setError, setLoading }">
        <brp-gegevens
          :internalKlantId="internalKlantId"
          @load="persoon = $event"
          @loading="setLoading"
          @error="setError"
        />
      </template>
    </tab-list-item>

    <tab-list-item label="Contactmomenten">
      <template #default="{ setError, setLoading, setDisabled }">
        <utrecht-heading :level="2"> Contactmomenten </utrecht-heading>

        <contactmomenten-for-klant-identificator
          v-if="persoon"
          :klant-identificator="persoon"
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
          v-if="persoon"
          :klant-identificator="persoon"
          :vraag="contactmomentStore.huidigContactmoment?.huidigeVraag"
          @load="setDisabled(!$event?.length)"
          @loading="setLoading"
          @error="setError"
        />
      </template>
    </tab-list-item>

    <tab-list-item label="Contactverzoeken">
      <template #default="{ setError, setLoading, setDisabled }">
        <utrecht-heading :level="2">Contactverzoeken</utrecht-heading>

        <contactverzoeken-for-klant-identificator
          v-if="persoon"
          :klant-identificator="persoon"
          @loading="setLoading"
          @error="setError"
          @load="setDisabled(!$event.length)"
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
import { BrpGegevens } from "@/features/persoon/persoon-details";
import ContactmomentenForKlantIdentificator from "@/features/contact/contactmoment/ContactmomentenForKlantIdentificator.vue";
import ContactverzoekenForKlantIdentificator from "@/features/contact/contactverzoek/overzicht/ContactverzoekenForKlantIdentificator.vue";

import type { Persoon } from "@/services/brp";
import ZakenForKlant from "@/features/zaaksysteem/ZakenForKlant.vue";

defineProps<{ internalKlantId: string }>();
const activeTab = ref("");
const contactmomentStore = useContactmomentStore();
const persoon = ref<Persoon>();
</script>
