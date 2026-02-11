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
import { useContactmomentStore } from "@/stores/contactmoment/index";
import { KlantDetails } from "@/features/contact/contactBehandelen/vraagBehandelen/klantgerelateerdeGegevensInzien/bedrijvenEnPersonen/_shared/klant-details";
import { TabList, TabListItem } from "@/components/tabs";
import BackLink from "@/components/BackLink.vue";
import { BrpGegevens } from "@/features/contact/contactBehandelen/vraagBehandelen/klantgerelateerdeGegevensInzien/bedrijvenEnPersonen/personen/persoon-details";
import ContactmomentenForKlantIdentificator from "@/features/contact/contactBehandelen/vraagBehandelen/klantgerelateerdeGegevensInzien/bedrijvenEnPersonen/_shared/ContactmomentenForKlantIdentificator.vue";
import ContactverzoekenForKlantIdentificator from "@/features/contact/contactBehandelen/vraagBehandelen/klantgerelateerdeGegevensInzien/bedrijvenEnPersonen/_shared/ContactverzoekenForKlantIdentificator.vue";

import type { Persoon } from "@/services/brp";
import ZakenForKlant from "@/features/contact/contactBehandelen/vraagBehandelen/klantgerelateerdeGegevensInzien/zaken/ZakenForKlant.vue";

defineProps<{ internalKlantId: string }>();
const activeTab = ref("");
const contactmomentStore = useContactmomentStore();
const persoon = ref<Persoon>();
</script>
