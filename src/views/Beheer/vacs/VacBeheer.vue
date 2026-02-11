<template>
  <utrecht-heading :level="1"
    >VAC {{ props.uuid ? "bewerken" : "toevoegen" }}</utrecht-heading
  >

  <simple-spinner v-if="loading" />

  <p v-else-if="error">Er is een fout opgetreden bij het ophalen van de VAC.</p>

  <beheer-form @submit="submit" v-else>
    <template #formFields>
      <label class="utrecht-form-label"
        ><span>Vraag *</span>

        <input
          class="utrecht-textbox utrecht-textbox--html-input"
          type="text"
          v-model="vac.record.data.vraag"
          required
      /></label>

      <label class="utrecht-form-label" for="antwoord">Antwoord *</label>
      <ck-editor v-model="vac.record.data.antwoord" required />

      <label class="utrecht-form-label" for="toelichting">Toelichting</label>
      <ck-editor v-model="vac.record.data.toelichting" />

      <fieldset>
        <legend>Afdelingen</legend>

        <ul>
          <li
            v-for="(value, key) in vac.record.data.afdelingen"
            :key="`afdeling_${key}`"
          >
            <label class="utrecht-form-label">
              <input
                class="utrecht-textbox utrecht-textbox--html-input"
                type="text"
                v-model="value.afdelingNaam"
                :aria-label="`Afdeling ${key + 1}`"
            /></label>
          </li>
        </ul>
      </fieldset>

      <fieldset>
        <legend>Trefwoorden</legend>

        <ul>
          <li
            v-for="(value, key) in vac.record.data.trefwoorden"
            :key="`trefwoord_${key}`"
          >
            <label class="utrecht-form-label">
              <input
                class="utrecht-textbox utrecht-textbox--html-input"
                type="text"
                v-model="value.trefwoord"
                :aria-label="`Trefwoord ${key + 1}`"
            /></label>
          </li>
        </ul>
      </fieldset>

      <p>Velden met (*) zijn verplicht</p>
    </template>

    <template #formMenuListItems>
      <li>
        <router-link
          to="/Beheer/vacs/"
          class="utrecht-button utrecht-button--secondary-action"
        >
          Annuleren
        </router-link>
      </li>

      <li>
        <utrecht-button appearance="primary-action-button" type="submit">
          Opslaan
        </utrecht-button>
      </li>
    </template>
  </beheer-form>
</template>

<script setup lang="ts">
import { onMounted, ref, watch } from "vue";
import { useRouter } from "vue-router";
import {
  Heading as UtrechtHeading,
  Button as UtrechtButton,
} from "@utrecht/component-library-vue";
import SimpleSpinner from "@/components/SimpleSpinner.vue";
import CkEditor from "@/components/ckeditor";
import BeheerForm from "@/components/beheer/BeheerForm.vue";
import { fetchLoggedIn, parseJson, throwIfNotOk } from "@/services";
import type { Vac } from "@/features/informatie/search/types";
import { toast } from "@/stores/toast";

type VacObject = {
  record: {
    startAt: string;
    data: Vac;
    index?: number;
    correctionFor?: number;
  };
};

const vacObjectenUrl = "/api/vacs/api/v2/objects";

const props = defineProps<{ uuid?: string }>();
const router = useRouter();

const loading = ref(false);
const error = ref(false);

const afdelingen = Array.from({ length: 5 }, () => ({ afdelingNaam: "" }));
const trefwoorden = Array.from({ length: 10 }, () => ({ trefwoord: "" }));

const vac = ref<VacObject>({
  record: {
    startAt: new Date().toISOString().substring(0, 10),
    data: {
      vraag: "",
      antwoord: "",
      afdelingen,
      toelichting: "",
      trefwoorden,
    },
  },
});

// Update with fetched data while remaining array length
watch(
  () => ({
    afdelingData: vac.value.record.data.afdelingen,
    trefwoordData: vac.value.record.data.trefwoorden,
  }),
  ({ afdelingData, trefwoordData }) => {
    vac.value.record.data.afdelingen = [
      ...(afdelingData || []),
      ...afdelingen,
    ].slice(0, afdelingen.length);

    vac.value.record.data.trefwoorden = [
      ...(trefwoordData || []),
      ...trefwoorden,
    ].slice(0, trefwoorden.length);
  },
  { once: true },
);

const showError = () => {
  toast({
    text: "Er is een fout opgetreden bij het opslaan van de Vac. Probeer het later opnieuw.",
    type: "error",
  });
};

const handleSuccess = () => {
  toast({
    text: "De Vac is opgeslagen.",
  });

  router.push("/Beheer/vacs");
};

const submit = async () => {
  loading.value = true;

  const createPayload = () => {
    const body = {
      ...vac.value,
      ...{
        record: {
          ...vac.value.record,
          data: {
            ...vac.value.record.data,
            afdelingen: vac.value.record.data.afdelingen?.filter(
              (afdeling) => afdeling.afdelingNaam.trim().length,
            ),
            trefwoorden: vac.value.record.data.trefwoorden?.filter(
              (trefwoord) => trefwoord.trefwoord.trim().length,
            ),
            status: "actief",
            doelgroep: "eu-burger",
          },
          correctionFor: props.uuid ? vac.value.record.index : undefined,
        },
      },
    };

    return JSON.stringify(body);
  };

  const result = await fetchLoggedIn(
    `${vacObjectenUrl}/${props.uuid ? props.uuid : ""}`,
    {
      method: props.uuid ? "PUT" : "POST",
      headers: {
        "Content-Type": "application/json",
      },
      body: createPayload(),
    },
  ).finally(() => (loading.value = false));

  result.ok ? handleSuccess() : showError();
};

onMounted(() => {
  if (!props.uuid) return;

  loading.value = true;

  fetchLoggedIn(`${vacObjectenUrl}/${props.uuid}`)
    .then(throwIfNotOk)
    .then(parseJson)
    .then((result: VacObject) => (vac.value = result))
    .catch(() => (error.value = true))
    .finally(() => (loading.value = false));
});
</script>

<style lang="scss" scoped>
fieldset {
  legend {
    color: var(--utrecht-form-label-color);
    font-size: var(--utrecht-form-label-font-size);
    font-weight: var(--utrecht-form-label-font-weight);
    margin-block-end: var(--spacing-default);
  }

  ul {
    display: flex;
    flex-direction: column;
    gap: var(--spacing-default);
    list-style: disc;
    padding-inline-start: var(--spacing-default);
  }
}
</style>
