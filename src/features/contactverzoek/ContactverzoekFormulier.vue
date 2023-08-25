<template>
  <SimpleSpinner v-if="afdelingen.loading" />
  <div class="container" @submit.prevent>
    <!-- <label
      class="utrecht-form-label"
      v-if="afdelingen.success && afdelingen.data.length"
    >
      Afdeling
      <select
        v-model="form.afdeling"
        @change="setActive"
        name="afdeling"
        class="utrecht-select utrecht-select--html-select"
      >
        <option
          v-for="afdeling in afdelingen.data"
          :key="afdeling.id"
          :value="afdeling.name"
        >
          {{ afdeling.name }}
        </option>
      </select>
    </label> -->

    <label class="utrecht-form-label">
      <span class="required">Contactverzoek versturen naar</span>
      <medewerker-search
        class="utrecht-textbox utrecht-textbox--html-input"
        required
        v-model="form.medewerker"
        :defaultValue="form.medewerker"
        @update:model-value="setActive"
      />
    </label>

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
          <template v-if="form.medewerker">
            <label class="utrecht-form-label">
              <span> VragenSets </span>
              <select
                class="utrecht-select utrecht-select--html-select"
                name="VragenSets"
                v-model="form.vragenSetId"
                @input="setActive"
              >
                <option
                  v-for="item in data || []"
                  :key="item.id"
                  :value="item.id"
                >
                  {{ item.naam }}
                </option>
              </select>
            </label>

            <template v-if="form.contactVerzoekVragenSet">
              <template
                v-for="(item, index) in form.contactVerzoekVragenSet
                  .vraagAntwoord"
                :key="index"
              >
                <label class="utrecht-form-label">
                  <span> {{ item.vraag }} </span>
                  <input
                    class="utrecht-textbox utrecht-textbox--html-input"
                    type="text"
                    v-model="item.antwoord"
                    @input="setActive"
                  />
                </label>
              </template>
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
import { useAfdelingen, useVragenSets } from "./service";
import SimpleSpinner from "@/components/SimpleSpinner.vue";
import type { ContactmomentContactVerzoek } from "@/stores/contactmoment";
import { ref, watch } from "vue";
import {
  FormFieldsetLegend,
  FormFieldset,
} from "@utrecht/component-library-vue";
import ServiceDataWrapper from "@/components/ServiceDataWrapper.vue";

const props = defineProps<{
  modelValue: ContactmomentContactVerzoek;
}>();

const form = ref<Partial<ContactmomentContactVerzoek>>({});

watch(
  () => props.modelValue,
  (v) => {
    form.value = v;
  },
  { immediate: true }
);

const afdelingen = useAfdelingen();
const vragenSets = useVragenSets();

watch(
  () => form.value.vragenSetId,
  (vragenSetId) => {
    if (!vragenSets.success) return;
    const vragenSet = vragenSets.data.find((s) => s.id == vragenSetId);
    form.value.contactVerzoekVragenSet = vragenSet;
  }
);

const setActive = () => {
  form.value.isActive = true;
};

const telEl = ref<HTMLInputElement>();

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
      hasContact ? "" : "Vul minimaal een telefoonnummer of een e-mailadres in"
    );
  }
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
</style>
