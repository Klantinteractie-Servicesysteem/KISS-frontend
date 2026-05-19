<template>
  <utrecht-heading :level="1">Links</utrecht-heading>
  <div v-if="loading"><SimpleSpinner /></div>
  <div v-else-if="error">Er is een fout opgetreden.</div>

  <ul v-else>
    <li v-for="linksgroep in linksGroepen" :key="linksgroep.categorie">
      <utrecht-heading :level="2">{{ linksgroep.categorie }}</utrecht-heading>
      <ul>
        <li v-for="link in linksgroep.items" :key="link.id" class="listItem">
          <router-link :to="'/Beheer/link/' + link.id">{{
            link.titel
          }}</router-link>

          <utrecht-button
            appearance="secondary-action-button"
            class="icon icon-after trash icon-only"
            title="Verwijderen"
            type="button"
            @click="confirmVerwijder(link.id)"
          ></utrecht-button>
        </li>
      </ul>
    </li>
  </ul>
  <menu>
    <router-link
      to="/Beheer/link/"
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

type Link = {
  id: number;
  titel: string;
};

type LinkGroep = {
  categorie: string;
  items: Array<Link>;
};

const loading = ref<boolean>(true);
const error = ref<boolean>(false);
const linksGroepen = ref<Array<LinkGroep>>([]);

const showError = () => {
  toast({
    text: "Er is een fout opgetreden. Probeer het later opnieuw.",
    type: "error",
  });
};

async function load() {
  loading.value = true;

  try {
    const response = await fetchLoggedIn("/api/links");
    if (response.status > 300) {
      showError();
      return;
    }

    const jsonData = await response.json();
    linksGroepen.value = jsonData;
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
    const response = await fetchLoggedIn("/api/links/" + id, {
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
      text: "link verwijderd",
    });

    await load();
  } catch {
    showError();
  } finally {
    loading.value = false;
  }
};

const confirmVerwijder = (id: number) => {
  if (confirm("Weet u zeker dat u deze link wilt verwijderen?")) {
    verwijder(id);
  }
};

onMounted(() => {
  load();
});
</script>

<style scoped lang="scss">
li h2 {
  margin-block-start: var(--spacing-large);
}
</style>
