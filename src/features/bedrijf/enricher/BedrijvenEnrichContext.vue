<template>
  <dialog ref="dialog">
    <simple-spinner />
  </dialog>
  <bedrijf-enrich-context
    v-for="(record, idx) in records"
    :key="idx"
    :record="record"
  >
    <template #default="{ enriched }">
      <slot :enriched="enriched"></slot>
    </template>
  </bedrijf-enrich-context>
</template>

<script setup lang="ts">
import { ref } from "vue";
import BedrijfEnrichContext from "./BedrijfEnrichContext.vue";
import SimpleSpinner from "@/components/SimpleSpinner.vue";
import type { BedrijfHandelsregister, BedrijfKlant } from "../types";

defineProps<{ records: Array<BedrijfHandelsregister | BedrijfKlant> }>();

const dialog = ref<HTMLDialogElement>();
const wrapCreate = (create: () => Promise<void>) => () => {
  if (dialog.value?.open) return Promise.resolve();
  dialog.value?.showModal();
  return create().finally(() => {
    dialog.value?.close();
  });
};
</script>

<style lang="scss" scoped>
dialog[open] {
  width: 100%;
  height: 100%;
  display: flex;
  place-content: center;
  place-items: center;
}
</style>
