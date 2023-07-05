<template>
  <utrecht-heading :level="2" modelValue>Nieuwe klant</utrecht-heading>

  <simple-spinner v-if="savingKlant" />

  <non-blocking-form v-else @submit.prevent="submit">
    <div class="input-group">
      <label for="voornaam" class="utrecht-form-label">Voornaam</label>
      <input
        id="voornaam"
        type="text"
        class="utrecht-textbox utrecht-textbox--html-input"
        v-model="formData.voornaam"
      />
    </div>

    <div class="input-group">
      <label for="tussenvoegsel" class="utrecht-form-label"
        >Tussenvoegsel</label
      >
      <input
        id="tussenvoegsel"
        type="text"
        class="utrecht-textbox utrecht-textbox--html-input"
        v-model="formData.tussenvoegsel"
      />
    </div>

    <div class="input-group">
      <label for="achternaam" class="utrecht-form-label">Achternaam</label>
      <input
        id="achternaam"
        type="text"
        class="utrecht-textbox utrecht-textbox--html-input"
        v-model="formData.achternaam"
      />
    </div>

    <div class="input-group">
      <label for="email" class="utrecht-form-label">E-mailadres</label>
      <input
        id="email"
        type="email"
        class="utrecht-textbox utrecht-textbox--html-input"
        v-model="formData.email"
      />
    </div>

    <non-blocking-errors
      :validate="customPhoneValidator"
      :value="formData.telefoonnummer"
    >
      <template #default>
        <div class="input-group">
          <label for="telefoonnummer" class="utrecht-form-label"
            >Telefoonnummer</label
          >
          <input
            v-model="formData.telefoonnummer"
            id="telefoonnummer"
            type="tel"
            class="utrecht-textbox utrecht-textbox--html-input"
          />
        </div>
      </template>
    </non-blocking-errors>

    <application-message
      v-if="formWarning"
      :message="formWarning"
      messageType="error"
    />

    <menu>
      <li>
        <utrecht-button
          modelValue
          @click="props.handleCancel"
          type="button"
          appearance="secondary-action-button"
          >Annuleren</utrecht-button
        >
      </li>
      <li>
        <utrecht-button appearance="primary-action-button" type="submit">
          Opslaan
        </utrecht-button>
      </li>
    </menu>
  </non-blocking-form>
</template>

<script setup lang="ts">
import {
  Heading as UtrechtHeading,
  Button as UtrechtButton,
} from "@utrecht/component-library-vue";
import { customPhoneValidator } from "@/helpers/validation";
import { ref, computed } from "vue";
import { createKlant } from "@/features/contactverzoek";
import SimpleSpinner from "@/components/SimpleSpinner.vue";
import ApplicationMessage from "@/components/ApplicationMessage.vue";
import { useRouter } from "vue-router";
import NonBlockingForm from "@/components/non-blocking-forms/NonBlockingForm.vue";
import NonBlockingErrors from "@/components/non-blocking-forms/NonBlockingErrors.vue";

const props = defineProps<{
  handleCancel: () => void;
}>();

const formData = ref({
  voornaam: "",
  tussenvoegsel: "",
  achternaam: "",
  email: "",
  telefoonnummer: "",
});

const router = useRouter();

const savingKlant = ref(false);

const attemptSaveKlant = ref(false);
const formWarning = computed(() => {
  return !formData.value.email &&
    !formData.value.telefoonnummer &&
    attemptSaveKlant.value
    ? "Je moet minimaal een e-mailadres óf telefoonnummer invoeren."
    : "";
});

const submit = async () => {
  attemptSaveKlant.value = true;
  if (formWarning.value) return;

  savingKlant.value = true;

  const nieuweKlant = {
    voornaam: formData.value.voornaam,
    voorvoegselAchternaam: formData.value.tussenvoegsel,
    achternaam: formData.value.achternaam,
    telefoonnummers: formData.value.telefoonnummer
      ? [{ telefoonnummer: formData.value.telefoonnummer }]
      : [],
    emails: formData.value.email ? [{ email: formData.value.email }] : [],
  };

  const { id } = await createKlant(nieuweKlant);

  router.push("/personen/" + id);
};
</script>

<style scoped lang="scss">
* ~ :deep(form) {
  max-width: 37.5rem;

  & > *:not(:last-child) {
    margin-block-end: var(--spacing-large);
  }
}

.input-group {
  display: flex;
  align-items: center;

  label {
    width: 9.375rem;
  }

  input {
    flex: 1;
    max-width: 28.125rem;
  }
}

.warning-container {
  display: flex;
  justify-content: flex-end;
}

.warning {
  max-width: 450px;
  color: var(--color-error);
}

label {
  user-select: none;
}

menu {
  display: flex;
  gap: var(--spacing-default);
  justify-content: flex-end;
}
</style>
