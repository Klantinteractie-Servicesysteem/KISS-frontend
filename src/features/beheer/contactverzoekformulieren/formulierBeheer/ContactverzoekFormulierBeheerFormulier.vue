<template>
  <prompt-modal
    :dialog="cancelDialog"
    message="Weet je zeker dat je het proces wil annuleren? Niet opgeslagen gegevens zullen verloren gaan."
    cancel-message="Nee"
    confirm-message="Ja"
  />
  <utrecht-heading :level="1">{{
    props.id ? "Formulier bewerken" : "Formulier aanmaken"
  }}</utrecht-heading>
  <template v-if="loading"> <simple-spinner /></template>
  <div>
    <pre>
      Hier kan je een template maken voor een contactverzoek. Houd er rekening mee dat
      dit template een aanvulling is op de standaard vragen.
      Deze hoef je hier dus niet toe te voegen. De standaardvragen zijn: 
      - Klantnaam
      - Telefoonnummer 1
      - Telefoonnummer 2 
      - Omschrijving telefoonnummer 2 
      - E-mailadres 
      - Interne toelichting voor medewerker
   
    </pre>
  </div>
  <form class="container" @submit.prevent="submit">
    <label class="utrecht-form-label" for="titel"
      ><span class="required">Titel </span>
      <input
        class="utrecht-textbox utrecht-textbox--html-input"
        required
        type="text"
        id="titel"
        v-model="title"
      />
    </label>

    <!-- dropdown for afdelingen -->
    <div
      v-if="soort === TypeOrganisatorischeEenheid.Afdeling"
      class="utrecht-form-label"
    >
      <label class="required">Afdeling</label>
      <service-data-search
        class="utrecht-textbox utrecht-textbox--html-input"
        :required="true"
        placeholder="Zoek een afdeling"
        :get-data="useAfdelingen"
        v-model="selectedOrganisatorischeEenheid"
        options-label="Afdelingen"
        :map-value="(x) => x?.naam"
        :map-description="(x) => x?.identificatie"
      />
    </div>

    <!-- dropdown for groepen -->
    <div
      v-else-if="soort === TypeOrganisatorischeEenheid.Groep"
      class="utrecht-form-label"
    >
      <label class="required">Groep</label>
      <service-data-search
        class="utrecht-textbox utrecht-textbox--html-input"
        :required="true"
        placeholder="Zoek een groep"
        options-label="Groepen"
        :get-data="useGroepen"
        v-model="selectedOrganisatorischeEenheid"
        :map-value="(x) => x?.naam"
        :map-description="(x) => x?.identificatie"
      />
    </div>

    <!-- Loop through vragen and render label and input field -->
    <template v-for="(vraag, index) in vragen" :key="index">
      <div class="question-container">
        <div class="question-container-label utrecht-form-label">
          <span
            >Vraag {{ index + 1 }} -
            {{ setVraagTypeDescription(vraag.questiontype) }}</span
          >
        </div>
        <!-- Conditional part for Input -->
        <div
          class="questions"
          v-if="
            isInputVraag(vraag) ||
            isTextareaVraag(vraag) ||
            isCheckboxVraag(vraag) ||
            isDropdownVraag(vraag)
          "
        >
          <div
            class="question-wrapper"
            v-on:mouseenter="showQuestionTrashButton(vraag.id, index)"
            v-on:mouseleave="hideQuestionTrashButton(vraag.id, index)"
          >
            <div class="question-inputfield-wrapper">
              <input
                class="utrecht-textbox utrecht-textbox--html-input"
                required
                type="text"
                v-model="vraag.description"
                placeholder="Vul hier een beschrijving voor de vraag in"
              />
            </div>
            <div
              class="question-delete-button"
              v-if="hoverId === vraag.id && hoverIndex === index"
            >
              <!-- delete button -->
              <utrecht-button
                appearance="secondary-action-button"
                class="icon icon-after trash icon-only"
                type="button"
                @click="removeVraag(vraag.id)"
              />
            </div>
          </div>
        </div>
      </div>
      <!-- Conditional part for Dropdown -->
      <div v-if="isDropdownVraag(vraag)">
        <label>
          <span>Antwoordopties:</span>
        </label>
        <template
          v-for="(option, optionIndex) in vraag.options"
          :key="optionIndex"
        >
          <div
            class="question-wrapper"
            v-on:mouseenter="showOptionTrashButton(vraag.id, optionIndex)"
            v-on:mouseleave="hideOptionTrashButton(vraag.id, optionIndex)"
          >
            <div class="question-inputfield-wrapper">
              <input
                class="utrecht-textbox utrecht-textbox--html-input"
                required
                type="text"
                v-model="vraag.options[optionIndex]"
                :placeholder="`Voeg hier optie ${optionIndex + 1} toe`"
              />
            </div>
            <div
              v-if="
                optionHoverId === vraag.id && optionHoverIndex === optionIndex
              "
              class="question-delete-button"
            >
              <utrecht-button
                appearance="secondary-action-button"
                class="icon icon-after trash icon-only"
                :title="`Verwijder optie ${optionIndex + 1}`"
                type="button"
                @click="removeOption(vraag.id, optionIndex)"
              />
            </div>
          </div>
        </template>
        <div class="option-add-wrapper">
          <div class="option-add-button">
            <utrecht-button
              appearance="secondary-action-button"
              class="icon icon-after plus"
              title="Antwoordoptie Toevoegen"
              type="button"
              @click="addOption(vraag.id)"
            >
              Antwoordoptie Toevoegen
            </utrecht-button>
          </div>
        </div>
      </div>
      <!-- Conditional part for checkbox -->
      <div v-if="isCheckboxVraag(vraag)">
        <label>
          <span>Antwoordopties:</span>
        </label>
        <template
          v-for="(option, optionIndex) in vraag.options"
          :key="optionIndex"
        >
          <div
            class="question-wrapper"
            v-on:mouseenter="showOptionTrashButton(vraag.id, optionIndex)"
            v-on:mouseleave="hideOptionTrashButton(vraag.id, optionIndex)"
          >
            <div class="options-wrapper">
              <input
                class="utrecht-textbox utrecht-textbox--html-input"
                required
                type="text"
                v-model="vraag.options[optionIndex]"
                :placeholder="`Voeg hier optie ${optionIndex + 1} toe`"
              />
            </div>
            <div
              v-if="
                optionHoverId === vraag.id && optionHoverIndex === optionIndex
              "
              class="question-delete-button"
            >
              <utrecht-button
                appearance="secondary-action-button"
                class="icon icon-after trash icon-only"
                :title="`Verwijder optie ${optionIndex + 1}`"
                type="button"
                @click="removeOption(vraag.id, optionIndex)"
              />
            </div>
          </div>
        </template>
        <div class="option-add-button">
          <utrecht-button
            appearance="secondary-action-button"
            class="icon icon-after plus"
            title="Label Toevoegen"
            type="button"
            @click="addOption(vraag.id)"
          >
            Antwoordoptie Toevoegen
          </utrecht-button>
        </div>
      </div>
    </template>
    <!-- Dropdown for vraag -->
    <label class="utrecht-form-label">
      <span>Vraag toevoegen</span>
      <select
        class="utrecht-select utrecht-select--html-select"
        v-model="selectedVraag"
        @change="handleVraagChange()"
      >
        <option disabled value="Vraag toevoegen">Kies een vraag</option>
        <option value="input">Open vraag kort</option>
        <option value="textarea">Open vraag lang</option>
        <option value="dropdown">Dropdown</option>
        <option value="checkbox">Checkbox</option>
      </select>
    </label>

    <menu>
      <li class="utrecht-form-actions">
        <utrecht-button
          modelValue
          type="button"
          appearance="secondary-action-button"
          @click="revealCancelDialog"
        >
          Annuleren
        </utrecht-button>
      </li>

      <li class="utrecht-form-actions">
        <utrecht-button appearance="primary-action-button" type="submit">
          Opslaan
        </utrecht-button>
      </li>
    </menu>
  </form>
</template>

<script setup lang="ts">
import { onMounted, ref } from "vue";
import { useConfirmDialog } from "@vueuse/core";
import PromptModal from "@/components/PromptModal.vue";
import {
  Heading as UtrechtHeading,
  Button as UtrechtButton,
} from "@utrecht/component-library-vue";
import SimpleSpinner from "@/components/SimpleSpinner.vue";
import { fetchLoggedIn } from "@/services";
import { toast } from "@/stores/toast";
import { useRouter } from "vue-router";

//todo: refactoring, kan hier niet het afdelingensearch component gebruikt worden?
import { useAfdelingen } from "@/components/afdelingenSearch/afdelingen";
import { useGroepen } from "@/components/groepenSearch/groepen";
import ServiceDataSearch from "@/components/ServiceDataSearch.vue";
import { TypeOrganisatorischeEenheid } from "@/features/types";
const router = useRouter();

const props = defineProps<{
  id?: string;
  soort?: TypeOrganisatorischeEenheid;
}>();

type Vraag = {
  id: number;
  questiontype: string;
  description: string;
};

type InputVraag = Vraag;

type TextareaVraag = Vraag;

type DropdownVraag = Vraag & {
  options: string[];
};

type CheckboxVraag = Vraag & {
  options: string[];
};

type ContactverzoekOrganisatorischeEenheid = {
  id: string;
  identificatie: string;
  naam: string;
};

const selectedOrganisatorischeEenheid =
  ref<ContactverzoekOrganisatorischeEenheid>();
const selectedVraag = ref("Vraag toevoegen");
const title = ref("");
const vragen = ref<Vraag[]>([]);

const submit = async () => {
  loading.value = true;
  const generatedSchema = createJsonSchema(vragen.value);
  try {
    const payload = {
      Titel: title.value,
      OrganisatorischeEenheidId: selectedOrganisatorischeEenheid.value?.id,
      OrganisatorischeEenheidNaam: selectedOrganisatorischeEenheid.value?.naam,
      OrganisatorischeEenheidSoort: props.soort,
      JsonVragen: JSON.stringify(generatedSchema, null, 2),
    };

    let result;
    const id = props.id;

    if (id) {
      result = await fetchLoggedIn(`/api/contactverzoekvragensets/${id}`, {
        method: "PUT",
        headers: {
          "Content-Type": "application/json",
        },
        body: JSON.stringify(payload),
      });
    } else {
      result = await fetchLoggedIn("/api/contactverzoekvragensets", {
        method: "POST",
        headers: {
          "Content-Type": "application/json",
        },
        body: JSON.stringify(payload),
      });
    }

    if (!result.ok) {
      handleError();
    } else {
      handleSuccess();
    }
  } catch (error) {
    handleError();
  } finally {
    loading.value = false;
  }
};

onMounted(() => {
  load();
});

function toSchemaFromVraag(
  vraag: InputVraag | TextareaVraag | DropdownVraag | CheckboxVraag,
): Record<string, any> {
  const baseSchema = {
    description: vraag.description,
    questiontype: vraag.questiontype,
    additionalProperties: false,
    type: "string",
  };

  if (vraag.questiontype === "dropdown" || vraag.questiontype === "checkbox") {
    const optionsVraag = vraag as DropdownVraag | CheckboxVraag;
    return {
      ...baseSchema,
      items: { type: "array", options: optionsVraag.options },
    };
  } else {
    return baseSchema;
  }
}

function createJsonSchema(vragen: Vraag[]): Record<string, any> {
  const schema = {
    $schema: "https://json-schema.org/draft-04/schema",
    title: "ContactVerzoekVragenSets",
    references: {},
    type: "object",
    additionalProperties: false,
    properties: {} as Record<string, any>,
  };

  vragen.forEach((vraag) => {
    const descriptionWithoutSpaces = vraag.description.replace(/\s/g, "");
    schema.properties[descriptionWithoutSpaces] = toSchemaFromVraag(vraag);
  });
  return schema;
}

let vraagCounter = 0;
const handleVraagChange = () => {
  vraagCounter++;

  const nieuweVraag: Vraag = {
    id: vraagCounter,
    questiontype: selectedVraag.value,
    description: "",
    ...(selectedVraag.value === "dropdown" || selectedVraag.value === "checkbox"
      ? { options: ["", ""] }
      : {}),
  };

  vragen.value.push(nieuweVraag);
  selectedVraag.value = "Vraag toevoegen";
};

const removeVraag = (id: number) => {
  const index = vragen.value.findIndex((vraag) => vraag.id === id);

  if (index !== -1) {
    vragen.value.splice(index, 1);
    vragen.value.forEach((vraag, i) => {
      vraag.id = i + 1;
    });
  }
  vraagCounter = vragen.value.length;
};

const isInputVraag = (question: Vraag): question is InputVraag =>
  question.questiontype === "input";

const isTextareaVraag = (question: Vraag): question is TextareaVraag =>
  question.questiontype === "textarea";

const isCheckboxVraag = (question: Vraag): question is CheckboxVraag =>
  question.questiontype === "checkbox" && "options" in question;

const isDropdownVraag = (question: Vraag): question is DropdownVraag =>
  question.questiontype === "dropdown" && "options" in question;

const removeOption = (vraagId: number, optionIndex: number) => {
  const vraag = vragen.value.find((v) => v.id === vraagId);

  if (vraag && (isCheckboxVraag(vraag) || isDropdownVraag(vraag))) {
    const newOptions = [...vraag.options];
    newOptions.splice(optionIndex, 1);
    vraag.options = newOptions;
  }
};

const addOption = (vraagId: number) => {
  const vraag = vragen.value.find((v) => v.id === vraagId);

  if (vraag && (isCheckboxVraag(vraag) || isDropdownVraag(vraag))) {
    vraag.options.push("");
  }
};

const loading = ref<boolean>(true);

async function load() {
  loading.value = true;

  try {
    const id = props.id;
    if (id) {
      const response = await fetchLoggedIn(
        `/api/contactverzoekvragensets/${id}`,
      );
      const data = await response.json();

      title.value = data.titel;
      selectedOrganisatorischeEenheid.value = {
        id: data.organisatorischeEenheidId,
        naam: data.organisatorischeEenheidNaam,
        identificatie: data.identificatie,
      };
      vragen.value = ToSchemaFromVragen(JSON.parse(data.jsonVragen));
      vraagCounter = vragen.value.length;
    }
  } catch {
    handleError();
  } finally {
    loading.value = false;
  }
}

function ToSchemaFromVragen(schema: any): Vraag[] {
  const results = [];

  if (schema && schema.properties) {
    let idCounter = 1;

    for (const key in schema.properties) {
      const property = schema.properties[key];
      const questionType = property.questiontype;

      switch (questionType) {
        case "dropdown":
          results.push({
            id: idCounter,
            questiontype: questionType,
            description: property.description,
            options: (property.items && property.items.options) || [],
          } as DropdownVraag);
          break;

        case "checkbox":
          results.push({
            id: idCounter,
            questiontype: questionType,
            description: property.description,
            options: (property.items && property.items.options) || [],
          } as CheckboxVraag);
          break;

        default:
          results.push({
            id: idCounter,
            questiontype: questionType,
            description: property.description,
          } as Vraag);
      }
      idCounter++;
    }
  }

  return results;
}

const handleError = () => {
  toast({
    text: "Er is een fout opgetreden. Probeer het later opnieuw.",
    type: "error",
  });
};

const handleSuccess = () => {
  toast({
    text: "Contactverzoek formulier opgeslagen",
  });

  navigateToContactverzoekformulieren();
};

const setVraagTypeDescription = (type: string) => {
  switch (type) {
    case "input":
      return "Open vraag kort";
    case "textarea":
      return "Open vraag lang";
    case "dropdown":
      return "Dropdown";
    case "checkbox":
      return "Checkbox";
    default:
      return "";
  }
};

const navigateToContactverzoekformulieren = () => {
  router.push(`/Beheer/formulieren-contactverzoek-${props.soort}/`);
};

const revealCancelDialog = () => {
  if (vragen.value.length > 0) {
    cancelDialog.reveal();
  } else {
    navigateToContactverzoekformulieren();
  }
};

const cancelDialog = useConfirmDialog();
cancelDialog.onConfirm(() => {
  navigateToContactverzoekformulieren();
});

const optionHoverId = ref(-1);
const optionHoverIndex = ref(-1);

const showOptionTrashButton = (id: number, index: number) => {
  optionHoverId.value = id;
  optionHoverIndex.value = index;
};

const hideOptionTrashButton = (id: number, index: number) => {
  if (optionHoverIndex.value === index && optionHoverId.value === id) {
    optionHoverId.value = -1;
    optionHoverIndex.value = -1;
  }
};

const hoverId = ref(-1);
const hoverIndex = ref(-1);

const showQuestionTrashButton = (id: number, index: number) => {
  hoverId.value = id;
  hoverIndex.value = index;
};

const hideQuestionTrashButton = (id: number, index: number) => {
  if (hoverIndex.value === index && hoverId.value === id) {
    hoverId.value = -1;
    hoverIndex.value = -1;
  }
};
</script>

<style lang="scss" scoped>
form {
  margin-top: var(--spacing-default);
}

label > span {
  display: block;
}

.question-container {
  flex-wrap: wrap;
  display: flex;
  align-items: center;
  justify-content: space-between;
}

.container {
  display: flex;
  flex-direction: column;
  gap: var(--spacing-default);
}

menu {
  margin-top: var(--spacing-large);
  display: flex;
  gap: var(--spacing-default);
  justify-content: flex-end;
}

.question-container-button {
  width: 65%;
  margin-top: 0.5rem;
}

.options-wrapper {
  width: 94%;
  margin-bottom: 0.5rem;
}

.option-add-wrapper {
  width: 100%;
  text-align: right;
}

.option-add-button {
  text-align: right;
}

.question-wrapper {
  width: 106%;
  display: flex;
  align-items: center;
  position: relative;
}

.question-inputfield-wrapper {
  width: 94%;
  margin-bottom: 0.5rem;
}

.question-delete-button {
  position: absolute;
  right: 0;
  cursor: pointer;
}

.questions {
  width: 100%;
}

pre {
  white-space: pre-line;
}
</style>
