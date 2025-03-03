<template>
  <form @submit.prevent="handleSearch">
    <label class="utrecht-form-label">
      Telefoonnummer of e-mailadres
      <input
        type="text"
        v-model="store.searchQuery"
        class="utrecht-textbox utrecht-textbox--html-input"
      />
    </label>
    <utrecht-button type="submit" appearance="primary-action-button">
      Zoeken
    </utrecht-button>
  </form>

  <section class="search-section">
    <simple-spinner v-if="store.zoekerResults.loading" />
    <template v-if="store.zoekerResults.success">
      <table class="overview zoekresultaten-view">
        <SearchResultsCaption
          :results="store.zoekerResults.data"
          :zoek-termen="undefined"
        />
        <contactverzoeken-overzicht
          :contactverzoeken="store.zoekerResults.data"
        >
          <template v-for="(_, slotName) in $slots" #[slotName]="props">
            <slot :name="slotName" v-bind="props"></slot>
          </template>
        </contactverzoeken-overzicht>
      </table>
    </template>

    <application-message
      v-if="store.zoekerResults.error"
      messageType="error"
      message="Er is een fout opgetreden"
    />
  </section>
</template>

<script lang="ts" setup>
import { ref, onMounted } from "vue";
import SimpleSpinner from "@/components/SimpleSpinner.vue";
import SearchResultsCaption from "@/components/SearchResultsCaption.vue";
import { Button as UtrechtButton } from "@utrecht/component-library-vue";
import ContactverzoekenOverzicht from "./ContactverzoekenOverzicht.vue";
import { ensureState } from "@/stores/create-store";
import { search } from "./service";
import type { ContactverzoekOverzichtItem } from "./types";
import ApplicationMessage from "@/components/ApplicationMessage.vue";
import { getRegisterDetails } from "@/features/shared/systeemdetails";

defineSlots();

const gebruikKlantInteracatiesApi = ref<boolean>(false);
const defaultSysteemId = ref<string | null>(null);

const store = ensureState({
  stateId: "contactverzoeken-zoeker",
  stateFactory() {
    return {
      searchQuery: "",
      zoekerResults: {
        loading: false,
        success: false,
        error: false,
        data: [] as ContactverzoekOverzichtItem[],
      },
    };
  },
});

onMounted(async () => {
  const { useKlantInteractiesApi, defaultSystemId } =
    await getRegisterDetails();
  defaultSysteemId.value = defaultSystemId;
  gebruikKlantInteracatiesApi.value = useKlantInteractiesApi;
});

const handleSearch = async () => {
  if (!defaultSysteemId.value) {
    throw new Error("defaultSysteemId ontbreekt of is leeg");
  }

  store.value.zoekerResults.loading = true;
  store.value.zoekerResults.success = false;
  store.value.zoekerResults.error = false;

  try {
    store.value.zoekerResults.data = await search(
      defaultSysteemId.value,
      store.value.searchQuery,
      gebruikKlantInteracatiesApi.value,
    );
    store.value.zoekerResults.success = true;
  } catch (error) {
    store.value.zoekerResults.error = true;
  } finally {
    store.value.zoekerResults.loading = false;
  }
};
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
  min-inline-size: max-content;
}

.zoekresultaten-view {
  min-inline-size: fit-content;
}
</style>
