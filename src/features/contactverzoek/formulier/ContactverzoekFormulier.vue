<template>
  <div class="container" @submit.prevent>
    <form-fieldset class="radio-group">
      <form-fieldset-legend class="required"
        >Contactverzoek maken voor</form-fieldset-legend
      >
      <label>
        <input
          type="radio"
          :value="undefined"
          class="utrecht-radio-button utrecht-radio-button--html-input"
          v-model="form.isMedewerker"
        />
        Afdeling
      </label>
      <label>
        <input
          type="radio"
          :value="true"
          class="utrecht-radio-button utrecht-radio-button--html-input"
          v-model="form.isMedewerker"
        />
        Medewerker
      </label>
    </form-fieldset>

    <label class="utrecht-form-label" v-if="form.isMedewerker">
      <span class="required">Contactverzoek versturen naar</span>
      <medewerker-search
        class="utrecht-textbox utrecht-textbox--html-input"
        required
        v-model="form.medewerker"
        @update:model-value="setActive"
      />
    </label>

    <template v-else>
      <label class="utrecht-form-label">
        <span class="required">Afdeling</span>
        <service-data-search
          class="utrecht-textbox utrecht-textbox--html-input"
          :required="true"
          v-model="form.afdeling"
          placeholder="Zoek een afdeling"
          @update:model-value="onUpdateAfdeling"
          :get-data="useAfdelingen"
          :map-value="(x) => x?.naam"
          @keydown.enter="setEnterPressed"
          :map-description="(x) => x?.identificatie"
        />
      </label>

      <service-data-wrapper :data="groepenFirstPage">
        <template #init>
          <label class="disabled utrecht-form-label">
            Groep
            <input
              type="text"
              class="utrecht-textbox utrecht-textbox--html-input"
              disabled
              placeholder="Kies eerst een afdeling"
            />
          </label>
        </template>
        <template #success="{ data }">
          <label :class="['utrecht-form-label', { disabled: !data.count }]">
            Groep
            <service-data-search
              class="utrecht-textbox utrecht-textbox--html-input"
              v-model="form.groep"
              :placeholder="
                !data.count ? 'Geen groepen gevonden' : 'Zoek een groep'
              "
              @update:model-value="setActive"
              :get-data="(x) => useGroepen(() => form.afdeling?.id, x)"
              :map-value="(x) => x?.naam"
              :map-description="(x) => x?.identificatie"
              ref="groepSearchRef"
              :disabled="!data.count"
            />
          </label>
        </template>
      </service-data-wrapper>
    </template>

    <label class="utrecht-form-label notitieveld">
      <span class="required">Interne toelichting voor medewerker</span>
      <textarea
        v-model="form.interneToelichting"
        name="Interne toelichting"
        required
        class="utrecht-textarea utrecht-textarea--html-textarea"
        rows="5"
        @input="setActive"
      />
    </label>

    <form-fieldset>
      <service-data-wrapper :data="vragenSets" class="container">
        <template #success="{ data }">
          <!-- Dropdown for selecting Onderwerp -->
          <contactverzoek-onderwerpen
            :vragenSets="data"
            :afdelingId="form?.afdeling?.id"
            :prefill="!form.vragenSetChanged"
            v-model:modelValue="form.contactVerzoekVragenSet"
            @change="form.vragenSetChanged = true"
          />

          <!-- Dynamic fields based on selected Onderwerp -->
          <template v-if="form.contactVerzoekVragenSet">
            <template
              v-for="(item, index) in form.contactVerzoekVragenSet
                .vraagAntwoord"
              :key="index"
            >
              <label class="utrecht-form-label">
                <span>{{ item.description }}</span>
                <input
                  v-if="isInputVraag(item)"
                  class="utrecht-textbox utrecht-textbox--html-input"
                  type="text"
                  v-model="item.input"
                  @input="setActive"
                />
                <textarea
                  v-if="isTextareaVraag(item)"
                  class="utrecht-textarea"
                  v-model="item.textarea"
                  @input="setActive"
                ></textarea>
                <select
                  v-if="isDropdownVraag(item)"
                  class="utrecht-select"
                  v-model="item.selectedDropdown"
                  @input="setActive"
                >
                  <option v-for="option in item.options" :key="option">
                    {{ option }}
                  </option>
                </select>
                <div v-if="isCheckboxVraag(item)">
                  <label
                    class="utrecht-checkbox-button"
                    v-for="(option, optionIndex) in item.options"
                    :key="option"
                  >
                    <input
                      class="utrecht-checkbox-button"
                      type="checkbox"
                      :value="option"
                      v-model="item.selectedCheckbox[optionIndex]"
                    />
                    {{ option }}
                  </label>
                </div>
              </label>
            </template>
          </template>
        </template>
      </service-data-wrapper>
    </form-fieldset>

    <form-fieldset>
      <form-fieldset-legend>Contact opnemen met</form-fieldset-legend>
      <label class="utrecht-form-label">
        <span>Voornaam</span>
        <input
          v-model="form.voornaam"
          type="tel"
          name="Naam"
          class="utrecht-textbox utrecht-textbox--html-input"
          @input="setActive"
        />
      </label>
      <label class="utrecht-form-label">
        <span>Tussenvoegsel</span>
        <input
          v-model="form.voorvoegselAchternaam"
          type="tel"
          name="Naam"
          class="utrecht-textbox utrecht-textbox--html-input"
          @input="setActive"
        />
      </label>
      <label class="utrecht-form-label">
        <span>Achternaam</span>
        <input
          v-model="form.achternaam"
          type="tel"
          name="Naam"
          class="utrecht-textbox utrecht-textbox--html-input"
          @input="setActive"
        />
      </label>
      <label class="utrecht-form-label">
        <span>Organisatie</span>
        <input
          v-model="form.organisatie"
          type="tel"
          name="Naam"
          class="utrecht-textbox utrecht-textbox--html-input"
          @input="setActive"
        />
      </label>
      <label class="utrecht-form-label">
        <span>Telefoonnummer 1</span>
        <input
          ref="telEl"
          v-model="form.telefoonnummer1"
          type="tel"
          name="Telefoonnummer 1"
          class="utrecht-textbox utrecht-textbox--html-input"
          @input="setActive"
        />
      </label>
      <label class="utrecht-form-label">
        <span>Telefoonnummer 2</span>
        <input
          v-model="form.telefoonnummer2"
          type="tel"
          name="Telefoonnummer 2"
          class="utrecht-textbox utrecht-textbox--html-input"
          @input="setActive"
        />
      </label>
      <label class="utrecht-form-label">
        <span>Omschrijving telefoonnummer 2</span>
        <input
          v-model="form.omschrijvingTelefoonnummer2"
          name="Omschrijving telefoonnummer 2"
          class="utrecht-textbox utrecht-textbox--html-input"
          @input="setActive"
        />
      </label>
      <label class="utrecht-form-label">
        <span>E-mailadres</span>
        <input
          v-model="form.emailadres"
          type="email"
          name="E-mailadres"
          class="utrecht-textbox utrecht-textbox--html-input"
          @input="setActive"
        />
      </label>
    </form-fieldset>
  </div>
</template>

<script lang="ts">
export default {
  inheritAttrs: false,
};
</script>

<script lang="ts" setup>
import MedewerkerSearch from "@/features/search/MedewerkerSearch.vue";
import type { ContactmomentContactVerzoek } from "@/stores/contactmoment";
import { ref } from "vue";
import { watch } from "vue";
import {
  FormFieldsetLegend,
  FormFieldset,
} from "@utrecht/component-library-vue";

import ServiceDataWrapper from "@/components/ServiceDataWrapper.vue";
import ServiceDataSearch from "@/components/ServiceDataSearch.vue";
import { whenever } from "@vueuse/core";
import { nextTick } from "vue";
import {
  useVragenSets,
  useGroepen,
  isInputVraag,
  isTextareaVraag,
  isDropdownVraag,
  isCheckboxVraag,
} from "./service";

import { useAfdelingen } from "@/composables/afdelingen";
import { computed } from "vue";
import ContactverzoekOnderwerpen from "./ContactverzoekOnderwerpen.vue";

const props = defineProps<{
  modelValue: ContactmomentContactVerzoek;
}>();

const form = ref<Partial<ContactmomentContactVerzoek>>({});

watch(
  () => props.modelValue,
  (v) => {
    form.value = v;
  },
  { immediate: true },
);

watch(
  () => form.value.afdeling,
  () => {
    setOnderwerp();
  },
);

const setActive = () => {
  form.value.isActive = true;
};

const onUpdateAfdeling = () => {
  form.value.contactVerzoekVragenSet = undefined;
  form.value.vragenSetChanged = false;
  setActive();
};

const telEl = ref<HTMLInputElement>();
const vragenSets = useVragenSets();

const setOnderwerp = () => {
  setActive();
};

const groepenFirstPage = useGroepen(() => form.value.afdeling?.id);

const groepSearchRef = ref();

const enterPressed = ref(false);
const setEnterPressed = () => {
  enterPressed.value = true;
};

// focus groep search element whenever it appears on the page (so when you select a Afdeling that has Groepen)
whenever(groepSearchRef, (v) => {
  if (!enterPressed.value) return;
  enterPressed.value = false;
  nextTick(() => {
    (v.$el as HTMLElement)?.getElementsByTagName("input")?.[0]?.focus();
  });
});

watch(
  [
    telEl,
    () =>
      !!form.value.telefoonnummer1 ||
      !!form.value.telefoonnummer2 ||
      !!form.value.emailadres,
  ],
  ([el, hasContact]) => {
    if (!el) return;
    el.setCustomValidity(
      hasContact ? "" : "Vul minimaal een telefoonnummer of een e-mailadres in",
    );
  },
);

watch(
  () => form.value.afdeling,
  () => {
    form.value.groep = undefined;
  },
);

const afdelingId = computed(() => {
  const afdelingen = props.modelValue.medewerker?.afdelingen;
  if (Array.isArray(afdelingen) && afdelingen.length > 0) {
    return afdelingen[0].afdelingId;
  }
  return null;
});

watch(
  () => form.value.medewerker,
  () => {
    form.value.afdeling = {
      id: afdelingId.value ?? "",
      identificatie: "",
      naam: "",
    };
    setOnderwerp();
  },
);
</script>

<style lang="scss" scoped>
.container {
  display: flex;
  flex-direction: column;
  gap: var(--spacing-default);
}

textarea {
  resize: none;
}

fieldset {
  display: grid;
  gap: var(--spacing-extrasmall);
}

.radio-group > legend {
  font-size: inherit;
}

.utrecht-checkbox-button {
  display: flex !important;
}
</style>
