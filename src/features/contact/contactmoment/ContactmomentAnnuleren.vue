<template>
  <prompt-modal
    :dialog="cancelDialog"
    message="Weet je zeker dat je wilt annuleren? Alle gegevens worden verwijderd."
    cancel-message="Nee"
    confirm-message="Ja"
  />

  <utrecht-button
    title="Contactmoment Annuleren"
    class="Annuleren"
    appearance="secondary-action-button"
    @click="cancelDialog.reveal"
  >
    Annuleren
  </utrecht-button>
</template>

<script lang="ts" setup>
import { Button as UtrechtButton } from "@utrecht/component-library-vue";
import { useConfirmDialog } from "@vueuse/core";
import PromptModal from "@/components/PromptModal.vue";
import { useRouter } from "vue-router";
import { useContactmomentStore } from "@/stores/contactmoment";
import { useAttrs } from "vue";

const emit = defineEmits(["cancel-confirmed"]);
const attrs = useAttrs();
const router = useRouter();
const contactmomentStore = useContactmomentStore();
const cancelDialog = useConfirmDialog();

const navigateToPersonen = () =>
  contactmomentStore.contactmomentLoopt
    ? router.push(contactmomentStore.huidigContactmoment?.route ?? "personen")
    : router.push({ path: "/" });

cancelDialog.onConfirm(() => {
  const hasListeners =
    "onCancel-confirmed" in attrs || "oncancel-confirmed" in attrs;

  if (hasListeners) {
    emit("cancel-confirmed");
  } else {
    contactmomentStore.stop();
    navigateToPersonen();
  }
});
</script>
