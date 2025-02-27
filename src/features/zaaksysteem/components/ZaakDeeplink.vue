<template>
  <a v-if="deeplink" :href="deeplink" target="_blank" rel="noopener noreferrer"
    >Open in zaaksysteem</a
  >
</template>

<script setup lang="ts">
import { computed } from "vue";
import type { ZaakDetails } from "../types";
import type { Systeem } from "@/services/environment/fetch-systemen";
const props = defineProps<{
  zaak: ZaakDetails;
  systeem: Systeem;
}>();

const deeplink = computed(() => {
  const { deeplinkProperty, deeplinkUrl } = props.systeem || {};
  if (!deeplinkProperty || !deeplinkUrl) return null;
  const property = (props.zaak as Record<string, unknown>)[deeplinkProperty];
  if (!property) return null;
  return deeplinkUrl + property;
});
</script>
