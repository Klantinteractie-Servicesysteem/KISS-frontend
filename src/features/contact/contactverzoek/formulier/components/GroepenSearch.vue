<template>
  <service-data-search
    options-label="Groepen"
    :get-data="useGroepen"
    :map-value="(x: Groep) => x.naam"
    :map-description="(x: Groep) => x.identificatie"
    :model-value="modelValue"
    @update:modelValue="$emit('update:modelValue', $event)"
    v-bind="$attrs"
  />
</template>

<script setup lang="ts">
import ServiceDataSearch from "@/components/ServiceDataSearch.vue";
import { getGroepenSearchUrl, groepenFetcher, type Groep } from "./groepen";
import { ServiceResult } from "@/services";
defineProps<{ modelValue: Groep | undefined }>();
defineEmits<{ "update:modelValue": [Groep | undefined] }>();

//refactoring suggestie: servicedatasearch voegt hier niets toe
//liever rechtstreeks search-combobox gebruiken, daar is een variant zonder servicedata nodig en die is er nu.
//dat zou de totale hoeveelheid code en de leesbaarder van onderstaande verbeteren
//(dan is alleen een watch op de gedebouncde invoer nodig zijn, die een nieuwe groepen fetch doet)
const useGroepen = (search: () => string | undefined) => {
  const getUrl = () => getGroepenSearchUrl(search(), false);
  return ServiceResult.fromFetcher(getUrl, groepenFetcher);
};
</script>
