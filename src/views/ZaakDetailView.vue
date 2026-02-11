<template>
  <simple-spinner v-if="loading" />

  <application-message
    v-if="error"
    messageType="error"
    message="Er kon geen zaak gevonden worden"
  ></application-message>

  <template v-if="zaak && zaaksysteemId">
    <back-link />

    <header class="zaak-header">
      <utrecht-heading :level="1"
        >Zaak {{ zaak.identificatie }}</utrecht-heading
      >
      <zaak-deeplink :zaak="zaak" :systeem-id="zaaksysteemId" />
    </header>

    <tab-list v-model="activeTab">
      <tab-list-item label="Algemeen">
        <zaak-algemeen :zaak="zaak" />
      </tab-list-item>

      <tab-list-item label="Documenten">
        <template #default="{ setDisabled, setLoading, setError }">
          <zaak-documenten
            :zaak-url="zaak.url"
            :systeem-id="zaaksysteemId"
            @load="setDisabled(!$event.length)"
            @loading="setLoading"
            @error="setError"
          />
        </template>
      </tab-list-item>

      <tab-list-item label="Contactmomenten">
        <template #default="{ setError, setLoading, setDisabled }">
          <div class="contactmomenten">
            <contactmomenten-for-object-url
              :object-url="zaak.url"
              :systeem-id="zaaksysteemId"
              @load="setDisabled(!$event.count)"
              @loading="setLoading"
              @error="setError"
            />
          </div>
        </template>
      </tab-list-item>
    </tab-list>
  </template>
</template>

<script setup lang="ts">
import { fetchZaakDetailsById } from "@/features/contact/contactBehandelen/vraagBehandelen/klantgerelateerdeGegevensInzien/zaken/service";
import ApplicationMessage from "@/components/ApplicationMessage.vue";
import SimpleSpinner from "@/components/SimpleSpinner.vue";
import { computed, ref, watch } from "vue";
import { useContactmomentStore } from "@/stores/contactmoment/index";
import ZaakDocumenten from "@/features/contact/contactBehandelen/vraagBehandelen/klantgerelateerdeGegevensInzien/zaken/components/ZaakDocumenten.vue";
import ZaakAlgemeen from "@/features/contact/contactBehandelen/vraagBehandelen/klantgerelateerdeGegevensInzien/zaken/components/ZaakAlgemeen.vue";
import { Heading as UtrechtHeading } from "@utrecht/component-library-vue";
import ZaakDeeplink from "@/features/contact/contactBehandelen/vraagBehandelen/klantgerelateerdeGegevensInzien/zaken/components/ZaakDeeplink.vue";
import { TabList, TabListItem } from "@/components/tabs";
import BackLink from "@/components/BackLink.vue";
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

const activeTab = ref("");

watch(
  zaak,
  (z) => {
    if (!z || !contactmomentStore.huidigContactmoment) return;
    contactmomentStore.upsertZaak(
      z,
      contactmomentStore.huidigContactmoment.huidigeVraag,
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
