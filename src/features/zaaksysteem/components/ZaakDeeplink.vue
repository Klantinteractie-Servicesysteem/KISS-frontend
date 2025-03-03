<template>
  <a v-if="deeplink" :href="deeplink" target="_blank" rel="noopener noreferrer"
    >Open in zaaksysteem</a
  >
</template>

<script setup lang="ts">
import { computed } from "vue";
import type { ZaakDetails } from "../types";
import { useSystemen } from "@/services/environment/fetch-systemen";
const props = defineProps<{
  zaak: ZaakDetails;
  systeemId: string;
}>();

const { systemen } = useSystemen();

const systeem = computed(() =>
  systemen.value?.find(({ identifier }) => identifier === props.systeemId),
);

const deeplink = computed(() => {
  const { deeplinkProperty, deeplinkUrl } = systeem.value || {};
  if (!deeplinkProperty || !deeplinkUrl) return null;
  const property = (props.zaak as Record<string, unknown>)[deeplinkProperty];
  if (!property) return null;
  return deeplinkUrl + property;
});
</script>
