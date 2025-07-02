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
import { searchMedewerkers } from "@/features/search/service";
import SearchCombobox from "@/components/SearchCombobox.vue";

import type { PropType } from "vue";

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
