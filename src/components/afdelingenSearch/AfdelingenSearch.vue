<template>
  <service-data-search
    :get-data="useAfdelingen"
    :map-value="(x: Afdeling) => x.naam"
    :map-description="(x: Afdeling) => x.identificatie"
    :model-value="modelValue"
    @update:modelValue="$emit('update:modelValue', $event)"
    v-bind="$attrs"
    options-label="Afdelingen"
  />
</template>

<script setup lang="ts">
import ServiceDataSearch from "@/components/ServiceDataSearch.vue";
import { useAfdelingen, type Afdeling } from "./afdelingen";
defineProps<{ modelValue: Afdeling | undefined }>();
defineEmits<{ "update:modelValue": [Afdeling | undefined] }>();
</script>
