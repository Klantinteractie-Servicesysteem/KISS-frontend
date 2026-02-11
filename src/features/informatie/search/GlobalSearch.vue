<template>
  <button
    type="button"
    :class="[
      'icon-after',
      'chevron-down',
      'expand-button',
      'icon-large',
      { isExpanded: state.isExpanded },
    ]"
    @click="state.isExpanded = !state.isExpanded"
    v-if="searchResults.success && searchResults.data.page.length"
  >
    {{ buttonText }}
  </button>

  <form
    method="get"
    enctype="application/x-www-form-urlencoded"
    @submit.prevent="applySearch"
    ref="searchBarRef"
  >
    <fieldset class="bronnen">
      <template v-if="sources.success">
        <label v-for="bron in sources.data" :key="bron.name + bron.index">
          <input
            type="checkbox"
            v-model="state.selectedSources"
            :value="bron"
          />
          {{ bron.name.replace(/(^\w+:|^)\/\//, "").replace("www.", "") }}
        </label>
      </template>
    </fieldset>
    <div class="search-bar">
      <label for="global-search-input"> Zoekterm</label>
      <search-combobox
        :list-items="listItems.success ? listItems.data : []"
        v-model="state.searchInput"
        :placeholder="'Zoeken...'"
        @search.prevent="applySearch"
        id="global-search-input"
        :required="false"
        :disabled="false"
        :loading="listItems.loading"
        :exact-match="false"
        options-label="Suggesties"
      />
      <button><span>Zoeken</span></button>
    </div>
  </form>
  <template v-if="state.currentSearch">
    <section
      v-if="state.isExpanded"
      :class="['search-results', { isExpanded: state.isExpanded }]"
    >
      <template v-if="searchResults.success">
        <p v-if="!hasResults" class="no-results">Geen resultaten gevonden</p>
        <template v-else>
          <nav v-show="!state.currentId">
            <ul>
              <li
                v-for="{
                  id,
                  title,
                  source,
                  jsonObject,
                  url,
                  documentUrl,
                } in searchResults.data.page"
                :key="'nav_' + id"
              >
                <a
                  v-if="!url"
                  :id="'nav_' + id"
                  :href="`#searchResult_${id}`"
                  @click.prevent="
                    selectSearchResult(
                      id,
                      source,
                      jsonObject,
                      title,
                      documentUrl,
                    )
                  "
                >
                  <span :class="`category-${source}`">{{ source }}</span>
                  <span v-if="source === 'Smoelenboek'">
                    {{
                      [
                        title,
                        jsonObject?.functie && jsonObject?.afdelingen?.length
                          ? jsonObject?.functie +
                            " (" +
                            jsonObject?.afdelingen
                              .map((a: any) => a.afdelingnaam)
                              .join(", ") +
                            ")"
                          : jsonObject?.functie,
                      ]
                        .filter(Boolean)
                        .join(", ")
                    }}
                  </span>
                  <span v-else>{{ title }}</span>
                </a>
                <a
                  v-else
                  @click.prevent="
                    handleWebsiteSelected({ url: url.toString(), title: title })
                  "
                  :href="url.toString()"
                  class="icon-after chevron-down"
                  rel="noopener noreferrer"
                  ><span :class="`category-${source}`">{{ source }}</span
                  ><span>{{ title }}</span></a
                >
              </li>
            </ul>
          </nav>
          <pagination
            class="pagination"
            :pagination="searchResults.data"
            @navigate="handlePaginationNavigation"
            v-show="!state.currentId"
          />
          <ul v-show="!!state.currentId">
            <li
              v-for="{
                id,
                title,
                source,
                content,
                url,
                jsonObject,
              } in searchResults.data.page"
              :key="'searchResult_' + id"
              v-show="id === state.currentId"
            >
              <a
                class="back-to-results"
                :href="'#nav_' + id"
                :id="'searchResult_' + id"
                @click.prevent="backToResults"
                >Alle zoekresultaten</a
              >
              <template v-if="id === state.currentId">
                <medewerker-detail
                  :medewerkerRaw="jsonObject"
                  v-if="source === 'Smoelenboek'"
                  :title="title"
                  :heading-level="2"
                />
                <kennisartikel-detail
                  v-else-if="
                    source === 'Kennisartikel' || source === 'Kennisbank'
                  "
                  :kennisartikel-raw="jsonObject"
                  :title="title"
                  :heading-level="2"
                  @kennisartikel-selected="handleKennisartikelSelected"
                />
                <vac-detail
                  v-else-if="source === 'VAC'"
                  :raw="jsonObject"
                  :title="title"
                  :heading-level="2"
                />

                <article v-else>
                  <header>
                    <utrecht-heading :level="2"
                      ><a
                        v-if="url"
                        :href="url.toString()"
                        target="_blank"
                        rel="noopener noreferrer"
                      >
                        {{ title }}
                      </a>
                      <template v-else>{{ title }}</template>
                    </utrecht-heading>
                    <small :class="`category-${source}`">{{ source }}</small>
                  </header>
                  <p v-if="content">{{ content }}</p>
                </article>
                <slot name="articleFooter" :id="url" :title="title"></slot>
              </template>
            </li>
          </ul>
        </template>
      </template>
      <simple-spinner
        class="spinner"
        v-if="searchResults.state === 'loading'"
      />
    </section>
    <div v-else class="search-results">Zoekresultaten</div>
  </template>
</template>

<script lang="ts">
export default {
  inheritAttrs: false,
};
</script>

<script lang="ts" setup>
import { Heading as UtrechtHeading } from "@utrecht/component-library-vue";
import { computed, nextTick, ref, watch } from "vue";
import { useGlobalSearch, useSources } from "./service";

import Pagination from "@/nl-design-system/components/Pagination.vue";
import SimpleSpinner from "@/components/SimpleSpinner.vue";
import type { Source } from "./types";
import MedewerkerDetail from "./MedewerkerDetail.vue";
import KennisartikelDetail from "./KennisartikelDetail.vue";
import VacDetail from "./VacDetail.vue";
import type {
  Medewerker,
  Website,
  Vac,
} from "@/features/informatie/search/types";
import { useContactmomentStore } from "@/stores/contactmoment/index";
import { ensureState } from "@/stores/create-store";
import SearchCombobox from "../../../components/SearchCombobox.vue";
import { mapServiceData } from "@/services";
import type { Kennisartikel } from "@/features/types";

const emit = defineEmits<{
  (
    e: "result-selected",
    params: {
      id: string;
      title: string;
      jsonObject: any;
      source: string;
      documentUrl: URL;
    },
  ): void;
}>();

const contactmomentStore = useContactmomentStore();

const state = ensureState({
  stateId: "GlobalSearch",
  stateFactory() {
    return {
      searchInput: "",
      currentSearch: "",
      currentId: "",
      currentPage: 1,
      isExpanded: true,
      selectedSources: [] as Source[],
    };
  },
});

const searchBarRef = ref();

const sources = useSources();

const sourceParameter = computed(() =>
  sources.success && !state.value.selectedSources.length
    ? sources.data
    : state.value.selectedSources,
);

const searchParameters = computed(() => ({
  search: state.value.currentSearch,
  page: state.value.currentPage,
  filters: sourceParameter.value,
}));

const searchResults = useGlobalSearch(searchParameters);

let automaticSearchTimeout: number | NodeJS.Timeout;

function applySearch() {
  state.value.currentSearch = state.value.searchInput;
  state.value.currentId = "";
  state.value.currentPage = 1;
}

watch(
  () => state.value.searchInput,
  () => {
    automaticSearchTimeout && clearTimeout(automaticSearchTimeout);

    automaticSearchTimeout = setTimeout(applySearch, 300);
  },
);

function handlePaginationNavigation(page: number) {
  state.value.currentPage = page;
  const el = searchBarRef.value;
  if (el instanceof Element) {
    el.scrollIntoView();
  }
}

const buttonText = computed(() =>
  state.value.isExpanded ? "Inklappen" : "Uitklappen",
);

const hasResults = computed(
  () => searchResults.success && !!searchResults.data.page.length,
);

watch(hasResults, (x) => {
  if (!x) {
    state.value.isExpanded = true;
  }
});

const selectSearchResult = (
  id: string,
  source: string,
  jsonObject: any,
  title: string,
  documentUrl: URL,
) => {
  state.value.currentId = id;

  if (contactmomentStore.contactmomentLoopt) {
    if (source === "Smoelenboek")
      handleSmoelenboekSelected(
        {
          ...jsonObject,
          title,
        },
        documentUrl.toString(),
      );

    if ((source || "").toUpperCase() === "VAC")
      handleVacSelected(jsonObject, documentUrl.toString());
  }

  emit("result-selected", {
    id,
    title,
    source,
    jsonObject,
    documentUrl,
  });

  nextTick(() => {
    document.getElementById("searchResult_" + id)?.focus();
  });
};

const backToResults = () => {
  const id = state.value.currentId;
  state.value.currentId = "";
  nextTick(() => {
    document.getElementById("nav_" + id)?.focus();
  });
};

const handleSmoelenboekSelected = (
  medewerker: Medewerker,
  url: string,
): void => {
  contactmomentStore.addMedewerker(medewerker, url);
};

const handleVacSelected = (vac: Vac, url: string): void => {
  contactmomentStore.addVac(vac, url);
};

const handleKennisartikelSelected = (kennisartikel: Kennisartikel): void => {
  contactmomentStore.addKennisartikel(kennisartikel);
};

const handleWebsiteSelected = (website: Website): void => {
  contactmomentStore.addWebsite(website);
  window.open(website.url);
};

const listItems = mapServiceData(searchResults, (result) =>
  result.suggestions.map((value) => ({ value })),
);
</script>

<style lang="scss" scoped>
form {
  grid-area: bar;
  padding-inline-start: var(--spacing-large);
  padding-block: var(--spacing-small) var(--spacing-default);
  display: grid;
  gap: var(--spacing-small);
  background-color: var(--color-primary);
}

.search-bar {
  max-width: min(40rem, 100%);
  position: relative;

  :deep([role="combobox"]) {
    outline: none;

    &[aria-expanded="true"] {
      border-end-start-radius: 0;
      border-block-end: none;
      border-block-end-color: white;

      &::after {
        content: "";
        inline-size: 100%;
        block-size: 1px;
        background-color: var(--color-secondary);
      }

      ~ button {
        border-end-end-radius: 0;
        border-block-end: none;
      }
    }
  }

  :deep([role="listbox"]) {
    border-start-end-radius: 0;
    border-start-start-radius: 0;
    border-block-start: none;
    gap: var(--spacing-small);
    inset-block-end: 0;
  }

  :deep([role="option"]) {
    padding-block: var(--spacing-small);
  }
}

button {
  font-size: 0;
}

input[type="checkbox"] {
  accent-color: var(--color-secondary);
}

fieldset {
  display: flex;
  flex-wrap: wrap;
  gap: var(--spacing-default);
  color: var(--color-white);
  min-height: 24px;
}

.search-results {
  overflow: hidden;
  grid-area: results;
  display: grid;
  justify-items: stretch;
  padding-inline-start: var(--spacing-large);
  background-color: var(--color-secondary);
  gap: var(--spacing-default);
  position: relative;

  > ul li {
    padding-block: var(--spacing-large);
    display: grid;
    gap: var(--spacing-default);
    padding-inline-end: var(--container-padding);
  }

  &:not(.isExpanded) {
    max-height: 2.5rem;
    pointer-events: none;
    user-select: none;
    overflow: hidden;

    > * {
      opacity: 0.5;
    }
  }
}

.no-results {
  justify-self: center;
  padding-block: var(--spacing-default);
}

.spinner {
  font-size: 2rem;
}

nav {
  grid-column: 1 / 1;
}

.expand-button {
  position: sticky;
  grid-area: scroll;
  top: var(--spacing-large);
  align-self: start;
  margin-block-start: var(--spacing-default);
  justify-content: center;

  &:not(.isExpanded) {
    background: none;
  }

  &.isExpanded::after {
    transform: rotate(180deg);
  }
}

.expand-button::after {
  position: absolute;
}

nav ul {
  display: grid;

  a {
    color: inherit;
    text-decoration: none;
    display: grid;
    grid-template-columns: 20ch 1fr 2ch;
    gap: var(--spacing-default);
    padding-inline-end: var(--spacing-default);
    place-items: center start;
  }

  li {
    padding-block: var(--spacing-default);
    border-block-end: 1px solid var(--color-tertiary);
    display: flex;
  }
}

.back-to-results::before {
  content: "< ";
}

.pagination {
  margin-inline: auto;
  margin-block-end: var(--spacing-default);
}
</style>
