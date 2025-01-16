<template>
  <div class="header-wrapper">
    <utrecht-heading :level="1">Vacs</utrecht-heading>

    <router-link
      to="/Beheer/vac/"
      title="Toevoegen"
      class="utrecht-button utrecht-button--primary-action icon icon-after plus icon-only"
    >
    </router-link>
  </div>

  <simple-spinner v-if="loading" />

  <div v-else-if="error">
    Er is een fout opgetreden bij het ophalen van de Vacs.
  </div>

  <ul v-else>
    <li v-for="vac in vacs" :key="vac.uuid" class="listItem">
      <router-link :to="'/Beheer/vac/' + vac.uuid">{{
        vac.vraag || "__"
      }}</router-link>
    </li>
  </ul>
</template>

<script setup lang="ts">
import { watch } from "vue";
import { Heading as UtrechtHeading } from "@utrecht/component-library-vue";
import SimpleSpinner from "@/components/SimpleSpinner.vue";
import { fetchLoggedIn, parseJson, parsePagination } from "@/services";
import type { Vac } from "@/features/search/types";
import { useLoader } from "@/services/use-loader";

// eslint-disable-next-line @typescript-eslint/no-explicit-any
const mapVac = (x: any): Vac => ({
  uuid: x.uuid,
  vraag: x.record.data.vraag,
  antwoord: x.record.data.antwoord,
});

const fetchAllVacs = async (url: string): Promise<Vac[]> => {
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

const {
  data: vacs,
  loading,
  error,
} = useLoader<Vac[]>(() => fetchAllVacs("/api/vacs/api/v2/objects"));

watch(vacs, (value) => value?.sort((a, b) => a.vraag.localeCompare(b.vraag)));
</script>
