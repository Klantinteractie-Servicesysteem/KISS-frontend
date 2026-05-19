<template>
  <prompt-modal
    :dialog="cancelDialog"
    cancel-message="Nee"
    confirm-message="Ja"
  >
    <utrecht-paragraph
      >Weet je zeker dat je het contactmoment wilt annuleren? Alle gegevens
      worden verwijderd.
    </utrecht-paragraph>
    <application-message
      v-if="vragen && vragen.length > 1"
      message-type="error"
    >
      <utrecht-paragraph>
        Let op: Dit contactmoment bevat meerdere vragen. Als je het
        contactmoment annuleert worden alle vragen automatisch afgebroken.<br />
        Het is mogelijk individuele vragen te verwijderen in het afrondscherm.
      </utrecht-paragraph>
    </application-message>
  </prompt-modal>

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
import {
  Button as UtrechtButton,
  Paragraph as UtrechtParagraph,
} from "@utrecht/component-library-vue";
import { useConfirmDialog } from "@vueuse/core";
import PromptModal from "@/components/PromptModal.vue";
import { useContactmomentStore } from "@/stores/contactmoment";
import ApplicationMessage from "@/components/ApplicationMessage.vue";

const router = useRouter();
const cancelDialog = useConfirmDialog();
const contactmomentStore = useContactmomentStore();

const navigateToPersonen = () =>
  contactmomentStore.contactmomentLoopt
    ? router.push(contactmomentStore.huidigContactmoment?.route ?? "personen")
    : router.push({ path: "/" });

const vragen = contactmomentStore.contactmomentLoopt
  ? contactmomentStore.huidigContactmoment?.vragen
  : undefined;

cancelDialog.onConfirm(() => {
  contactmomentStore.stop();
  navigateToPersonen();
});
</script>
