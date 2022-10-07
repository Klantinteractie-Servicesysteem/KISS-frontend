<!--
nb. de gekozen oplossing met het vullen van de klant gegevens obv de 
geselecteerde klant in de klantstore levert in bepaalde edge cases problemen op
zo kan een dezelfde klant niet meer geselecteerd worden nadat deze is 
verwijderd uit het formulier. (de verwijder optie is nodig omdat je
anders niet meer vrij klantgegevens kan invullen als er toevallig al 
een klant geselecteerd was in het contact moment. het is mogelijk dat je 
bv een klant electeert om er vervolgens achter te komen dat dit toch niet
de juiste persoon is)

het zou beter zijn als alle tijdens het contactmoment gevonden klanten hier
uit een lijst geselecteerd zouden kunnen worden, met een 'anders namelijk' optie 
erbij voor het vrij invullen. 

-->
<template>
  <utrecht-heading model-value :level="2">Contactverzoek maken</utrecht-heading>
  <SimpleSpinner class="spinner" v-if="loading" />
  <p v-else-if="error">Er ging iets mis. Probeer het later nog eens.</p>
  <non-blocking-form
    @submit.prevent="submit"
    class="form"
    v-else
    v-bind="$attrs"
  >
    <fieldset class="utrecht-form-fieldset">
      <medewerker-search
        class="utrecht-textbox utrecht-textbox--html-input"
        v-model="formData.medewerker"
        @update:model-value="setFormInProgress"
        required
      >
        <template #label
          ><span class="utrecht-form-label"
            >Contactverzoek versturen naar</span
          ></template
        >
      </medewerker-search>
    </fieldset>
    <fieldset class="utrecht-form-fieldset">
      <label class="utrecht-form-label">
        <span class="required">Voornaam van de klant</span>
        <input
          type="text"
          v-model="formData.voornaam"
          class="utrecht-textbox utrecht-textbox--html-input"
          required
          :disabled="klantReadonly"
          @input="setFormInProgress"
        />
      </label>

      <label class="utrecht-form-label">
        <span>Tussenvoegsel van de klant</span>
        <input
          type="text"
          v-model="formData.voorvoegselAchternaam"
          class="utrecht-textbox utrecht-textbox--html-input"
          :disabled="klantReadonly"
          @input="setFormInProgress"
        />
      </label>

      <label class="utrecht-form-label">
        <span class="required">Achternaam van de klant</span>
        <input
          type="text"
          v-model="formData.achternaam"
          class="utrecht-textbox utrecht-textbox--html-input"
          required
          :disabled="klantReadonly"
          @input="setFormInProgress"
        />
      </label>

      <label class="utrecht-form-label">
        E-mailadres van de klant
        <input
          type="email"
          name="klant-email"
          v-model="formData.emailadres"
          class="utrecht-textbox utrecht-textbox--html-input"
          :disabled="klantReadonly"
          @input="setFormInProgress"
        />
      </label>

      <label class="utrecht-form-label">
        Telefoonnummer 1 van de klant
        <input
          v-if="klantReadonly"
          type="tel"
          :value="formData.telefoonnummer1"
          class="utrecht-textbox utrecht-textbox--html-input"
          :disabled="true"
        />
        <non-blocking-errors
          :validate="customPhoneValidator"
          :value="formData.telefoonnummer1"
          v-else
        >
          <template #default>
            <input
              type="tel"
              v-model="formData.telefoonnummer1"
              class="utrecht-textbox utrecht-textbox--html-input"
              @input="setFormInProgress"
            />
          </template>
        </non-blocking-errors>
      </label>

      <label class="utrecht-form-label">
        Telefoonnummer 2 van de klant
        <input
          v-if="klantReadonly"
          type="tel"
          :value="formData.telefoonnummer2"
          class="utrecht-textbox utrecht-textbox--html-input"
          :disabled="true"
        />
        <non-blocking-errors
          :value="formData.telefoonnummer2"
          :validate="customPhoneValidator"
          v-else
        >
          <template #default>
            <input
              type="tel"
              v-model="formData.telefoonnummer2"
              class="utrecht-textbox utrecht-textbox--html-input"
              @input="setFormInProgress"
            />
          </template>
        </non-blocking-errors>
      </label>

      <button
        v-if="klantReadonly"
        type="button"
        @click.prevent="wisGeselecteerdeKlant"
        class="utrecht-button utrecht-button--secondary-action"
        tabindex="-1"
      >
        Verwijder deze klantgegevens uit het contactverzoek
      </button>
    </fieldset>

    <label class="utrecht-form-label notitieveld">
      <span class="required">Notitie bij het contactverzoek</span>
      <textarea
        v-model="formData.description"
        class="utrecht-textarea utrecht-textarea--html-textarea"
        required
        @input="setFormInProgress"
      />
    </label>

    <application-message
      v-if="emailRequiredMessage"
      :message="emailRequiredMessage"
      messageType="error"
    />

    <utrecht-button model-value type="submit">
      Contactverzoek versturen
    </utrecht-button>
  </non-blocking-form>
</template>

<script lang="ts">
export default {
  inheritAttrs: false,
};
</script>

<script lang="ts" setup>
import { ref, watch, computed } from "vue";
import type { Vraag } from "@/stores/contactmoment";
import { saveContactverzoek, createKlant } from "./service";
import {
  UtrechtButton,
  UtrechtHeading,
} from "@utrecht/web-component-library-vue";
import { koppelKlant } from "../contactmoment";
import MedewerkerSearch from "../search/MedewerkerSearch.vue";
import SimpleSpinner from "@/components/SimpleSpinner.vue";
import ApplicationMessage from "@/components/ApplicationMessage.vue";
import {
  NonBlockingForm,
  NonBlockingErrors,
} from "@/components/non-blocking-forms";
import { customPhoneValidator } from "@/helpers/validation";
import { ensureState } from "@/stores/create-store";
import type { Klant } from "../klant/types";

const props = defineProps<{
  huidigeVraag: Vraag;
  huidigeKlant: Klant | undefined;
}>();

//EVENTS
const START = "start";
const SUBMIT = "submit";

const emit = defineEmits<{
  (e: typeof START): void;
  (e: typeof SUBMIT, data: { url: string; medewerker: string }): void;
}>();

const emitStarted = () => emit(START);
// END EVENTS

const loading = ref(false);
const error = ref(false);

const formData = ensureState({
  stateId: "contactverzoekForm",
  stateFactory() {
    return {
      voornaam: "",
      voorvoegselAchternaam: undefined as string | undefined,
      achternaam: "",
      telefoonnummer1: "",
      telefoonnummer2: "",
      emailadres: "",
      medewerker: "",
      description: "",
      useExistingKlant: false,
    };
  },
});

const klantReadonly = computed(() => formData.value.useExistingKlant);
const emailIsRequired = computed(
  () =>
    !klantReadonly.value &&
    !formData.value.telefoonnummer1 &&
    !formData.value.telefoonnummer2 &&
    !formData.value.emailadres
);

const emailRequiredMessage = computed(() =>
  emailIsRequired.value
    ? "Vul een minimaal een e-mailadres of een telefoonnummer van de klant in"
    : ""
);

watch(
  () => props.huidigeKlant,
  (klant) => {
    formData.value.voornaam = klant?.voornaam || "";
    formData.value.voorvoegselAchternaam = klant?.voorvoegselAchternaam;
    formData.value.achternaam = klant?.achternaam || "";
    formData.value.emailadres = klant?.emails?.[0]?.email || "";
    formData.value.telefoonnummer1 =
      klant?.telefoonnummers?.[0]?.telefoonnummer || "";
    formData.value.telefoonnummer2 =
      klant?.telefoonnummers?.[1]?.telefoonnummer || "";
    formData.value.useExistingKlant = klant != null;
  },
  { immediate: true, deep: true }
);

watch(
  () => props.huidigeVraag.notitie,
  (newNote, oldNote) => {
    if (
      newNote &&
      (!formData.value.description || formData.value.description === oldNote)
    ) {
      formData.value.description = newNote;
    }
  },
  { immediate: true }
);

watch(
  () => props.huidigeVraag,
  () => {
    formData.reset();
  }
);

async function submit() {
  try {
    const {
      telefoonnummer1,
      telefoonnummer2,
      emailadres,
      voornaam,
      voorvoegselAchternaam,
      achternaam,
      description,
      medewerker,
      useExistingKlant,
    } = formData.value;

    if (emailRequiredMessage.value) return;

    loading.value = true;

    let klantId = "";

    if (!useExistingKlant) {
      const telefoonnummers = [telefoonnummer1, telefoonnummer2]
        .filter(Boolean)
        .map((telefoonnummer) => ({ telefoonnummer }));

      const emails = emailadres ? [{ email: emailadres }] : [];

      const newKlantResult = await createKlant({
        telefoonnummers,
        emails,
        voornaam,
        voorvoegselAchternaam,
        achternaam,
      });

      klantId = newKlantResult.id;
      formData.value.useExistingKlant = true;
    } else {
      klantId = props.huidigeKlant?.id ?? "";
    }

    if (!klantId) {
      throw new Error("kan klant niet koppelen, id ontbreekt");
    }

    const { url, id } = await saveContactverzoek({
      bronorganisatie: window.organisatieIds[0],
      todo: {
        name: "contactverzoek",
        description,
        attendees: [medewerker],
      },
    });

    await koppelKlant({ klantId, contactmomentId: id });

    emit(SUBMIT, { url, medewerker });
  } catch (e) {
    error.value = true;
  } finally {
    loading.value = false;
  }
}

const wisGeselecteerdeKlant = () => {
  formData.value.voornaam = "";
  formData.value.voorvoegselAchternaam = "";
  formData.value.achternaam = "";
  formData.value.emailadres = "";
  formData.value.telefoonnummer1 = "";
  formData.value.telefoonnummer2 = "";
  formData.value.useExistingKlant = false;
};

const setFormInProgress = (e: any) => {
  if (typeof e === "string" && !e) return;
  if (typeof e === "object" && !e?.target?.value) return;
  emitStarted();
};
</script>

<style lang="scss" scoped>
@import "@utrecht/component-library-css";

menu {
  display: flex;

  justify-content: flex-end;
}

.error,
.confirm {
  margin-top: var(--spacing-default);
}

//hack: non-blocking-form styling
* + :deep(form) {
  flex: 1;
  display: flex;
  gap: var(--spacing-default);
  flex-direction: column;
}

fieldset {
  --gap: var(--spacing-default);
  gap: var(--gap);
  display: grid;

  &::after {
    content: "";
    margin-inline: calc(-1 * var(--gap));
    height: 0.5rem;
    background-color: var(--color-tertiary);
  }
}

label {
  display: grid;
  gap: var(--spacing-small);
}

label.radio {
  grid-template-columns: min-content 1fr;
  align-items: center;

  input {
    margin: 0;
  }
}

legend {
  margin-block-end: var(--spacing-small);
}

utrecht-heading {
  margin-block-end: var(--spacing-default);
}

utrecht-button {
  --utrecht-button-min-inline-size: 100%;
  --utrecht-button-min-block-size: 3rem;
}

.notitieveld {
  display: flex;
  flex-direction: column;
}

.notitieveld textarea.utrecht-textarea {
  padding: var(--spacing-small);
  margin-block-start: 0;
  min-height: 20rem;
}

.spinner {
  color: var(--color-primary);
}
</style>
