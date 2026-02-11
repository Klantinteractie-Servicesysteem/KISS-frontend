<template>
  <div>
    <search-combobox
      v-bind="{ ...$attrs, ...props }"
      options-label="Medewerkers"
      :placeholder="placeholder"
      :model-value="searchText"
      @update:model-value="updateModelValue"
      :list-items="result"
      :exact-match="true"
      :required="required"
      :disabled="isDisabled"
      ref="searchCombo"
      :loading="isLoading"
      :show-description="false"
    />
  </div>
</template>

<script lang="ts">
export default {
  inheritAttrs: false,
};
</script>

<script lang="ts" setup>
import { debouncedRef } from "@vueuse/core";
import { ref, watch } from "vue";

import SearchCombobox from "@/components/SearchCombobox.vue";

import type { PropType } from "vue";
import { fetchLoggedIn, parseJson, throwIfNotOk } from "@/services";
import { globalSearchBaseUri } from "@/features/types";

type DatalistItem = {
  value: string;
  description: string;
};

const props = defineProps({
  modelValue: {
    // eslint-disable-next-line @typescript-eslint/no-explicit-any
    type: Object as PropType<Record<string, any> | undefined>,
  },
  id: {
    type: String,
    default: undefined,
  },
  filterField: {
    type: String,
    default: undefined,
  },
  filterValue: {
    type: String,
    default: undefined,
  },
  required: {
    type: Boolean,
    default: false,
  },
  isDisabled: {
    type: Boolean,
    default: false,
  },
  placeholder: {
    type: String,
    default: "Zoek een medewerker",
  },
});

const emit = defineEmits(["update:modelValue"]);

const searchText = ref("");
const debouncedSearchText = debouncedRef(searchText, 300);

function searchMedewerkers(parameters: any): Promise<DatalistItem[]> {
  function mapToDataListItem(obj: any): any {
    const functie = obj?._source.Smoelenboek.functie || obj.function;
    const department =
      obj?._source.Smoelenboek.afdelingen?.[0]?.afdelingnaam ||
      obj?._source.Smoelenboek.department;

    const werk = [functie, department].filter(Boolean).join(" bij ");
    return {
      value: obj?._source.title,
      description: werk,

      identificatie: obj?._source?.Smoelenboek?.identificatie,
      afdelingen: obj?._source?.Smoelenboek?.afdelingen,
      groepen: obj?._source?.Smoelenboek?.groepen,
    };
  }

  const getPayload = () => {
    const { search, filterField, filterValue } = parameters;

    const searchQuery = search
      ? {
          simple_query_string: {
            query: search,
            default_operator: "and",
          },
        }
      : null;

    const filterMatchQuery =
      filterField && filterValue
        ? {
            match: {
              [`${filterField}.enum`]: filterValue,
            },
          }
        : null;

    const query = {
      from: 0,
      size: 30,
      sort: [{ "Smoelenboek.achternaam.enum": { order: "asc" } }],
      query: {
        bool: {
          must: [searchQuery, filterMatchQuery].filter(Boolean),
        },
      },
    };

    return JSON.stringify(query);
  };

  return fetchLoggedIn(`${globalSearchBaseUri}/search-smoelenboek/_search`, {
    method: "POST",
    headers: {
      "content-type": "application/json",
    },
    body: getPayload(),
  })
    .then(throwIfNotOk)
    .then(parseJson)
    .then((r: any) => {
      const {
        hits: { hits },
      } = r ?? {};

      return Array.isArray(hits) ? hits.map(mapToDataListItem) : [];
    });
}

function updateModelValue(v: string) {
  searchText.value = v;
  if (!isLoading.value) {
    const match = result.value.find((x: { value: string }) => x?.value === v);

    emit(
      "update:modelValue",
      match && {
        ...match,
        title: match.value,
        achternaam: match.value,
      },
    );
  }
}

watch(
  () => props.modelValue,
  (v) => {
    searchText.value = v?.title;
  },
  { immediate: true },
);

const result = ref<DatalistItem[]>([]);
const isLoading = ref<boolean>(false);

watch(
  [
    () => props.filterField,
    () => props.filterValue,
    () => debouncedSearchText.value,
  ],
  async () => {
    isLoading.value = true;
    try {
      result.value = await searchMedewerkers({
        search: debouncedSearchText.value,
        filterField: props.filterField,
        filterValue: props.filterValue,
      });
    } finally {
      isLoading.value = false;
    }
  },
);
</script>

<style lang="scss" scoped>
div {
  position: relative;
}
</style>
