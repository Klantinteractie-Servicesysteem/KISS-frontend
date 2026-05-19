<template>
  <utrecht-heading :level="1">Kanaal</utrecht-heading>

  <template v-if="loading">
    <SimpleSpinner />
  </template>

  <template v-else-if="item">
    <beheer-form @submit="submit">
      <template #formFields>
        <label for="titel" class="utrecht-form-label">
          <span>Naam</span>
          <input
            class="utrecht-textbox utrecht-textbox--html-input"
            type="text"
            id="titel"
            v-model="item.naam"
            required
          />
        </label>
      </template>
      <template #formMenuListItems>
        <li>
          <router-link
            to="/Beheer/kanalen/"
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
</template>

<script setup lang="ts">
import { onMounted, ref } from "vue";
import {
  Heading as UtrechtHeading,
  Button as UtrechtButton,
} from "@utrecht/component-library-vue";
import SimpleSpinner from "@/components/SimpleSpinner.vue";
import { toast } from "@/stores/toast";
import { fetchLoggedIn } from "@/services";
import { useRouter } from "vue-router";
import beheerForm from "@/components/beheer/BeheerForm.vue";

const props = defineProps<{ id?: string }>();

type Kanaal = {
  id?: string;
  naam?: string;
};
const router = useRouter();

const loading = ref<boolean>(false);

const item = ref<Kanaal>();

const showError = (message?: string) => {
  toast({
    text: message ?? "Er is een fout opgetreden. Probeer het later opnieuw.",
    type: "error",
  });
};

const handleSuccess = () => {
  toast({
    text: "opgeslagen",
  });
  return router.push("/Beheer/kanalen/");
};

const submit = async () => {
  loading.value = true;

  try {
    if (props.id) {
      return update(props.id);
    }
    return create();
  } catch {
    showError();
  } finally {
    loading.value = false;
  }
};

async function update(id: string) {
  const result = await fetchLoggedIn("/api/KanaalBewerken/" + id, {
    method: "PUT",
    headers: {
      "Content-Type": "application/json",
    },
    body: JSON.stringify(item.value),
  });

  if (result.status > 300) {
    showError();
  } else {
    return handleSuccess();
  }
}

async function create() {
  const result = await fetchLoggedIn("/api/KanaalToevoegen/", {
    method: "POST",
    headers: {
      "Content-Type": "application/json",
    },
    body: JSON.stringify(item.value),
  });

  if (result.status == 409) {
    showError("Er bestaat al een item met dezelfde naam");
  } else if (result.status > 300) {
    showError();
  } else {
    return handleSuccess();
  }
}

onMounted(async () => {
  loading.value = true;
  try {
    if (props.id) {
      //load link
      const response = await fetchLoggedIn(
        "/api/KanaalBeheerDetails/" + props.id,
      );
      if (response.status > 300) {
        showError();
        return;
      }
      const jsonData = await response.json();

      item.value = jsonData;
    } else {
      item.value = {};
    }
  } catch {
    showError();
  } finally {
    loading.value = false;
  }
});
</script>
