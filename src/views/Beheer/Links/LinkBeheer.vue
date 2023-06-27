<template>
  <utrecht-heading :level="1">Link</utrecht-heading>

  <template v-if="loading">
    <SimpleSpinner />
  </template>

  <template v-else-if="link">
    <form class="container" @submit.prevent="submit">
      <label for="titel" class="utrecht-form-label">
        <span>Titel</span>
        <input
          class="utrecht-textbox utrecht-textbox--html-input"
          type="text"
          id="titel"
          v-model="link.titel"
          required
        />
      </label>

      <label for="naam" class="utrecht-form-label">
        <span>Url</span>
        <input
          type="url"
          id="url"
          class="utrecht-textbox utrecht-textbox--html-input"
          v-model="link.url"
          required
        />
      </label>

      <label for="categorie" class="utrecht-form-label p-r">
        <span>Categorie</span>
        <SearchCombobox
          v-model="link.categorie"
          class="utrecht-textbox utrecht-textbox--html-input"
          required
          :exactMatch="false"
          :listItems="filteredCategorien"
        />
      </label>

      <menu>
        <li>
          <router-link
            to="/Beheer/links/"
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
      </menu>
    </form>
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
import {
  ServiceResult,
  fetchLoggedIn,
  mapServiceData,
  parseJson,
  throwIfNotOk,
} from "@/services";
import { useRouter } from "vue-router";
import SearchCombobox, {
  type DatalistItem,
} from "@/components/SearchCombobox.vue";

const props = defineProps<{ id?: string }>();

type Link = {
  id?: number;
  titel?: string;
  url?: string;
  categorie?: string;
};
const router = useRouter();

const loading = ref<boolean>(false);

const link = ref<Link>();

const showError = () => {
  toast({
    text: "Er is een fout opgetreden. Probeer het later opnieuw.",
    type: "error",
  });
};

const handleSuccess = () => {
  toast({
    text: "link opgeslagen",
  });
  return router.push("/Beheer/links/");
};

const submit = async () => {
  loading.value = true;

  try {
    if (props.id) {
      const result = await fetchLoggedIn("/api/links/" + props.id, {
        method: "PUT",
        headers: {
          "Content-Type": "application/json",
        },
        body: JSON.stringify(link.value),
      });
      if (result.status > 300) {
        showError();
      } else {
        return handleSuccess();
      }
    } else {
      const result = await fetchLoggedIn("/api/links/", {
        method: "POST",
        headers: {
          "Content-Type": "application/json",
        },
        body: JSON.stringify(link.value),
      });
      if (result.status > 300) {
        showError();
      } else {
        return handleSuccess();
      }
    }
    return handleSuccess();
  } catch {
    showError();
  } finally {
    loading.value = false;
  }
};

const categorien = ServiceResult.fromPromise<DatalistItem[]>(
  fetchLoggedIn("/api/categorien")
    .then(throwIfNotOk)
    .then(parseJson)
    .then((r: string[]) => r.map((value) => ({ value })))
);

const filteredCategorien = mapServiceData(categorien, (c) =>
  c.filter(
    ({ value }) =>
      link.value?.categorie &&
      value.toLocaleLowerCase().includes(link.value.categorie)
  )
);

onMounted(async () => {
  loading.value = true;

  try {
    if (props.id) {
      //load link
      const response = await fetchLoggedIn("/api/links/" + props.id);
      if (response.status > 300) {
        showError();
        return;
      }
      const jsonData = await response.json();

      link.value = jsonData;
    } else {
      link.value = {};
    }

    //load categorie suggestions
    await categorien;
  } catch {
    showError();
  } finally {
    loading.value = false;
  }
});
</script>

<style>
menu {
  margin-top: var(--spacing-large);
  display: flex;
  gap: var(--spacing-default);
  justify-content: flex-end;
}

.container {
  display: flex;
  flex-direction: column;
  gap: var(--spacing-default);
}

form {
  margin-top: var(--spacing-default);
}

label > span {
  display: block;
}

.p-r {
  position: relative;
}
</style>
