<template>
  <div class="header-wrapper">
    <UtrechtHeading :level="1">Berichten</UtrechtHeading>
    <router-link to="/Beheer/NieuwsEnWerkinstructie/"> Toevoegen </router-link>
  </div>
  <div v-if="loading"><SimpleSpinner /></div>
  <div v-else class="table-wrapper">
    <table class="overview">
      <thead>
        <tr>
          <th>Titel</th>
          <th>Type</th>
          <th>Publicatiedatum</th>
          <th class="wrap">Aangemaakt op</th>
          <th class="wrap">Gewijzigd op</th>
          <th class="row-link-header">Acties</th>
        </tr>
      </thead>
      <tbody>
        <tr v-for="bericht in berichten" :key="bericht.id">
          <td class="wrap">{{ bericht.titel }}</td>
          <td>{{ bericht.type }}</td>
          <td>
            <time :datetime="bericht.publicatiedatum.toISOString()">
              {{ formatDateAndTime(bericht.publicatiedatum) }}
            </time>
          </td>
          <td>
            <time :datetime="bericht.dateCreated.toISOString()">
              {{ formatDateAndTime(bericht.dateCreated) }}
            </time>
          </td>
          <td>
            <time
              v-if="bericht.dateUpdated"
              :datetime="bericht.dateUpdated.toISOString()"
            >
              {{ formatDateAndTime(bericht.dateUpdated) }}
            </time>
          </td>
          <td class="actions">
            <ul>
              <li>
                <utrecht-button
                  appearance="secondary-action-button"
                  class="icon icon-after trash icon-only"
                  :title="`Verwijder ${bericht.titel}`"
                  type="button"
                  @click="confirmVerwijder(bericht.id)"
                />
              </li>
              <li>
                <router-link
                  class="icon icon-after icon-only chevron-right"
                  :to="'/Beheer/NieuwsEnWerkinstructie/' + bericht.id"
                  :title="`Details ${bericht.titel}`"
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
import { onMounted, ref } from "vue";
import {
  Heading as UtrechtHeading,
  Button as UtrechtButton,
} from "@utrecht/component-library-vue";
import SimpleSpinner from "@/components/SimpleSpinner.vue";
import { toast } from "@/stores/toast";
import { fetchLoggedIn } from "@/services";
import { formatDateAndTime } from "@/helpers/date";
import { fetchFeaturedWerkberichten } from "@/features/werkbericht";

type berichtType = {
  id: number;
  titel: string;
  type: string;
  dateCreated: Date;
  dateUpdated?: Date;
  publicatiedatum: Date;
};
const loading = ref<boolean>(true);
const berichten = ref<Array<berichtType>>([]);

const showError = () => {
  toast({
    text: "Er is een fout opgetreden. Probeer het later opnieuw.",
    type: "error",
  });
};

async function load() {
  loading.value = true;

  try {
    const response = await fetchLoggedIn("/api/berichten");
    if (response.status > 300) {
      showError();
      return;
    }
    const jsonData: any[] = await response.json();
    berichten.value = jsonData.map((x) => ({
      ...x,
      dateCreated: new Date(x.dateCreated),
      dateUpdated: x.dateUpdated && new Date(x.dateUpdated),
      publicatiedatum: new Date(x.publicatiedatum),
    }));
  } catch {
    showError();
  } finally {
    loading.value = false;
  }
}

const verwijder = async (id: number) => {
  loading.value = true;

  try {
    const response = await fetchLoggedIn("/api/berichten/" + id, {
      method: "DELETE",
      headers: {
        "Content-Type": "application/json",
      },
    });
    if (response.status > 300) {
      showError();
      return;
    }
    fetchFeaturedWerkberichten();
    toast({ text: "Bericht verwijderd" });
    await load();
  } catch {
    showError();
  } finally {
    loading.value = false;
  }
};

const confirmVerwijder = (id: number) => {
  if (confirm("Weet u zeker dat u dit bericht wilt verwijderen?")) {
    verwijder(id);
  }
};

onMounted(() => {
  load();
});
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
