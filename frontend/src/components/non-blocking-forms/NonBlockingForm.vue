<template>
  <prompt-modal :dialog="dialog" confirm-message="Ja" cancel-message="Nee">
    <ul class="errors">
      <li v-for="(message, i) in allMessages" :key="i">
        {{ message }}
      </li>
    </ul>
    <p>Wil je de gegevens toch opslaan?</p>
  </prompt-modal>
  <form v-bind="$attrs" @submit.prevent="submit">
    <slot></slot>
  </form>
</template>

<script lang="ts">
export default {
  inheritAttrs: false,
};
</script>

<script setup lang="ts">
import { useConfirmDialog } from "@vueuse/core";
import { provideValidationMessages } from "./useValidationMessages";
import PromptModal from "../PromptModal.vue";
import type { FormHTMLAttributes } from "vue";

defineProps<FormHTMLAttributes>();

const dialog = useConfirmDialog();

const allMessages = provideValidationMessages();

const emit = defineEmits<{ (e: "submit", payload: Event): void }>();

async function submit(e: Event) {
  if (allMessages.value.length) {
    const { isCanceled } = await dialog.reveal();
    if (isCanceled) return;
  }

  emit("submit", e);
}
</script>

<style lang="scss" scopde>
.errors {
  list-style: disc;
  margin-inline-start: var(--spacing-default);
}
</style>
