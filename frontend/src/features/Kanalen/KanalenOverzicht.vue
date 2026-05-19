<template>
  <simple-spinner v-if="kanalen.loading" />
  <application-message
    v-if="kanalen.error"
    :message-type="'error'"
    message="Er ging iets mis. Probeer het later nog eens."
  />

  <template v-if="kanalen.success">
    <ul v-if="kanalen.data.length">
      <li v-for="{ id, naam } in kanalen.data" :key="id" class="listItem">
        <router-link :to="BEHEER_URL + id">{{ naam }}</router-link>

        <utrecht-button
          appearance="secondary-action-button"
          class="icon icon-after trash icon-only"
          title="Verwijderen"
          type="button"
          @click="confirmVerwijder(id)"
        ></utrecht-button>
      </li>
    </ul>
    <p v-else>Geen kanalen gevonden.</p>
  </template>
</template>

<script lang="ts" setup>
import { useKanalen, verwijderkanaal } from "./service";
import SimpleSpinner from "@/components/SimpleSpinner.vue";
import ApplicationMessage from "@/components/ApplicationMessage.vue";
import { ref } from "vue";

import { toast } from "@/stores/toast";
import { fetchLoggedIn, throwIfNotOk } from "@/services";
import { Button as UtrechtButton } from "@utrecht/component-library-vue";

const kanalen = useKanalen();
const loading = ref<boolean>(true);
const BEHEER_URL = "/beheer/kanaal/";

const showError = () => {
  toast({
    text: "Er is een fout opgetreden. Probeer het later opnieuw.",
    type: "error",
  });
};

const verwijder = async (id: number) => {
  loading.value = true;

  try {
    const response = await verwijderkanaal(id);
    throwIfNotOk(response);
    await kanalen.refresh();
    return toast({
      text: "Kanaal verwijderd",
    });
  } catch {
    return showError();
  } finally {
    loading.value = false;
  }
};

const confirmVerwijder = (id: number) => {
  if (confirm("Weet u zeker dat u dit item wilt verwijderen?")) {
    verwijder(id);
  }
};
</script>

<style lang="scss" scoped>
dt {
  font-weight: 600;
  margin-block-end: var(--spacing-small);

  &:not(:first-of-type) {
    margin-block-start: var(--spacing-default);
  }
}

a:not(:visited) {
  color: #2e71d7;
}
</style>
