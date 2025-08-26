<template>
  <prompt-modal
    :dialog="cancelDialog"
    message="Weet je zeker dat je het contactmoment wilt annuleren? Alle gegevens worden verwijderd."
    cancel-message="Nee"
    confirm-message="Ja"
  />

  <menu>
    <li>
      <utrecht-button
        title="Contactmoment Annuleren"
        class="Annuleren"
        appearance="secondary-action-button"
        @click="cancelDialog.reveal"
      >
        Annuleren
      </utrecht-button>
    </li>
    <li>
      <utrecht-button
        @click="onStopContactMoment"
        title="Contactmoment afronden"
        appearance="primary-action-button"
        class="afronden"
      >
        Afronden
      </utrecht-button>
    </li>
  </menu>
</template>

<script lang="ts" setup>
import { useRouter } from "vue-router";
import { Button as UtrechtButton } from "@utrecht/component-library-vue";
import { useConfirmDialog } from "@vueuse/core";
import PromptModal from "@/components/PromptModal.vue";
import { useContactmomentStore } from "@/stores/contactmoment";
const router = useRouter();
const onStopContactMoment = async () => router.push({ name: "afhandeling" }); // een link zou wellicht toepasselijker zijn, maar de styling adhv het designsystem wordt lastig.
const cancelDialog = useConfirmDialog();
const contactmomentStore = useContactmomentStore();

const navigateToPersonen = () => router.push({ name: "personen" });

cancelDialog.onConfirm(() => {
  contactmomentStore.stop();
  navigateToPersonen();
});
</script>

<style lang="scss" scoped>
/* stylelint-disable custom-property-pattern */
menu {
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: var(--spacing-large);
}

.afronden {
  --_utrecht-button-background-color: var(--color-error);
  --_utrecht-button-focus-background-color: var(--color-error);
  --_utrecht-button-active-background-color: var(--color-error);
  --_utrecht-button-hover-background-color: var(--color-error-hover);
  --_utrecht-button-color: var(--color-headings);
  --_utrecht-button-hover-color: var(--color-headings);
  --_utrecht-button-active-color: var(--color-headings);
  --_utrecht-button-focus-color: var(--color-headings);
}
</style>
