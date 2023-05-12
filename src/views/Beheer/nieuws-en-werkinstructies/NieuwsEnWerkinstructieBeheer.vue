<template>
  <utrecht-heading :level="1">Nieuws of werkinstructie</utrecht-heading>

  <template v-if="loading"> <simple-spinner /></template>
  <template v-else-if="bericht">
    <form class="container" @submit.prevent="submit">
      <fieldset>
        <legend>Type</legend>
        <label class="utrecht-form-label" v-for="t in berichtTypes" :key="t">
          <input
            required
            type="radio"
            name="berichtType"
            :value="t"
            v-model="bericht.type"
          />
          {{ t }}
        </label>
      </fieldset>

      <label class="utrecht-form-label" for="titel"
        ><span>Titel</span>
        <input required type="text" id="titel" v-model="bericht.titel" />
      </label>
      <label class="utrecht-form-label" for="inhoud">Inhoud</label>

      <!-- <div class="editorWithPreview">
        <div> -->
      <ck-editor v-model="bericht.inhoud" />
      <!-- </div>
        <div class="preview" v-html="bericht.inhoud"></div>
      </div> -->
      <label class="utrecht-form-label" for="isBelangrijk">
        <input
          type="checkbox"
          id="isBelangrijk"
          name="isBelangrijk"
          v-model="bericht.isBelangrijk"
        />Belangrijk</label
      >

      <label class="utrecht-form-label" for="publicatieDatum">
        <span>Publicatiedatum</span>

        <input
          type="datetime-local"
          id="publicatieDatum"
          v-model="bericht.publicatieDatum"
          required
        />
      </label>

      <label class="utrecht-form-label">
        <span>Publicatie-einddatum</span>

        <input type="datetime-local" v-model="bericht.publicatieEinddatum" />
      </label>

      <fieldset>
        <legend>Skills</legend>
        <label
          v-for="skill in skills"
          :key="skill.id"
          class="utrecht-form-label"
        >
          <input type="checkbox" :value="skill.id" v-model="bericht.skills" />
          {{ skill.naam }}</label
        >
      </fieldset>

      <ul>
        <li></li>
      </ul>

      <!-- <input type="submit" value="ok" @click="submit" /> -->

      <menu>
        <li>
          <router-link to="/Beheer/NieuwsEnWerkinstructies/">
            <utrecht-button appearance="secondary-action-button" type="button">
              Annuleren
            </utrecht-button>
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
import { berichtTypes, type Berichttype } from "@/features/werkbericht/types";
import SimpleSpinner from "@/components/SimpleSpinner.vue";
import { toast } from "@/stores/toast";
import { fetchLoggedIn } from "@/services";
import { useRouter } from "vue-router";
import CkEditor from "@/components/ckeditor";

const props = defineProps(["id"]);

type BerichtDetail = {
  id?: number;
  type?: Berichttype;
  titel?: string;
  inhoud?: string;
  publicatieDatum?: string;
  publicatieEinddatum?: string;
  isBelangrijk?: boolean;
  skills: Array<number>;
};

type Skill = {
  id: number;
  naam: string;
};

const router = useRouter();

const loading = ref<boolean>(true);

const bericht = ref<BerichtDetail | null>(null);
const skills = ref<Array<Skill>>([]);

const addTimezone = (s?: string) => s && new Date(s).toISOString();

const showError = () => {
  toast({
    text: "Er is een fout opgetreden. Probeer het later opnieuw.",
    type: "error",
  });
};

const handleSuccess = () => {
  toast({
    text: "Het bericht is opgeslagen",
  });
  return router.push("/Beheer/NieuwsEnWerkinstructies/");
};

const submit = async () => {
  if (!bericht.value?.inhoud) {
    alert("De inhoud van het bericht mag niet leeg zijn");
    return;
  }
  loading.value = true;
  try {
    bericht.value.publicatieDatum = addTimezone(bericht.value.publicatieDatum);
    bericht.value.publicatieEinddatum = addTimezone(
      bericht.value.publicatieEinddatum
    );
    if (props.id) {
      const result = await fetchLoggedIn("/api/berichten/" + props.id, {
        method: "PUT",
        headers: {
          "Content-Type": "application/json",
        },
        body: JSON.stringify(bericht.value),
      });

      if (result.status > 300) {
        showError();
      } else {
        return handleSuccess();
      }
    } else {
      const result = await fetchLoggedIn("/api/berichten/", {
        method: "POST",
        headers: {
          "Content-Type": "application/json",
        },
        body: JSON.stringify(bericht.value),
      });
      if (result.status > 300) {
        showError();
      } else {
        handleSuccess();
      }
    }
  } catch {
    showError();
  } finally {
    loading.value = false;
  }
};

async function load() {
  loading.value = true;

  try {
    if (props.id) {
      //load bericht if id provided
      const response = await fetchLoggedIn("/api/berichten/" + props.id);

      if (response.status > 300) {
        showError();
        return;
      }

      const jsonData = await response.json();

      jsonData.publicatieDatum = toHtmlInputDateTime(jsonData.publicatieDatum);
      jsonData.publicatieEinddatum = toHtmlInputDateTime(
        jsonData.publicatieEinddatum
      );
      bericht.value = jsonData;
    } else {
      bericht.value = { skills: [] };
    }

    //load skils
    const skillsResponse = await fetchLoggedIn("/api/Skills");

    if (skillsResponse.status > 300) {
      showError();
      return;
    }

    const skillsJonData = await skillsResponse.json();
    skills.value = skillsJonData;
  } catch {
    toast({
      text: "Er is een fout opgetreden. Probeer het later opnieuw.",
      type: "error",
    });
  }
  loading.value = false;
}

function toHtmlInputDateTime(datumString?: string) {
  if (!datumString) return datumString;
  const datum = new Date(datumString);

  return `${datum.getFullYear()}-${("0" + (datum.getMonth() + 1)).slice(-2)}-${(
    "0" + datum.getDate()
  ).slice(-2)}T${("0" + datum.getHours()).slice(-2)}:${(
    "0" + datum.getMinutes()
  ).slice(-2)}`;
}

onMounted(() => {
  load();
});
</script>

<style lang="scss" scoped>
.container {
  display: flex;
  flex-direction: column;
  gap: var(--spacing-default);
}
// .preview {
//   background-color: var(--color-secondary);
//   padding: var(--spacing-default);
// }
// .editorWithPreview {
//   width: 100%;
//   display: flex;
//   gap: var(--spacing-default);
//   * {
//     flex: 1 1 0;
//   }
// }

:deep(.ck-editor ol),
:deep(.ck-editor ul) {
  padding-left: var(--spacing-default);
}

menu {
  margin-top: 2rem;
  display: flex;
  gap: 1rem;
  justify-content: flex-end;
}

form {
  margin-top: var(--spacing-default);
}
label > span {
  display: block;
}
</style>