<template>
  <dialog ref="dialogRef" @close="onClose">
    <form method="dialog">
      <slot
        ><paragraph v-if="message">
          {{ message }}
        </paragraph></slot
      >
      <menu>
        <li>
          <utrecht-button
            value="cancel"
            type="submit"
            appearance="secondary-action-button"
          >
            {{ cancelMessage }}
          </utrecht-button>
        </li>
        <li>
          <utrecht-button
            type="submit"
            value="confirm"
            appearance="primary-action-button"
            v-focus
          >
            {{ confirmMessage }}
          </utrecht-button>
        </li>
      </menu>
    </form>
  </dialog>
</template>

<script setup lang="ts">
import Paragraph from "@/nl-design-system/components/Paragraph.vue";
import { Button as UtrechtButton } from "@utrecht/component-library-vue";
import { whenever, type UseConfirmDialogReturn } from "@vueuse/core";
import { ref, type PropType } from "vue";

type DialogFromLibrary = UseConfirmDialogReturn<unknown, unknown, unknown>;

const props = defineProps({
  dialog: {
    type: Object as PropType<DialogFromLibrary>,
    required: true,
  },
  message: {
    type: String,
    default: "",
  },
  warning: {
    type: String,
    default: "",
  },
  cancelMessage: {
    type: String,
    default: "Hier blijven",
  },
  confirmMessage: {
    type: String,
    default: "Doorgaan",
  },
});

const dialogRef = ref<HTMLDialogElement>();

const onClose = () => {
  if (dialogRef.value?.returnValue === "confirm") {
    props.dialog.confirm();
  } else {
    props.dialog.cancel();
  }
};

whenever(
  () => props.dialog.isRevealed.value,
  () => {
    dialogRef.value?.showModal();
  },
  { immediate: true },
);
</script>

<style lang="scss" scoped>
menu {
  display: flex;
  gap: var(--spacing-default);
  justify-content: flex-start;
}

form {
  display: flex;
  flex-direction: column;
  gap: var(--spacing-large);
}

dialog {
  border-radius: var(--radius-default);
  padding: var(--spacing-large);
  border: 1px solid var(--color-primary);
  min-width: 50%;
  max-width: 70%;
}

::backdrop {
  background-color: rgb(102 102 102 / 80%);
}
</style>
