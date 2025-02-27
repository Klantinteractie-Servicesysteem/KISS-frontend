<template>
  <simple-spinner v-if="loading" />

  <application-message
    v-if="error"
    messageType="error"
    message="Er kon geen zaak gevonden worden"
  ></application-message>

  <template v-if="zaak">
    <back-link />

    <header class="zaak-header">
      <utrecht-heading :level="1"
        >Zaak {{ zaak.identificatie }}</utrecht-heading
      >
      <zaak-deeplink :zaak="zaak" :systeem="systeem" v-if="systeem" />
    </header>

    <tab-list v-model="activeTab" v-if="zaaksysteemId">
      <tab-list-item label="Algemeen">
        <zaak-algemeen :zaak="zaak" />
      </tab-list-item>
      <tab-list-item label="Documenten" :disabled="!zaak.documenten?.length">
        <zaak-documenten :zaak="zaak" />
      </tab-list-item>
      <tab-list-item label="Contactmomenten">
        <template #default="{ setError, setLoading, setDisabled }">
          <div class="contactmomenten">
            <utrecht-heading :level="2"> Contactmomenten </utrecht-heading>
            <contactmomenten-for-object-url
              v-if="zaakUrl"
              :object-url="zaakUrl"
              @load="setDisabled(!$event.count)"
              @loading="setLoading"
              @error="setError"
            >
              <template #object="{ object }">
                <zaak-preview
                  :zaakurl="object.object"
                  :systeem-id="zaaksysteemId"
                />
              </template>
            </contactmomenten-for-object-url>
          </div>
        </template>
      </tab-list-item>
    </tab-list>
  </template>
</template>

<script setup lang="ts">
import { fetchZaakDetailsById } from "@/features/zaaksysteem/service";
import ApplicationMessage from "@/components/ApplicationMessage.vue";
import SimpleSpinner from "@/components/SimpleSpinner.vue";
import { computed, ref, watch } from "vue";
import { useContactmomentStore } from "@/stores/contactmoment";
import ZaakDocumenten from "@/features/zaaksysteem/components/ZaakDocumenten.vue";
import ZaakAlgemeen from "@/features/zaaksysteem/components/ZaakAlgemeen.vue";
import { Heading as UtrechtHeading } from "@utrecht/component-library-vue";
import ZaakPreview from "@/features/zaaksysteem/components/ZaakPreview.vue";
import ZaakDeeplink from "@/features/zaaksysteem/components/ZaakDeeplink.vue";
import { TabList, TabListItem } from "@/components/tabs";
import BackLink from "@/components/BackLink.vue";
import ContactmomentenForObjectUrl from "@/features/contact/contactmoment/ContactmomentenForObjectUrl.vue";
import { useSystemen } from "@/services/environment/fetch-systemen";
import { useLoader } from "@/services";

const props = defineProps<{ zaakId: string; zaaksysteemId: string }>();

const contactmomentStore = useContactmomentStore();

const { systemen } = useSystemen();
const systeem = computed(() =>
  systemen.value?.find((x) => x.identifier === props.zaaksysteemId),
);

const {
  data: zaak,
  loading,
  error,
} = useLoader(() => {
  if (props.zaakId && systeem.value)
    return fetchZaakDetailsById(props.zaakId, systeem.value);
});

const zaakUrl = computed(() => zaak.value?.self || "");

const activeTab = ref("");

watch(
  zaak,
  (z) => {
    if (!z || !contactmomentStore.huidigContactmoment) return;
    contactmomentStore.upsertZaak(
      z,
      contactmomentStore.huidigContactmoment.huidigeVraag,
      true,
      props.zaaksysteemId,
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
