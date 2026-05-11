<template>
  <utrecht-heading :level="1">Skills</utrecht-heading>
  <div v-if="loading"><SimpleSpinner /></div>
  <div v-else-if="error">Er is een fout opgetreden.</div>

  <ul v-else>
    <li v-for="skill in skills" :key="skill.id" class="listItem">
      <router-link :to="'/Beheer/Skill/' + skill.id">{{
        skill.naam
      }}</router-link>

      <utrecht-button
        appearance="secondary-action-button"
        class="icon icon-after trash icon-only"
        title="Verwijderen"
        type="button"
        @click="confirmVerwijder(skill.id)"
      ></utrecht-button>
    </li>
  </ul>
  <menu>
    <router-link
      to="/Beheer/Skill/"
      title="toevoegen"
      class="utrecht-button utrecht-button--primary-action icon icon-after plus icon-only"
    >
    </router-link>
  </menu>
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

type skill = {
  id: number;
  naam: string;
};

const loading = ref<boolean>(true);
const error = ref<boolean>(false);
const skills = ref<Array<skill>>([]);

const showError = () => {
  toast({
    text: "Er is een fout opgetreden. Probeer het later opnieuw.",
    type: "error",
  });
};

async function load() {
  loading.value = true;

  try {
    const response = await fetchLoggedIn("/api/Skills");
    if (response.status > 300) {
      showError();
      return;
    }

    const jsonData = await response.json();
    skills.value = jsonData;
  } catch {
    showError();
  } finally {
    loading.value = false;
  }
}

const verwijder = async (id: number) => {
  loading.value = true;
  error.value = false;
  try {
    const response = await fetchLoggedIn("/api/Skills/" + id, {
      method: "DELETE",
      headers: {
        "Content-Type": "application/json",
      },
    });

    if (response.status > 300) {
      showError();
      return;
    }

    toast({
      text: "Skill verwijderd",
    });

    await load();
  } catch {
    showError();
  } finally {
    loading.value = false;
  }
};

const confirmVerwijder = (id: number) => {
  if (confirm("Weet u zeker dat u deze skill wilt verwijderen?")) {
    verwijder(id);
  }
};

onMounted(() => {
  load();
});
</script>
