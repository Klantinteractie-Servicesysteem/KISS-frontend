<template>
  <form @submit.prevent="handleSearch">
    <fieldset class="radio-group">
      <legend>Waar wil je op zoeken?</legend>
      <label v-for="(label, field) in labels" :key="field">
        <input type="radio" :value="field" v-model="state.field" required />
        {{ label }}
      </label>
    </fieldset>
    <fieldset class="search-bar">
      <label>
        <span>Zoek naar een bedrijf</span>
        <input
          type="search"
          placeholder="Zoek naar een bedrijf"
          ref="inputRef"
          v-model="state.currentSearch"
          @search="handleSearch"
        />
      </label>
      <button title="Zoeken">
        <span>Zoeken</span>
      </button>
    </fieldset>
  </form>

  <section class="search-section" v-if="state.klantSearchQuery?.query">
    <simple-spinner v-if="klanten.loading" />
    <template v-if="klanten.success">
      <bedrijven-overzicht :records="klanten.data.page">
        <template #caption>
          <SearchResultsCaption :results="klanten.data" />
        </template>
      </bedrijven-overzicht>
    </template>
    <application-message
      v-if="klanten.error"
      messageType="error"
      message="Er is een fout opgetreden"
    />
  </section>

  <section class="search-section" v-else-if="state.query?.value">
    <simple-spinner v-if="bedrijven.loading" />
    <template v-if="bedrijven.success">
      <bedrijven-overzicht :records="bedrijven.data.page">
        <template #caption>
          <SearchResultsCaption :results="bedrijven.data" />
        </template>
      </bedrijven-overzicht>
      <pagination
        class="pagination"
        :pagination="bedrijven.data"
        @navigate="navigate"
      />
    </template>
    <application-message
      v-if="bedrijven.error"
      messageType="error"
      :message="
        bedrijven.error instanceof FriendlyError
          ? bedrijven.error.message
          : 'Er is een fout opgetreden'
      "
    />
  </section>
</template>

<script setup lang="ts">
import { parseKvkNummer, parsePostcodeHuisnummer } from "@/helpers/validation";
import { ensureState } from "@/stores/create-store";
import { computed, ref, watch } from "vue";
import { bedrijfQuery, useSearchBedrijven, FriendlyError } from "./service";
import SimpleSpinner from "@/components/SimpleSpinner.vue";
import Pagination from "@/nl-design-system/components/Pagination.vue";
import ApplicationMessage from "@/components/ApplicationMessage.vue";
import BedrijvenOverzicht from "./BedrijvenOverzicht.vue";
import type { SearchCategories, BedrijfQuery } from "./types";
import SearchResultsCaption from "@/components/SearchResultsCaption.vue";
import {
  createKlantQuery,
  useSearchKlanten,
  type KlantSearch,
  type KlantSearchField,
} from "../service";
import { KlantType } from "../types";
import { useRouter } from "vue-router";
type SearchFields = KlantSearchField | SearchCategories;

const labels: { [key in SearchFields]: string } = {
  handelsnaam: "Bedrijfsnaam",
  kvkNummer: "KVK-nummer",
  postcodeHuisnummer: "Postcode + Huisnummer",
  email: "E-mailadres",
  telefoonnummer: "Telefoonnummer",
};

const state = ensureState({
  stateId: "BedrijfZoeker",
  stateFactory() {
    return {
      currentSearch: "",
      field: "handelsnaam" as SearchFields,
      query: undefined as BedrijfQuery | undefined,
      klantSearchQuery: undefined as KlantSearch<KlantSearchField> | undefined,
      page: 1,
    };
  },
});

const inputRef = ref();

const currentBedrijfQuery = computed(() => {
  const { currentSearch, field } = state.value;

  if (field === "telefoonnummer" || field === "email") return undefined;

  if (field === "postcodeHuisnummer") {
    const parsed = parsePostcodeHuisnummer(currentSearch);
    return parsed instanceof Error
      ? parsed
      : bedrijfQuery({
          field,
          value: parsed,
        });
  }

  if (field === "kvkNummer") {
    const parsed = parseKvkNummer(currentSearch);
    return parsed instanceof Error
      ? parsed
      : bedrijfQuery({
          field,
          value: parsed,
        });
  }

  return bedrijfQuery({
    field,
    value: currentSearch,
  });
});

const currentKlantQuery = computed(() => {
  const { currentSearch, field } = state.value;

  if (field === "telefoonnummer" || field === "email")
    return createKlantQuery({
      field,
      query: currentSearch,
    });

  return undefined;
});

watch(
  [currentBedrijfQuery, inputRef],
  ([query, input]) => {
    if (!(input instanceof HTMLInputElement)) return;
    input.setCustomValidity(query instanceof Error ? query.message : "");
  },
  { immediate: true }
);

const klanten = useSearchKlanten({
  query: computed(() => state.value.klantSearchQuery),
  page: computed(() => state.value.page),
  subjectType: KlantType.Bedrijf,
});

const handleSearch = () => {
  state.value.klantSearchQuery = currentKlantQuery.value;
  if (!(currentBedrijfQuery.value instanceof Error)) {
    state.value.query = currentBedrijfQuery.value;
  }
  state.value.page = 1;
};

const bedrijven = useSearchBedrijven(() => ({
  query: state.value.query,
  page: state.value.page,
}));

const navigate = (val: number) => {
  state.value.page = val;
};

const singleKlantId = computed(() => {
  if (klanten.success && klanten.data.page.length === 1) {
    const first = klanten.data.page[0];
    if (first?._typeOfKlant === "klant") {
      return first.id;
    }
  }
  return undefined;
});

const router = useRouter();

watch(singleKlantId, (newId, oldId) => {
  if (newId && newId !== oldId) {
    router.push(`/bedrijven/${newId}`);
  }
});
</script>

<style scoped lang="scss">
input[type="search"] {
  width: var(--section-width);
}

.search-bar {
  margin-bottom: var(--spacing-large);
  width: min(100%, 20rem);
}

.pagination {
  margin-inline: auto;
}

.search-section {
  display: grid;
  gap: var(--spacing-small);
}

input[type="radio"] {
  accent-color: var(--color-primary);
  margin: 0;
}

.radio-group {
  display: grid;
  grid-template-columns: repeat(3, auto);
  justify-content: flex-start;
  column-gap: var(--spacing-default);
  row-gap: var(--spacing-extrasmall);

  > legend {
    font-size: 0;
  }

  label {
    display: flex;
    gap: var(--spacing-small);
    align-items: center;
  }
}
</style>
