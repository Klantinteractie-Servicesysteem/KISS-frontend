<template>
  <template v-if="!currentCv">
    <utrecht-heading :level="level"> Contactverzoeken </utrecht-heading>
    <div class="table-wrapper">
      <table class="overview">
        <caption class="sr-only">
          Contactverzoeken
        </caption>
        <thead>
          <tr>
            <th>Datum</th>
            <th>Onderwerp</th>
            <th>Status</th>
            <th>Behandelaar</th>
            <th>Details</th>
          </tr>
        </thead>
        <tbody>
          <tr v-for="cv in contactverzoeken" :key="cv.url">
            <td><dutch-date-time :date="cv.registratiedatum" /></td>
            <td>{{ cv.onderwerp }}</td>
            <td>{{ cv.status }}</td>
            <td>{{ cv.behandelaar }}</td>
            <td>
              <button type="button" @click="currentCv = cv">
                Details<span class="sr-only">
                  van contactverzoek op
                  <dutch-date-time :date="cv.registratiedatum"
                /></span>
              </button>
            </td>
          </tr>
        </tbody>
      </table>
    </div>
  </template>
  <template v-else>
    <button @click="currentCv = undefined">Alle contactverzoeken</button>
    <utrecht-heading :level="level" :id="generatedId"
      >Contactverzoek</utrecht-heading
    >
    <dl :aria-labelledby="generatedId">
      <dt>Onderwerp / vraag</dt>
      <dd>{{ currentCv.vraag }}</dd>
      <dt>Informatie voor klant</dt>
      <dd>{{ currentCv.toelichtingBijContactmoment }}</dd>
      <template v-if="currentCv.zaaknummers.length">
        <dt>Gekoppelde zaak</dt>
        <dd>
          {{ currentCv.zaaknummers.join(", ") }}
        </dd>
      </template>
      <dt>Interne toelichting</dt>
      <dd>{{ currentCv.toelichtingVoorCollega }}</dd>
      <template v-if="currentCv.betrokkene?.persoonsnaam?.achternaam">
        <dt>Naam betrokkene</dt>
        <dd>
          {{ fullName(currentCv.betrokkene.persoonsnaam) }}
        </dd>
      </template>
      <template v-if="currentCv.betrokkene?.organisatie">
        <dt>Organisatie</dt>
        <dd>{{ currentCv.betrokkene?.organisatie }}</dd>
      </template>
      <dt>E-mailadres</dt>
      <dd>TODO</dd>
      <dt>Telefoonnummer(s)</dt>
      <dd>TODO</dd>
      <dt>Aangemaakt op</dt>
      <dd><dutch-date-time :date="currentCv.registratiedatum" /></dd>
      <dt>Aangemaakt door</dt>
      <dd>{{ currentCv.aangemaaktDoor }}</dd>
      <dt>Behandelaar</dt>
      <dd>{{ currentCv.behandelaar }}</dd>
      <dt>Status</dt>
      <dd>{{ currentCv.status }}</dd>
      <dt>Kanaal</dt>
      <dd>TODO</dd>
    </dl>
  </template>
</template>

<script lang="ts" setup>
import { fullName } from "@/helpers/string";
import DutchDateTime from "@/components/DutchDateTime.vue";
import { Heading as UtrechtHeading } from "@utrecht/component-library-vue";
import { ref, useId } from "vue";
import type { ContactverzoekOverzichtItem } from "./types";
const { level = 3 } = defineProps<{
  contactverzoeken: ContactverzoekOverzichtItem[];
  level: 1 | 2 | 3 | 4;
}>();

const generatedId = useId();

const capitalizeFirstLetter = (val: string) =>
  `${val?.[0]?.toLocaleUpperCase() || ""}${val?.substring(1) || ""}`;

const currentCv = ref<ContactverzoekOverzichtItem>();
</script>

<style scoped>
.preserve-newline {
  white-space: pre-line;
}

.max18char {
  max-width: 18ch;
  overflow: hidden;
  white-space: nowrap;
  text-overflow: ellipsis;
}

dl {
  display: grid;
  grid-template-columns: auto auto;
}
</style>
