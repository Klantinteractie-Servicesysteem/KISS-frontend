<template>
  <section>
    <form class="search-bar" @submit.prevent="zoekOpZaak">
      <label>
        <input
          type="search"
          placeholder="Zoek op zaaknummer"
          v-model="store.searchField"
          title="ZAAK-1"
        />
      </label>
      <button title="Zoeken">
        <span>Zoeken</span>
      </button>
    </form>

    <application-message
      v-if="pabcActive"
      messageType="warning"
      message="Let op: u ziet mogelijk niet alle zaken vanwege autorisatie-instellingen."
    ></application-message>

    <template v-if="store.currentSearch">
      <application-message
        v-if="error"
        messageType="error"
        message="Er is een probleem opgetreden."
      ></application-message>
      <simple-spinner v-else-if="loading"></simple-spinner>
      <section class="resultaten-container" v-if="zaken">
        <zaken-overzicht
          :zaken="zaken"
          :vraag="contactmomentStore.huidigContactmoment?.huidigeVraag"
        >
          <template #caption>
            <SearchResultsCaption :results="zaken" :zoekTermen="undefined" />
          </template>
        </zaken-overzicht>
      </section>
    </template>
  </section>
</template>

<script lang="ts" setup>
import { watch, computed, ref } from "vue";
import { fetchZakenByZaaknummer } from "./service";
import SimpleSpinner from "@/components/SimpleSpinner.vue";
import ApplicationMessage from "@/components/ApplicationMessage.vue";
import ZakenOverzicht from "./ZakenOverzicht.vue";
import { ensureState } from "@/stores/create-store"; //todo: niet in de stores map. die is applicatie specifiek. dit is generieke functionaliteit
import { useContactmomentStore } from "@/stores/contactmoment";
import { useRouter } from "vue-router";
import SearchResultsCaption from "../../components/SearchResultsCaption.vue";
import { useLoader } from "@/services";
import { useSystemen } from "@/services/environment/fetch-systemen";
import { usePabcStatus } from "@/services/pabc";

const submitted = ref(false);

const contactmomentStore = useContactmomentStore();
const { data: pabcActive } = usePabcStatus();

const store = ensureState({
  stateId: "zaak-zoeker",
  stateFactory() {
    return {
      searchField: "",
      currentSearch: "",
    };
  },
});

const { systemen } = useSystemen();

const {
  data: zaken,
  error,
  loading,
} = useLoader(() => {
  if (systemen.value && store.value.currentSearch)
    return fetchZakenByZaaknummer(systemen.value, store.value.currentSearch);
});

const zoekOpZaak = () => {
  submitted.value = true;
  store.value.currentSearch = store.value.searchField;
};

const singleZaakUrl = computed(() => {
  if (zaken.value?.length === 1) {
    const zaak = zaken.value[0];
    const zaaksysteemId = zaak.zaaksysteemId
      ? encodeURIComponent(zaak.zaaksysteemId)
      : "";
    return `/zaken/${zaak.url.split("/").pop()}?zaaksysteemId=${zaaksysteemId}`;
  }
  return "";
});

const router = useRouter();

watch(singleZaakUrl, (newId, oldId) => {
  if (newId && newId !== oldId && submitted.value) {
    router.push(newId);
  }
});
</script>

<style lang="scss" scoped>
.search-bar {
  inline-size: min(100%, 20rem);
  margin-block-end: var(--spacing-large);
}
</style>
