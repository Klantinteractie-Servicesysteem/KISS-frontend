<template>
  <simple-spinner v-if="serviceResult?.loading"></simple-spinner>

  <form v-else @submit.prevent="submit">
    <fieldset class="utrecht-form-fieldset">
      <label for="current-section" class="utrecht-form-label"
        >Huidige sectie</label
      >
      <input
        disabled
        v-model="currentSectionLabel"
        type="text"
        id="current-section"
        class="utrecht-textbox utrecht-textbox--html-input"
      />

      <label for="content" class="utrecht-form-label"
        >Citeer de tekst waar het om gaat</label
      >
      <textarea
        id="content"
        v-model="feedback.content"
        class="utrecht-textarea utrecht-textarea--html-textarea"
      ></textarea>

      <label for="opmerking" class="utrecht-form-label"
        ><span class="required">Omschrijf wat je feedback is</span></label
      >
      <textarea
        id="opmerking"
        v-model="feedback.opmerking"
        class="utrecht-textarea utrecht-textarea--html-textarea"
        required
      ></textarea>

      <label for="aanleiding" class="utrecht-form-label"
        >Wat is de aanleiding?</label
      >
      <textarea
        id="aanleiding"
        v-model="feedback.aanleiding"
        class="utrecht-textarea utrecht-textarea--html-textarea"
      ></textarea>

      <label for="contactgegevens" class="utrecht-form-label"
        >Het e-mailadres of telefoonnummer waarop de auteur jou kan
        bereiken</label
      >
      <input
        type="text"
        id="contactgegevens"
        v-model="feedback.contactgegevens"
        class="utrecht-textbox utrecht-textbox--html-input"
      />
    </fieldset>

    <application-message
      v-if="serviceResult?.error"
      messageType="error"
      message="Er is een fout opgetreden"
    ></application-message>

    <menu>
      <li>
        <utrecht-button
          @click="cancelDialog.reveal"
          appearance="secondary-action-button"
          type="button"
        >
          Annuleren
        </utrecht-button>
      </li>

      <li>
        <utrecht-button appearance="primary-action-button" type="submit">
          Verzenden
        </utrecht-button>
      </li>
    </menu>
  </form>

  <!-- Annuleer Dialog -->
  <prompt-modal
    :dialog="cancelDialog"
    message="Weet je zeker dat je wilt annuleren? Alle gegevens worden verwijderd."
    cancel-message="Nee"
    confirm-message="Ja"
  />
</template>

<script lang="ts" setup>
import { ref, reactive, computed } from "vue";
import { useFeedbackService } from "../service";
import type { CurrentFeedbackSection, Feedback } from "../types";
import { useConfirmDialog } from "@vueuse/core";
import SimpleSpinner from "@/components/SimpleSpinner.vue";
import ApplicationMessage from "@/components/ApplicationMessage.vue";
import PromptModal from "@/components/PromptModal.vue";
import type { ServiceData } from "@/services/index";
import { useUserStore } from "@/stores/user";
import { Button as UtrechtButton } from "@utrecht/component-library-vue";

const props = defineProps<{
  url: unknown | URL;
  name: string;
  currentSection: CurrentFeedbackSection;
}>();

const currentSectionLabel = computed(() => props.currentSection.label);

const userStore = useUserStore();

const serviceResult = ref<ServiceData<void>>();
const service = useFeedbackService();
const cancelDialog = useConfirmDialog();
const emit = defineEmits(["cancelled", "saved"]);
const feedback: Feedback = reactive({
  naam: props.name,
  url: props.url,
  content: "",
  opmerking: "",
  aanleiding: "",
  contactgegevens: userStore.user.isLoggedIn ? userStore.user.email : "",
  currentSection: props.currentSection,
});

cancelDialog.onConfirm(() => annuleren());

const submit = () => {
  const result = service.postFeedback(feedback);
  serviceResult.value = result.state;

  result.promise.then(() => {
    clear();
    emit("saved");
  });
};

//maak leeg en verberg het formulier
const annuleren = () => {
  clear();
  emit("cancelled");
};

const clear = () => {
  feedback.content = "";
  feedback.opmerking = "";
  feedback.contactgegevens = "";
  feedback.aanleiding = "";
};
</script>

<style lang="scss" scoped>
fieldset {
  display: grid;
  align-items: center;
  grid-gap: var(--spacing-default);
  grid-template-columns: 1fr 2fr;
}

label {
  grid-column: 1 / 2;
}

menu {
  margin-top: var(--spacing-large);
  display: flex;
  gap: var(--spacing-default);
  justify-content: flex-end;
}

.error,
.confirm {
  margin-top: var(--spacing-default);
}
</style>
