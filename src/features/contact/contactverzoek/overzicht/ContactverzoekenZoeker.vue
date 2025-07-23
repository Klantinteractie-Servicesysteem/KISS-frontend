<template>
  <form @submit.prevent="handleSearch" v-if="systemen">
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
      <contactverzoeken-overzicht
        :contactverzoeken="store.zoekerResults.data"
        :level="1"
      />
    </template>

    <application-message
      v-if="store.zoekerResults.error"
      messageType="error"
      message="Er is een fout opgetreden"
    />
  </section>
</template>

<script lang="ts" setup>
import SimpleSpinner from "@/components/SimpleSpinner.vue";
import SearchResultsCaption from "@/components/SearchResultsCaption.vue";
import { Button as UtrechtButton } from "@utrecht/component-library-vue";
import ContactverzoekenOverzicht from "./ContactverzoekenOverzicht.vue";
import { ensureState } from "@/stores/create-store";
import { search } from "./service";
import type { ContactverzoekOverzichtItem } from "./types";
import ApplicationMessage from "@/components/ApplicationMessage.vue";
import { useSystemen } from "@/services/environment/fetch-systemen";

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

const { systemen } = useSystemen();

const handleSearch = async () => {
  if (!systemen.value) {
    throw new Error("systemen niet gevonden");
  }

  store.value.zoekerResults.loading = true;
  store.value.zoekerResults.success = false;
  store.value.zoekerResults.error = false;

  try {
    store.value.zoekerResults.data = await search(
      systemen.value,
      store.value.searchQuery,
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
