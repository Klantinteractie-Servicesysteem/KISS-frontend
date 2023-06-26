<template>
  <utrecht-heading :level="1">Skill</utrecht-heading>

  <template v-if="loading"><SimpleSpinner /></template>

  <template v-else>
    <form class="container" @submit.prevent="submit">
      <label for="naam" class="utrecht-form-label"
        ><span>Naam</span>
        <input
          class="utrecht-textbox utrecht-textbox--html-input"
          type="text"
          id="naam"
          v-model="skill.naam"
          required
      /></label>
      <menu>
        <li>
          <router-link
            to="/Beheer/Skills/"
            class="utrecht-button utrecht-button--secondary-action"
          >
            Annuleren
          </router-link>
        </li>

        <li>
          <utrecht-button
            appearance="primary-action-button"
            type="submit"
            @click="submit"
          >
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
import { fetchLoggedIn } from "@/services";
import { useRouter } from "vue-router";
const props = defineProps(["id"]);

type skillType = {
  id?: number;
  naam?: string;
};
const router = useRouter();

const loading = ref<boolean>(false);

const skill = ref<skillType>({});

const showError = () => {
  toast({
    text: "Er is een fout opgetreden. Probeer het later opnieuw.",
    type: "error",
  });
};

const handleSuccess = () => {
  toast({
    text: "Skill opgeslagen",
  });
  return router.push("/Beheer/Skills/");
};

const submit = async () => {
  loading.value = true;

  try {
    if (props.id) {
      const result = await fetchLoggedIn("/api/Skills/" + props.id, {
        method: "PUT",
        headers: {
          "Content-Type": "application/json",
        },
        body: JSON.stringify(skill.value),
      });
      if (result.status > 300) {
        showError();
      } else {
        return handleSuccess();
      }
    } else {
      const result = await fetchLoggedIn("/api/Skills/", {
        method: "POST",
        headers: {
          "Content-Type": "application/json",
        },
        body: JSON.stringify(skill.value),
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

onMounted(async () => {
  if (props.id) {
    loading.value = true;
    try {
      const response = await fetchLoggedIn("/api/Skills/" + props.id);
      if (response.status > 300) {
        showError();
        return;
      }
      const jsonData = await response.json();
      skill.value = jsonData;
    } catch {
      showError();
    } finally {
      loading.value = false;
    }
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
</style>
