<template>
  <utrecht-heading :level="1">Gespreksresultaat</utrecht-heading>

  <template v-if="loading">
    <SimpleSpinner />
  </template>

  <template v-else-if="gespreksresultaat">
    <beheer-form @submit="submit">
      <template #formFields>
        <label for="titel" class="utrecht-form-label">
          <span>Titel</span>
          <input
            type="text"
            class="utrecht-textbox utrecht-textbox--html-input"
            id="titel"
            v-model="gespreksresultaat.definitie"
            required
          />
        </label>
      </template>
      <template #formMenuListItems>
        <li>
          <router-link :to="LIJST_BEHEER_URL">
            <utrecht-button appearance="secondary-action-button" type="button">
              Annuleren
            </utrecht-button>
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
</template>

<script setup lang="ts">
import { onMounted, ref } from "vue";
import {
  Heading as UtrechtHeading,
  Button as UtrechtButton,
} from "@utrecht/component-library-vue";
import SimpleSpinner from "@/components/SimpleSpinner.vue";
import { toast } from "@/stores/toast";
import { fetchLoggedIn, parseJson, throwIfNotOk } from "@/services";
import { useRouter } from "vue-router";
import beheerForm from "@/components/beheer/BeheerForm.vue";

const props = defineProps<{ id?: string }>();
const LIJST_BEHEER_URL = "/beheer/gespreksresultaten/";
const API_URL = "/api/gespreksresultaten/";
type Gespreksresultaat = {
  id?: number;
  definitie?: string;
};
const router = useRouter();

const loading = ref<boolean>(false);

const gespreksresultaat = ref<Gespreksresultaat>({});

const showError = () => {
  toast({
    text: "Er is een fout opgetreden. Probeer het later opnieuw.",
    type: "error",
  });
};

const handleSuccess = () => {
  toast({
    text: "Gespreksresultaat opgeslagen",
  });
  return router.push(LIJST_BEHEER_URL);
};

const submit = () => {
  loading.value = true;
  return fetchLoggedIn(`${API_URL}${props.id || ""}`, {
    method: props.id ? "PUT" : "POST",
    headers: {
      "Content-Type": "application/json",
    },
    body: JSON.stringify(gespreksresultaat.value),
  })
    .then(throwIfNotOk)
    .then(handleSuccess)
    .catch(showError)
    .finally(() => {
      loading.value = false;
    });
};

onMounted(async () => {
  if (props.id) {
    loading.value = true;
    gespreksresultaat.value =
      (await fetchLoggedIn(API_URL + props.id)
        .then(throwIfNotOk)
        .then(parseJson)
        .catch(showError)
        .finally(() => {
          loading.value = false;
        })) ?? {};
  }
});
</script>
