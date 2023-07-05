<template>
  <klant-aanmaken
    v-if="showKlantAanmaken"
    :handle-cancel="handleCancelKlantAanmaken"
  />

  <template v-else>
    <section class="actions">
      <form @submit.prevent="handleSearch">
        <fieldset class="radio-group">
          <legend>Waar wil je op zoeken?</legend>
          <label v-for="(label, field) in labels" :key="field">
            <input type="radio" :value="field" v-model="store.field" required />
            {{ label }}
          </label>
        </fieldset>
        <fieldset class="search-bar">
          <label>
            <span>Zoek naar een persoon</span>
            <input
              type="search"
              placeholder="Zoek naar een persoon"
              ref="inputRef"
              v-model="store.currentSearch"
              @search="handleSearch"
            />
          </label>
          <button title="Zoeken">
            <span>Zoeken</span>
          </button>
        </fieldset>
      </form>
      <!-- temporarily disabled -->
      <button
        v-if="false"
        @click="toggleKlantAanmaken"
        type="button"
        class="klant-aanmaken icon-before plus utrecht-button utrecht-button--secondary-action"
      >
        <span>Klant aanmaken</span>
      </button>
    </section>

    <section class="search-section" v-if="store.klantSearchQuery?.query">
      <simple-spinner v-if="klanten.loading" />
      <template v-if="klanten.success">
        <personen-overzicht :records="klanten.data.page">
          <template #caption>
            <SearchResultsCaption :results="klanten.data" />
          </template>
        </personen-overzicht>
      </template>
      <application-message
        v-if="klanten.error"
        messageType="error"
        message="Er is een fout opgetreden"
      />
    </section>

    <section class="search-section" v-else-if="store.persoonSearchQuery?.value">
      <simple-spinner v-if="personen.loading" />
      <template v-if="personen.success">
        <personen-overzicht :records="personen.data.page">
          <template #caption>
            <SearchResultsCaption :results="personen.data" />
          </template>
        </personen-overzicht>
        <pagination
          class="pagination"
          :pagination="personen.data"
          @navigate="navigate"
        />
      </template>
      <application-message
        v-if="personen.error"
        messageType="error"
        message="Er is een fout opgetreden"
      />
    </section>
  </template>
</template>

<script lang="ts" setup>
import { watch, ref, computed } from "vue";
import {
  createKlantQuery,
  type KlantSearch,
  useSearchKlanten,
  type KlantSearchField,
} from "../service";
import PersonenOverzicht from "./PersonenOverzicht.vue";
import ApplicationMessage from "@/components/ApplicationMessage.vue";
import SimpleSpinner from "@/components/SimpleSpinner.vue"; //todo: spinner via slot?
import Pagination from "@/nl-design-system/components/Pagination.vue"; //todo: ook via slot?
import KlantAanmaken from "./PersoonAanmaken.vue";
import { ensureState } from "@/stores/create-store"; //todo: niet in de stores map. die is applicatie specifiek. dit is generieke functionaliteit
import { useRouter } from "vue-router";
import SearchResultsCaption from "@/components/SearchResultsCaption.vue";
import {
  parseBsn,
  parseGeslachtsnaamGeboortedatum,
  parsePostcodeHuisnummer,
} from "@/helpers/validation";
import {
  persoonQuery,
  useSearchPersonen,
  type PersoonQuery,
  type PersoonSearchField,
} from "./service";

type SearchFields = KlantSearchField | PersoonSearchField;

const labels: {
  readonly [K in SearchFields]: string;
} = {
  geslachtsnaamGeboortedatum: "Achternaam + geboortedatum",
  postcodeHuisnummer: "Postcode + huisnummer",
  email: "E-mailadres",
  telefoonnummer: "Telefoonnummer",
  bsn: "BSN",
};

const store = ensureState({
  stateId: "klant-zoeker",
  stateFactory() {
    return {
      currentSearch: "",
      field: "geboortedatum" as SearchFields,
      klantSearchQuery: undefined as KlantSearch<KlantSearchField> | undefined,
      persoonSearchQuery: undefined as
        | PersoonQuery<PersoonSearchField>
        | undefined,
      page: 1,
    };
  },
});

const inputRef = ref();

const currentKlantQuery = computed(() => {
  const { currentSearch, field } = store.value;

  if (field === "telefoonnummer" || field === "email")
    return createKlantQuery({
      field,
      query: currentSearch,
    });

  return undefined;
});

const currentPersoonQuery = computed(() => {
  const { currentSearch, field } = store.value;

  if (field === "geslachtsnaamGeboortedatum") {
    const parsed = parseGeslachtsnaamGeboortedatum(currentSearch);
    return parsed instanceof Error
      ? parsed
      : persoonQuery({
          field,
          value: parsed,
        });
  }

  if (field === "postcodeHuisnummer") {
    const parsed = parsePostcodeHuisnummer(currentSearch);
    return parsed instanceof Error
      ? parsed
      : persoonQuery({
          field,
          value: parsed,
        });
  }

  if (field === "bsn") {
    const parsed = parseBsn(currentSearch);
    return parsed instanceof Error
      ? parsed
      : persoonQuery({
          field,
          value: parsed,
        });
  }

  return undefined;
});

watch(
  [currentPersoonQuery, inputRef],
  ([query, input]) => {
    if (!(input instanceof HTMLInputElement)) return;
    input.setCustomValidity(query instanceof Error ? query.message : "");
  },
  { immediate: true }
);

const klanten = useSearchKlanten({
  query: computed(() => store.value.klantSearchQuery),
  page: computed(() => store.value.page),
});

const personen = useSearchPersonen({
  query: computed(() => store.value.persoonSearchQuery),
});

const navigate = (val: number) => {
  store.value.page = val;
};

const showKlantAanmaken = ref(false);
const toggleKlantAanmaken = (): void => {
  showKlantAanmaken.value = !showKlantAanmaken.value;
};
const handleCancelKlantAanmaken = () => {
  showKlantAanmaken.value = false;
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
    router.push(`/personen/${newId}`);
  }
});

const handleSearch = () => {
  store.value.klantSearchQuery = currentKlantQuery.value;
  if (!(currentPersoonQuery.value instanceof Error)) {
    store.value.persoonSearchQuery = currentPersoonQuery.value;
  }
  store.value.page = 1;
};
</script>

<style lang="scss" scoped>
.klant-aanmaken {
  display: flex;

  span {
    margin-inline-start: var(--spacing-extrasmall);
  }
}

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

.actions {
  display: flex;
  justify-content: space-between;
  align-items: flex-start;
  flex-wrap: wrap;
}
</style>
