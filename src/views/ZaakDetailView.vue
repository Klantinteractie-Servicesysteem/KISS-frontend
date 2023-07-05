<template>
  <article>
    <simple-spinner v-if="zaak.loading" />

    <application-message
      v-if="zaak.error"
      messageType="error"
      message="Er kon geen zaak gevonden worden"
    ></application-message>

    <section v-if="zaak.success">
      <div class="header">
        <utrecht-heading :level="1"
          >Zaak {{ zaak.data.identificatie }}
        </utrecht-heading>
        <router-link :to="{ name: 'zaken' }">{{
          "< Zaken zoeken"
        }}</router-link>
      </div>

      <tabs-component v-model="activeTab">
        <template #[Tabs.algemeen]>
          <zaak-algemeen :zaak="zaak.data" />
        </template>

        <template #[Tabs.documenten]>
          <zaak-documenten :zaak="zaak.data" />
        </template>

        <template #[Tabs.contactmomenten]>
          <section
            v-if="contactmomenten.success && contactmomenten.data"
            class="contactmomenten"
          >
            <utrecht-heading :level="2"> Contactmomenten </utrecht-heading>

            <contactmomenten-overzicht
              :contactmomenten="contactmomenten.data.page"
            >
              <template v-slot:zaken="{ zaken }">
                <template v-for="zaakurl in zaken" :key="zaakurl">
                  <zaak-preview :zaakurl="zaakurl"></zaak-preview>
                </template>
              </template>
            </contactmomenten-overzicht>
          </section>
        </template>
      </tabs-component>

      <div class="toelichting">
        <zaak-toelichting :zaak="zaak.data" @success="onNotitieUpdate" />
      </div>
    </section>
  </article>
</template>

<script setup lang="ts">
import { useZaakById } from "@/features/zaaksysteem/service";
import ApplicationMessage from "@/components/ApplicationMessage.vue";
import SimpleSpinner from "@/components/SimpleSpinner.vue";
import { computed, ref, watch } from "vue";
import { useContactmomentStore } from "@/stores/contactmoment";
import ZaakToelichting from "@/features/zaaksysteem/components/ZaakToelichting.vue";
import ZaakDocumenten from "@/features/zaaksysteem/components/ZaakDocumenten.vue";
import ZaakAlgemeen from "@/features/zaaksysteem/components/ZaakAlgemeen.vue";
import { Heading as UtrechtHeading } from "@utrecht/component-library-vue";
import TabsComponent from "@/components/TabsComponent.vue";
import {
  ContactmomentenOverzicht,
  useContactmomentenByObjectUrl,
} from "@/features/contactmoment";
import ZaakPreview from "@/features/zaaksysteem/components/ZaakPreview.vue";
const props = defineProps<{ zaakId: string }>();
const contactmomentStore = useContactmomentStore();
const zaak = useZaakById(computed(() => props.zaakId));
const zaakUrl = computed(() =>
  zaak.success && zaak.data.self ? zaak.data.self : ""
);

const contactmomenten = useContactmomentenByObjectUrl(zaakUrl);

const Tabs = {
  algemeen: "Algemeen",
  documenten: "Documenten",
  contactmomenten: "Contactmomenten",
} as const;

const activeTab = ref(Tabs.algemeen);

watch(
  () => zaak.success && zaak.data,
  (z) => {
    if (!z || !contactmomentStore.huidigContactmoment) return;
    contactmomentStore.upsertZaak(
      z,
      contactmomentStore.huidigContactmoment.huidigeVraag
    );
  },
  { immediate: true }
);

const onNotitieUpdate = () => zaak.refresh();
</script>

<style lang="scss" scoped>
.contactmomenten {
  padding: var(--spacing-large);

  > :first-child {
    margin-block-end: var(--spacing-large);
  }
}
</style>
