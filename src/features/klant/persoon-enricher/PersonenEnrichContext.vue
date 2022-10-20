<template>
  <dialog ref="dialog">
    <simple-spinner />
  </dialog>
  <persoon-enrich-context
    v-for="(record, idx) in records"
    :key="idx"
    :record="record"
  >
    <template #default="{ enriched }">
      <slot
        :enriched="{ ...enriched, create: wrapCreate(enriched.create) }"
      ></slot>
    </template>
  </persoon-enrich-context>
</template>

<script setup lang="ts">
import { ref } from "vue";
import type { Persoon } from "../types";
import PersoonEnrichContext from "./PersoonEnrichContext.vue";
import SimpleSpinner from "@/components/SimpleSpinner.vue";
import type { Klant } from "@/features/shared/types";

defineProps<{ records: Klant[] | Persoon[] }>();

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
