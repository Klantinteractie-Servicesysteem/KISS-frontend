<template>
  <simple-spinner v-if="zaak.loading" />

  <application-message
    v-if="zaak.error"
    messageType="error"
    message="Er kon geen zaak gevonden worden"
  ></application-message>

  <template v-if="zaak.success">
    <back-link />

    <header class="zaak-header">
      <utrecht-heading :level="1"
        >Zaak {{ zaak.data.identificatie }}</utrecht-heading
      >
      <a
        v-if="deeplink"
        :href="deeplink"
        target="_blank"
        rel="noopener noreferrer"
        >Open in zaaksysteem</a
      >
    </header>

    <tab-list v-model="activeTab" v-if="zaaksysteemId">
      <tab-list-item label="Algemeen">
        <zaak-algemeen :zaak="zaak.data" />
      </tab-list-item>
      <tab-list-item
        label="Documenten"
        :disabled="!zaak.data.documenten?.length"
      >
        <zaak-documenten :zaak="zaak.data" />
      </tab-list-item>
      <tab-list-data-item
        label="Contactmomenten"
        :data="contactmomenten"
        :disabled="(c) => !c.count"
      >
        <template #success="{ data }">
          <div class="contactmomenten">
            <utrecht-heading :level="2"> Contactmomenten </utrecht-heading>
            <contactmomenten-overzicht :contactmomenten="data.page">
              <template #object="{ object }">
                <zaak-preview :zaakurl="object.object"></zaak-preview>
              </template>
            </contactmomenten-overzicht>
          </div>
        </template>
      </tab-list-data-item>
    </tab-list>
  </template>
</template>

<script setup lang="ts">
import { useZaakById } from "@/features/zaaksysteem/service";
import ApplicationMessage from "@/components/ApplicationMessage.vue";
import SimpleSpinner from "@/components/SimpleSpinner.vue";
import { computed, ref, watch } from "vue";
import { useContactmomentStore } from "@/stores/contactmoment";
import ZaakDocumenten from "@/features/zaaksysteem/components/ZaakDocumenten.vue";
import ZaakAlgemeen from "@/features/zaaksysteem/components/ZaakAlgemeen.vue";
import { Heading as UtrechtHeading } from "@utrecht/component-library-vue";
import {
  ContactmomentenOverzicht,
  useContactmomentenByObjectUrl,
} from "@/features/contact/contactmoment";
import ZaakPreview from "@/features/zaaksysteem/components/ZaakPreview.vue";
import { TabList, TabListItem, TabListDataItem } from "@/components/tabs";
import BackLink from "@/components/BackLink.vue";
import { useZaaksysteemDeeplink } from "@/features/zaaksysteem/deeplink";

const props = defineProps<{ zaakId: string; zaaksysteemId: string }>();
const contactmomentStore = useContactmomentStore();
const zaak = useZaakById(
  computed(() => props.zaakId),
  computed(() => props.zaaksysteemId),
);
const zaakUrl = computed(() =>
  zaak.success && zaak.data.self ? zaak.data.self : "",
);

const contactmomenten = useContactmomentenByObjectUrl(zaakUrl);

const activeTab = ref("");

const deeplink = useZaaksysteemDeeplink(() =>
  zaak.success ? zaak.data : undefined,
);

watch(
  () => zaak.success && zaak.data,
  (z) => {
    if (!z || !contactmomentStore.huidigContactmoment) return;
    contactmomentStore.upsertZaak(
      z,
      contactmomentStore.huidigContactmoment.huidigeVraag,
    );
  },
  { immediate: true },
);
</script>

<style lang="scss" scoped>
.zaak-header {
  display: flex;
  flex-wrap: wrap;
  gap: var(--spacing-default);
  align-items: center;
}
</style>
