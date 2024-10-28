<template>
  <form @submit.prevent="handleSearch">
    <label class="utrecht-form-label">
      Telefoonnummer of e-mailadres
      <input
        type="text"
        v-model="query"
        class="utrecht-textbox utrecht-textbox--html-input"
      />
    </label>
    <utrecht-button type="submit" appearance="primary-action-button">
      Zoeken
    </utrecht-button>
  </form>

  <section class="search-section">
    <simple-spinner v-if="zoekerResults.loading" />
    <template v-if="zoekerResults.success">
      <table class="overview zoekresultaten-view">
        <SearchResultsCaption
          :results="filteredZoekerData"
          :zoek-termen="undefined"
        />
        <contactverzoeken-overzicht :contactverzoeken="filteredZoekerData">
          <template #onderwerp="{ contactmomentUrl }">
            <contactmoment-details-context :url="contactmomentUrl">
              <template #details="{ details }">
                {{ details?.vraag || details?.specifiekeVraag }}
              </template>
            </contactmoment-details-context>
          </template>

          <template v-if="!gebruikKlantInteracatiesApi" #contactmoment="{ url }">
            <contactmoment-preview :url="url">
              <template #object="{ object }">
                <zaak-preview :zaakurl="object.object" />
              </template>
            </contactmoment-preview>
          </template>
        </contactverzoeken-overzicht>
      </table>
    </template>

    <application-message
      v-if="zoekerResults.error"
      messageType="error"
      message="Er is een fout opgetreden"
    />
  </section>
</template>
<script lang="ts" setup>
import { computed, ref, onMounted } from "vue";
import SimpleSpinner from "@/components/SimpleSpinner.vue";
import SearchResultsCaption from "@/components/SearchResultsCaption.vue";
import { Button as UtrechtButton } from "@utrecht/component-library-vue";
import ContactverzoekenOverzicht from "./ContactverzoekenOverzicht.vue";
import { ensureState } from "@/stores/create-store";
import ContactmomentDetailsContext from "@/features/contact/contactmoment/ContactmomentDetailsContext.vue";
import ContactmomentPreview from "@/features/contact/contactmoment/ContactmomentPreview.vue";
import { isOk2DefaultContactenApi } from "@/features/contact/contactmoment/service";
import { search } from "./service";
import type { Contactverzoek } from "./types";
import type { PaginatedResult } from "@/services";

const gebruikKlantInteracatiesApi = ref<boolean>(false);

const store = ensureState({
  stateId: "klant-zoeker",
  stateFactory() {
    return {
      searchQuery: "",
    };
  },
});

const query = ref(store.value.searchQuery);

const zoekerResults = ref({
  loading: false,
  success: false,
  error: false,
  data: [] as PaginatedResult<Contactverzoek>[], 
});

onMounted(async () => {
  gebruikKlantInteracatiesApi.value = await isOk2DefaultContactenApi();
});

const handleSearch = async () => {
  store.value.searchQuery = query.value; // Sla `query` op in de `store`
  zoekerResults.value.loading = true;
  zoekerResults.value.success = false;
  zoekerResults.value.error = false;

  try {
    zoekerResults.value.data = await search(query, gebruikKlantInteracatiesApi);
    zoekerResults.value.success = true;
  } catch (error) {
    zoekerResults.value.error = true;
  } finally {
    zoekerResults.value.loading = false;
  }
};

const filteredZoekerData = computed(() => {
  if (zoekerResults.value.success && query.value) {
    return zoekerResults.value.data.flatMap((paginatedResult) =>
      paginatedResult.page.filter((item) => {
        return !item.record.data.betrokkene.hasOwnProperty.call(
          item.record.data.betrokkene,
          "klant"
        );
      })
    );
  }
  return [];
});

</script>

<style lang="scss" scoped>
form {
  display: grid;
  inline-size: 30rem;
  max-inline-size: 100%;
  gap: var(--spacing-default);

  :deep(button) {
    justify-self: flex-end;
  }
}

.actions {
  display: flex;
  justify-content: space-between;
  align-items: flex-start;
  flex-wrap: wrap;
}

.overview {
  inline-size: 30rem;
  min-inline-size: max-content;
}

.zoekresultaten-view {
  min-inline-size: fit-content;
}
</style>
