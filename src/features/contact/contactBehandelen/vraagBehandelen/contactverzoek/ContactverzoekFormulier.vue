<template>
  <simple-spinner v-if="loading" />

  <div v-else class="container" @submit.prevent>
    <form-fieldset class="radio-group">
      <form-fieldset-legend class="required"
        >Contactverzoek maken voor</form-fieldset-legend
      >

      <label>
        <input
          type="radio"
          :value="ActorType.afdeling"
          class="utrecht-radio-button utrecht-radio-button--html-input"
          v-model="form.typeActor"
          @change="onUpdateActorAfdelingOrGroep"
        />
        Afdeling
      </label>

      <label>
        <input
          type="radio"
          :value="ActorType.groep"
          class="utrecht-radio-button utrecht-radio-button--html-input"
          v-model="form.typeActor"
          @change="onUpdateActorAfdelingOrGroep"
        />
        Groep
      </label>

      <label v-if="!useMedewerkeremail">
        <input
          type="radio"
          :value="ActorType.medewerker"
          class="utrecht-radio-button utrecht-radio-button--html-input"
          v-model="form.typeActor"
          @change="onUpdateActorAfdelingOrGroep"
        />
        Medewerker
      </label>
    </form-fieldset>

    <div
      v-if="form.typeActor === ActorType.afdeling"
      class="utrecht-form-label"
    >
      <label for="afdelingen-search" class="required">Afdeling</label>
      <afdelingen-search
        id="afdelingen-search"
        :model-value="form.afdeling"
        @update:model-value="onUpdateAfdeling"
        :exact-match="true"
        class="utrecht-textbox utrecht-textbox--html-input"
        :required="true"
        placeholder="Zoek een afdeling"
      />
    </div>

    <div v-if="form.typeActor === ActorType.groep" class="utrecht-form-label">
      <label for="groepen-search" class="required">Groep</label>
      <groepen-search
        id="groepen-search"
        :model-value="form.groep"
        @update:model-value="onUpdateGroep"
        :exact-match="true"
        class="utrecht-textbox utrecht-textbox--html-input"
        :required="true"
        placeholder="Zoek een groep"
      />
    </div>

    <label v-if="useMedewerkeremail" class="utrecht-form-label">
      <span class="">E-mailadres medewerker</span>

      <input
        v-model="form.medewerkeremail"
        v-validity-handler="validateEmailInput"
        name="E-mailadres medewerker"
        class="utrecht-textbox utrecht-textbox--html-input"
        @input="setActive"
      />
    </label>

    <div
      v-else
      :class="[
        'utrecht-form-label',
        {
          disabled:
            (form.typeActor == ActorType.afdeling && !form.afdeling?.id) ||
            (form.typeActor == ActorType.groep && !form.groep?.id),
        },
      ]"
    >
      <label for="medewerker-search">
        Medewerker
        <span v-if="form.typeActor === ActorType.medewerker" class="required">
        </span>
      </label>
      <medewerker-search
        id="medewerker-search"
        class="utrecht-textbox utrecht-textbox--html-input"
        :model-value="form.medewerker"
        @update:model-value="onUpdateMedewerker"
        :filter-field="
          form.typeActor == ActorType.afdeling
            ? 'Smoelenboek.afdelingen.afdelingnaam'
            : form.typeActor === ActorType.groep
              ? 'Smoelenboek.groepen.groepsnaam'
              : ''
        "
        :filter-value="
          form.typeActor === ActorType.afdeling
            ? form.afdeling?.naam
            : form.groep?.naam
        "
        :required="form.typeActor === ActorType.medewerker"
        :isDisabled="
          (form.typeActor == ActorType.afdeling && !form.afdeling?.id) ||
          (form.typeActor == ActorType.groep && !form.groep?.id)
        "
        :placeholder="
          form.typeActor === ActorType.medewerker
            ? 'Zoek een medewerker'
            : 'Kies eerst een afdeling of groep'
        "
      />
    </div>

    <label
      v-if="form.typeActor === ActorType.medewerker && form.medewerker"
      for="groep"
      class="utrecht-form-label"
    >
      <span class="required">Afdeling / groep </span>
      <select
        id="groep"
        class="utrecht-textbox utrecht-textbox--html-input"
        v-model="form.organisatorischeEenheidVanMedewerker"
        required="true"
      >
        <option v-for="item in afdelingenGroepen" :value="item" :key="item.id">
          {{ item.naam }}
        </option>
      </select>
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
      ></textarea>
    </label>

    <form-fieldset>
      <div class="container">
        <!-- Dropdown for selecting Onderwerp -->
        <contactverzoek-onderwerpen
          :organisatorischeEenheidId="soort && form[soort]?.id"
          :organisatorischeEenheidSoort="soort"
          v-model:vragenSets="vragenSets"
          v-model:vragenSetIdMap="vragenSetIdMap"
          v-model:contactVerzoekVragenSet="form.contactVerzoekVragenSet"
        />

        <!-- Dynamic fields based on selected Onderwerp -->
        <template v-if="form.contactVerzoekVragenSet">
          <template
            v-for="(item, index) in form.contactVerzoekVragenSet.vraagAntwoord"
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
      </div>
    </form-fieldset>

    <form-fieldset>
      <div class="container">
        <form-fieldset-legend>Contact opnemen met</form-fieldset-legend>
        <label class="utrecht-form-label">
          <span>Voornaam</span>
          <input
            v-model="form.voornaam"
            name="Voornaam"
            class="utrecht-textbox utrecht-textbox--html-input"
            @input="setActive"
          />
        </label>
        <label class="utrecht-form-label">
          <span>Tussenvoegsel</span>
          <input
            v-model="form.voorvoegselAchternaam"
            name="Tussenvoegsel"
            class="utrecht-textbox utrecht-textbox--html-input"
            @input="setActive"
          />
        </label>
        <label class="utrecht-form-label">
          <span>Achternaam</span>
          <input
            v-model="form.achternaam"
            name="Achternaam"
            class="utrecht-textbox utrecht-textbox--html-input"
            @input="setActive"
          />
        </label>
        <label class="utrecht-form-label">
          <span>Organisatie</span>
          <input
            v-model="form.organisatie"
            name="Organisatie"
            class="utrecht-textbox utrecht-textbox--html-input"
            @input="setActive"
          />
        </label>
        <label class="utrecht-form-label">
          <span>Telefoonnummer 1</span>
          <input
            ref="telEl"
            v-model="form.telefoonnummer1"
            v-validity-handler="validateTelefoonInput"
            name="Telefoonnummer 1"
            class="utrecht-textbox utrecht-textbox--html-input"
            @input="setActive"
          />
        </label>
        <label class="utrecht-form-label">
          <span>Telefoonnummer 2</span>
          <input
            v-model="form.telefoonnummer2"
            v-validity-handler="validateTelefoonInput"
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
            v-validity-handler="validateEmailInput"
            name="E-mailadres"
            class="utrecht-textbox utrecht-textbox--html-input"
            @input="setActive"
          />
        </label>
      </div>
    </form-fieldset>
  </div>
</template>

<script lang="ts">
export default {
  inheritAttrs: false,
};
</script>

<script lang="ts" setup>
import SimpleSpinner from "@/components/SimpleSpinner.vue";
import MedewerkerSearch from "./components/MedewerkerSearch.vue";
import type {
  ContactmomentContactVerzoek,
  ContactVerzoekMedewerker,
} from "@/stores/contactmoment/index";
import { ActorType } from "@/stores/contactmoment/index";
import { computed, ref, useModel, watch, type Directive, nextTick } from "vue";
import {
  FormFieldsetLegend,
  FormFieldset,
} from "@utrecht/component-library-vue";
import {
  isInputVraag,
  isTextareaVraag,
  isDropdownVraag,
  isCheckboxVraag,
} from "@/features/contact/service";
import ContactverzoekOnderwerpen from "./components/ContactverzoekOnderwerpen.vue";
import AfdelingenSearch from "@/components/afdelingenSearch/AfdelingenSearch.vue";
import GroepenSearch from "@/components/groepenSearch/GroepenSearch.vue";
import {
  fetchAfdelingen,
  type Afdeling,
} from "@/components/afdelingenSearch/afdelingen";
import { fetchGroepen, type Groep } from "@/components/groepenSearch/groepen";
import { TELEFOON_PATTERN, EMAIL_PATTERN } from "@/helpers/validation";

import { fetchLoggedIn, useLoader } from "@/services";
import { TypeOrganisatorischeEenheid } from "@/features/types";

const props = defineProps<{ modelValue: ContactmomentContactVerzoek }>();
const model = useModel(props, "modelValue");

const useModelProperty = <K extends keyof ContactmomentContactVerzoek>(
  key: K,
) =>
  computed({
    get: () => model.value[key],
    set: (v) => {
      model.value = { ...props.modelValue, [key]: v };
    },
  });

const vragenSets = useModelProperty("vragenSets");
const vragenSetIdMap = useModelProperty("vragenSetIdMap");

const form = ref<Partial<ContactmomentContactVerzoek>>({});

// cast to TypeOrganisatorischeEenheid
const soort = computed(() =>
  form.value.typeActor === ActorType.afdeling ||
  form.value.typeActor === ActorType.groep
    ? Object.values(TypeOrganisatorischeEenheid)[form.value.typeActor]
    : undefined,
);

// update het formulier als er tussen vragen/contactmomenten/afhandelscherm geswitched wordt
watch(
  () => props.modelValue,
  (v) => (form.value = v),
  { immediate: true },
);

const setActive = () => (form.value.isActive = true);

// i.v.m. voorinvullen medewerker/typeActor is activeren vh formulier losgekoppeld van model
// afdeling en groep volgen zelfde patroon maar worden (vooralsnog) niet vooringevuld
const onUpdateAfdeling = (afdeling?: Afdeling) => {
  form.value.afdeling = afdeling;

  onUpdateActorAfdelingOrGroep();
};

const onUpdateGroep = (groep?: Groep) => {
  form.value.groep = groep;

  onUpdateActorAfdelingOrGroep();
};

const onUpdateMedewerker = (medewerker?: ContactVerzoekMedewerker) => {
  form.value.medewerker = medewerker;

  setActive();
};

const onUpdateActorAfdelingOrGroep = () => {
  form.value.medewerker = undefined;

  setActive();
};

const { data: useMedewerkeremail, loading } = useLoader(() =>
  fetchLoggedIn("/api/environment/use-medewerkeremail")
    .then((r) => r.json())
    .then(({ useMedewerkeremail }) => useMedewerkeremail),
);

const telEl = ref<HTMLInputElement>();

/////////////////////////////////////////////////////////

const afdelingenGroepen = ref();

const refreshAllAfdelingen = async () => {
  const organisatorischeEenheid = await fetchAfdelingen(undefined, false);
  if (organisatorischeEenheid.page) {
    const items = organisatorischeEenheid.page.map(
      (item: { id: any; identificatie: any; naam: string }) => ({
        id: item.id,
        identificatie: item.identificatie,
        naam: "Afdeling: " + item.naam,
      }),
    );
    afdelingenGroepen.value = afdelingenGroepen.value.concat(items);
  }
};

const refreshAllGroepen = async () => {
  const organisatorischeEenheid = await fetchGroepen(undefined, false);
  if (organisatorischeEenheid.page) {
    const items = organisatorischeEenheid.page.map((item) => ({
      id: item.id,
      identificatie: item.identificatie,
      naam: "Groep: " + item.naam,
    }));
    afdelingenGroepen.value = afdelingenGroepen.value.concat(items);
  }
};

const refreshAfdelingen = async (namen: string[]) => {
  for (const naam of namen) {
    const organisatorischeEenheid = await fetchAfdelingen(naam, false);

    if (organisatorischeEenheid.page) {
      const items = organisatorischeEenheid.page
        .filter((x: { naam: string }) => x.naam === naam)
        .map((item: { id: any; identificatie: any; naam: string }) => ({
          id: item.id,
          identificatie: item.identificatie,
          naam: "Afdeling: " + item.naam,
        }));

      afdelingenGroepen.value = afdelingenGroepen.value.concat(items);
    }
  }
};

const refreshGroepen = async (namen: string[]) => {
  for (const naam of namen) {
    const organisatorischeEenheid = await fetchGroepen(naam, false);

    if (organisatorischeEenheid.page) {
      const items = organisatorischeEenheid.page
        .filter((x) => x.naam === naam)
        .map((item) => ({
          id: item.id,
          identificatie: item.identificatie,
          naam: "Groep: " + item.naam,
        }));

      afdelingenGroepen.value = afdelingenGroepen.value.concat(items);
    }
  }
};

watch(
  [
    () => form.value.medewerker?.afdelingen,
    () => form.value.medewerker?.groepen,
  ],
  async () => {
    //als er een andere medewerker geselecteerd wordt en zodoende de lijsten met
    //afdelingen of groepen van de geselecteerde medewerker wijzigen,
    afdelingenGroepen.value = [];

    //als er geen afdelingen en geen groepen zijn, toon dan alle afdelingen en groepen
    //de koppeling is niet perfect, dan kan de kcm zelf een keuze maken uit de complete lijst
    if (
      !form.value.medewerker?.afdelingen?.length &&
      !form.value.medewerker?.groepen?.length
    ) {
      await refreshAllAfdelingen();
      await refreshAllGroepen();
      return;
    }

    if (form.value.medewerker?.afdelingen) {
      await refreshAfdelingen(
        form.value.medewerker?.afdelingen.map(
          (afdeling) => afdeling.afdelingnaam,
        ) || [],
      );
    }

    if (form.value.medewerker?.groepen) {
      await refreshGroepen(
        form.value.medewerker?.groepen?.map((groep) => groep.groepsnaam) || [],
      );
    }
  },
  { immediate: true },
);

//////////////////////////////////////////////////////

const hasContact = computed(
  () =>
    !!form.value.telefoonnummer1 ||
    !!form.value.telefoonnummer2 ||
    !!form.value.emailadres,
);

const noContactMessage =
  "Vul minimaal een telefoonnummer of een e-mailadres in";

watch(
  [telEl, hasContact],
  ([el, bool]) => el && el.setCustomValidity(!bool ? noContactMessage : ""),
);

const validateTelefoonInput = (el: HTMLInputElement) => {
  if (!el.value || TELEFOON_PATTERN.test(el.value)) {
    // telEl: back to custom noContactMessage if applicable, otherwise clear
    return el === telEl.value && !hasContact.value ? noContactMessage : "";
  } else {
    return "Vul een geldig telefoonnummer in.";
  }
};

const validateEmailInput = (el: HTMLInputElement) =>
  !el.value || EMAIL_PATTERN.test(el.value)
    ? ""
    : "Vul een geldig e-mailadres in.";

const vValidityHandler: Directive<
  HTMLInputElement & { onInputHandler?: EventListener },
  (el: HTMLInputElement) => string
> = {
  mounted(el, binding) {
    const setCustomValidity = () => el.setCustomValidity(binding.value(el));

    // wait for the element to be in the DOM
    nextTick(setCustomValidity); // onmounted

    el.addEventListener("input", setCustomValidity); // oninput
    el.onInputHandler = setCustomValidity;
  },
  unmounted(el) {
    if (el.onInputHandler) {
      el.removeEventListener("input", el.onInputHandler);
      delete el.onInputHandler;
    }
  },
};
</script>

<style lang="scss" scoped>
.container {
  display: flex;
  flex-direction: column;
  gap: var(--spacing-small);
}

textarea {
  resize: none;
}

fieldset {
  display: grid;
  gap: var(--spacing-extrasmall);
}

.radio-group {
  > legend {
    font-size: inherit;
  }

  // styling with only two radios, override space-between
  &:has(> label:nth-of-type(2):last-of-type) {
    justify-content: flex-start;
    gap: var(--spacing-default);
  }
}

.utrecht-checkbox-button {
  display: flex !important;
}
</style>
