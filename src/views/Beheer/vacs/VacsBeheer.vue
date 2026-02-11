<template>
  <div class="header-wrapper">
    <utrecht-heading :level="1">VACs</utrecht-heading>

    <router-link
      to="/Beheer/vac/"
      title="Toevoegen"
      class="utrecht-button utrecht-button--primary-action icon icon-after plus icon-only"
    >
    </router-link>
  </div>

  <simple-spinner v-if="loading" />

  <div v-else-if="error">
    Er is een fout opgetreden bij het ophalen van de VACs.
  </div>

  <ul v-else>
    <li v-for="vac in vacs" :key="vac.uuid" class="listItem">
      <router-link :to="'/Beheer/vac/' + vac.uuid">{{
        vac.vraag.trim() || "__"
      }}</router-link>

      <utrecht-button
        appearance="secondary-action-button"
        class="icon icon-after trash icon-only"
        title="Verwijderen"
        type="button"
        @click="confirmRemove(vac.uuid)"
      ></utrecht-button>
    </li>
  </ul>
</template>

<script setup lang="ts">
import { onMounted, ref } from "vue";
import {
  Heading as UtrechtHeading,
  Button as UtrechtButton,
} from "@utrecht/component-library-vue";
import SimpleSpinner from "@/components/SimpleSpinner.vue";
import { fetchLoggedIn, parseJson, parsePagination } from "@/services";
import type { Vac } from "@/features/informatie/search/types";
import { toast } from "@/stores/toast";

type StoredVac = Vac & Required<{ uuid: string }>;

const vacObjectenUrl = "/api/vacs/api/v2/objects";

const vacs = ref<StoredVac[]>();
const loading = ref(true);
const error = ref(false);

// eslint-disable-next-line @typescript-eslint/no-explicit-any
const mapVac = (x: any): StoredVac => ({
  uuid: x.uuid,
  vraag: x.record.data.vraag,
  antwoord: x.record.data.antwoord,
});

const fetchAllVacs = async (url: string): Promise<StoredVac[]> => {
  const { page, next } = await fetchLoggedIn(url)
    .then(parseJson)
    .then((json) => parsePagination(json, mapVac));

  if (next) {
    const [pathname] = url.split("?");
    const { searchParams } = new URL(next);

    return [
      ...page,
      ...(await fetchAllVacs(`${pathname}?page=${searchParams.get("page")}`)),
    ];
  }

  return page;
};

const loadAllVacs = () => {
  loading.value = true;

  fetchAllVacs(vacObjectenUrl)
    .then(
      (results) =>
        (vacs.value = results.sort((a, b) => a.vraag.localeCompare(b.vraag))),
    )
    .catch(() => (error.value = true))
    .finally(() => (loading.value = false));
};

const removeVac = async (uuid: string) => {
  loading.value = true;

  const response = await fetchLoggedIn(`${vacObjectenUrl}/${uuid}`, {
    method: "DELETE",
    headers: {
      "Content-Type": "application/json",
    },
  }).finally(() => (loading.value = false));

  response.ok && loadAllVacs();

  toast(
    !response.ok
      ? {
          type: "error",
          text: "Er is een fout opgetreden. Probeer het later opnieuw.",
        }
      : {
          text: "De VAC is verwijderd.",
        },
  );
};

const confirmRemove = (uuid: string) =>
  confirm("Weet u zeker dat u deze VAC wilt verwijderen?") && removeVac(uuid);

onMounted(() => loadAllVacs());
</script>

<style lang="scss" scoped>
.trash {
  flex-shrink: 0;
}
</style>
