<template>
  <div class="header-wrapper">
    <UtrechtHeading :level="1"
      >Formulieren contactverzoek {{ soort }}en</UtrechtHeading
    >
    <router-link :to="`/Beheer/formulier-contactverzoek-${soort}/`">
      Toevoegen
    </router-link>
  </div>
  <div v-if="loading"><SimpleSpinner /></div>
  <div v-else class="table-wrapper">
    <table class="overview">
      <thead>
        <tr>
          <th>Titel</th>
          <th>{{ soort }}</th>
          <th></th>
        </tr>
      </thead>
      <tbody>
        <tr v-for="vragenset in vragenSets" :key="vragenset.id">
          <td class="wrap">{{ vragenset.titel }}</td>
          <td>{{ vragenset.organisatorischeEenheidNaam }}</td>
          <td class="actions">
            <ul>
              <li>
                <utrecht-button
                  appearance="secondary-action-button"
                  class="icon icon-after trash icon-only"
                  :title="`Verwijder ${vragenset.titel}`"
                  type="button"
                  @click="confirmVerwijder(vragenset.id)"
                />
              </li>
              <li>
                <router-link
                  class="icon icon-after icon-only chevron-right"
                  :to="`/Beheer/formulier-contactverzoek-${soort}/${vragenset.id}`"
                  :title="`Details ${vragenset.titel}`"
                />
              </li>
            </ul>
          </td>
        </tr>
      </tbody>
    </table>
  </div>
</template>

<script setup lang="ts">
import { ref, watch } from "vue";
import {
  Heading as UtrechtHeading,
  Button as UtrechtButton,
} from "@utrecht/component-library-vue";
import SimpleSpinner from "@/components/SimpleSpinner.vue";
import { toast } from "@/stores/toast";
import { fetchLoggedIn } from "@/services";
import { TypeOrganisatorischeEenheid } from "@/features/types";

const props = defineProps<{ soort?: TypeOrganisatorischeEenheid }>();

type VragenSets = {
  id: number;
  titel: string;
  jsonVragen?: string;
  organisatorischeEenheidId?: string;
  organisatorischeEenheidNaam: string;
  organisatorischeEenheidSoort: TypeOrganisatorischeEenheid;
};

const loading = ref<boolean>(true);
const vragenSets = ref<Array<VragenSets>>([]);

const showError = () => {
  toast({
    text: "Er is een fout opgetreden. Probeer het later opnieuw.",
    type: "error",
  });
};

async function load() {
  loading.value = true;
  try {
    const response = await fetchLoggedIn(
      `/api/contactverzoekvragensets/?soort=${props.soort}`,
    );

    if (response.status > 300) {
      showError();
      return;
    }

    const jsonData: any[] = await response.json();

    vragenSets.value = jsonData
      .map((x) => ({
        ...x,
        id: Number(x.id),
        // ??
        // dateCreated: new Date(x.dateCreated),
        // dateUpdated: x.dateUpdated && new Date(x.dateUpdated),
        // publicatiedatum: new Date(x.publicatiedatum),
      }))
      .sort((a, b) => a.titel.localeCompare(b.titel));
  } catch {
    showError();
  } finally {
    loading.value = false;
  }
}

const verwijder = async (id: number) => {
  loading.value = true;

  try {
    const response = await fetchLoggedIn(
      "/api/contactverzoekvragensets/" + id,
      {
        method: "DELETE",
        headers: {
          "Content-Type": "application/json",
        },
      },
    );
    if (response.status > 300) {
      showError();
      return;
    }
    toast({ text: "Contactformulier verwijderd" });
    await load();
  } catch {
    showError();
  } finally {
    loading.value = false;
  }
};

const confirmVerwijder = (id: number) => {
  if (confirm("Weet u zeker dat u dit contactformulier wilt verwijderen?")) {
    verwijder(id);
  }
};

watch(
  () => props.soort,
  () => load(),
  { immediate: true },
);
</script>

<style scoped lang="scss">
.actions {
  vertical-align: middle;

  ul {
    display: flex;
    gap: var(--spacing-small);
  }

  li {
    display: flex;
    align-items: stretch;
    width: fit-content;
  }

  a {
    display: flex;
    inline-size: 2rem;
  }
}
</style>
