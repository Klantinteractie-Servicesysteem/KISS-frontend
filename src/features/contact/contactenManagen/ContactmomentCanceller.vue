<template>
  <prompt-modal
    :dialog="cancelDialog"
    message="Weet je zeker dat je het contactmoment wilt annuleren? Alle gegevens worden verwijderd."
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
import { useRouter } from "vue-router";
import { Button as UtrechtButton } from "@utrecht/component-library-vue";
import { useConfirmDialog } from "@vueuse/core";
import PromptModal from "@/components/PromptModal.vue";
import { useContactmomentStore } from "@/stores/contactmoment/index";
const router = useRouter();
const cancelDialog = useConfirmDialog();
const contactmomentStore = useContactmomentStore();

const navigateToPersonen = () =>
  contactmomentStore.contactmomentLoopt
    ? router.push(contactmomentStore.huidigContactmoment?.route ?? "personen")
    : router.push({ path: "/" });

cancelDialog.onConfirm(() => {
  contactmomentStore.stop();
  navigateToPersonen();
});
</script>
