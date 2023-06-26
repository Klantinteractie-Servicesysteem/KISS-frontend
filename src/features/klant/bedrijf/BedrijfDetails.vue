<template>
  <article class="details-block">
    <non-blocking-form @submit.prevent="submit">
      <header class="heading-container">
        <utrecht-heading :level="level">
          <span class="heading">
            Gegevens klant
            <utrecht-button
              v-if="!editing"
              appearance="subtle-button"
              @click="toggleEditing"
              title="Bewerken"
              :class="'icon-after icon-only pen'"
              class="toggleEdit"
            />
            <simple-spinner class="spinner" v-if="submitter.loading" />
            <application-message
              v-else-if="submitter.error"
              class="error-message"
              message="Er ging iets mis. Probeer het later nog eens."
              :auto-close="true"
              message-type="error"
            />
          </span>
        </utrecht-heading>

        <menu v-if="showForm" class="buttons-container">
          <li>
            <utrecht-button
              @click="reset"
              type="reset"
              appearance="secondary-action-button"
            >
              Annuleren
            </utrecht-button>
          </li>
          <li>
            <utrecht-button appearance="primary-action-button" type="submit"
              >Opslaan</utrecht-button
            >
          </li>
        </menu>
      </header>
      <dl>
        <dt>Bedrijfsnaam</dt>
        <dd>
          {{ klant.bedrijfsnaam }}
        </dd>
        <dt>E-mailadres</dt>
        <dd>
          <fieldset v-if="showForm">
            <input
              v-model="email"
              type="email"
              name="E-mail"
              aria-label="E-mail"
              class="utrecht-textbox utrecht-textbox--html-input"
            />
          </fieldset>
          <template v-else>{{ email }}</template>
        </dd>
        <dt>Telefoonnummer</dt>
        <dd>
          <fieldset v-if="showForm">
            <non-blocking-errors
              :value="telefoonnummer || ''"
              :validate="customPhoneValidator"
            />
            <input
              v-model="telefoonnummer"
              type="tel"
              name="Telefoonnummer"
              aria-label="Telefoonnummer"
              class="utrecht-textbox utrecht-textbox--html-input"
            />
          </fieldset>
          <template v-else>{{ telefoonnummer }}</template>
        </dd>
      </dl>
    </non-blocking-form>
  </article>
</template>

<script lang="ts" setup>
import { ref, watch, type PropType } from "vue";
import {
  Heading as UtrechtHeading,
  Button as UtrechtButton,
} from "@utrecht/component-library-vue";
import type { Klant } from "../types";
import { useUpdateContactGegevens } from "../service";
import SimpleSpinner from "@/components/SimpleSpinner.vue";
import { computed } from "vue";
import ApplicationMessage from "@/components/ApplicationMessage.vue";
import {
  NonBlockingErrors,
  NonBlockingForm,
} from "@/components/non-blocking-forms";
import { customPhoneValidator } from "@/helpers/validation";

const props = defineProps({
  klant: {
    type: Object as PropType<Klant>,
    required: true,
  },
  level: {
    type: Number as PropType<1 | 2 | 3 | 4 | 5>,
    default: 2,
  },
});

const email = ref(props.klant.emailadres);
const telefoonnummer = ref(props.klant.telefoonnummer);

function populate() {
  email.value = props.klant.emailadres;
  telefoonnummer.value = props.klant.telefoonnummer;
}

watch(
  () => props.klant,
  () => {
    if (!editing.value) {
      reset();
    }
  },
  { deep: true }
);

const editing = ref<boolean>(false);

const toggleEditing = (): void => {
  editing.value = !editing.value;
};

const reset = () => {
  submitter.reset();
  populate();
  editing.value = false;
};

const submitter = useUpdateContactGegevens();

const submit = () =>
  submitter
    .submit({
      id: props.klant.id,
      telefoonnummer: telefoonnummer.value,
      emailadres: email.value,
    })
    .then(() => {
      editing.value = false;
    });

const showForm = computed(() => !submitter.loading && editing.value);
</script>

<style lang="scss" scoped>
.heading-container {
  display: flex;
  align-items: center;
  justify-content: space-between;

  .heading {
    display: flex;
    align-items: center;
    gap: var(--spacing-small);
  }

  .buttons-container {
    display: flex;
    align-items: center;
    gap: var(--spacing-default);
  }
}

.add-item {
  display: flex;
  color: white;
  margin-inline-start: auto;
}

.spinner {
  font-size: 16px;
}

.error-message {
  font-size: 50%;
}

dd fieldset {
  // flex seems to mess up icons
  display: grid;
  grid-auto-flow: column;
  gap: var(--spacing-default);
  align-items: center;
}

:disabled {
  cursor: not-allowed;
}
</style>
