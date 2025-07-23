<template>
  <template v-if="!currentCv">
    <utrecht-heading :level="level">Contactverzoeken</utrecht-heading>
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
            <th><span class="sr-only">Details</span></th>
          </tr>
        </thead>
        <tbody>
          <tr v-for="cv in contactverzoeken" :key="cv.url">
            <td><dutch-date-time :date="cv.registratiedatum" /></td>
            <td>{{ cv.onderwerp }}</td>
            <td>{{ cv.status }}</td>
            <td>{{ cv.behandelaar }}</td>
            <td>
              <button
                type="button"
                @click="currentCv = cv"
                :id="`details_${cv.url}`"
              >
                <span class="sr-only">Open </span>Details<span class="sr-only">
                  van het contactverzoek dat plaats vond op
                  <dutch-date-time :date="cv.registratiedatum"
                /></span>
              </button>
            </td>
          </tr>
        </tbody>
      </table>
    </div>
  </template>
  <div v-else class="details">
    <button @click="goToOverview" class="icon-before chevron-left">
      Alle contactverzoeken
    </button>
    <utrecht-heading :level="level" :id="generatedId"
      >Contactverzoek</utrecht-heading
    >
    <dl :aria-labelledby="generatedId">
      <div>
        <dt>Onderwerp / vraag</dt>
        <dd>{{ currentCv.vraag }}</dd>
        <dt>Informatie voor klant</dt>
        <dd>{{ currentCv.toelichtingBijContactmoment }}</dd>
        <dt>Gekoppelde zaak</dt>
        <dd>
          {{ currentCv.zaaknummers.join(", ") }}
        </dd>
        <dt class="intern">Interne toelichting</dt>
        <dd class="intern">{{ currentCv.toelichtingVoorCollega }}</dd>
      </div>
      <div>
        <dt>Klantnaam</dt>
        <dd>
          {{
            currentCv.betrokkene?.persoonsnaam.achternaam
              ? fullName(currentCv.betrokkene.persoonsnaam)
              : currentCv.betrokkene?.organisatie
          }}
        </dd>
        <template
          v-if="
            !currentCv.betrokkene?.persoonsnaam.achternaam &&
            !!currentCv.betrokkene?.organisatie
          "
        >
          <dt>Organisatie</dt>
          <dd>{{ currentCv.betrokkene?.organisatie }}</dd>
        </template>
        <dt>E-mailadres</dt>
        <dd>TODO</dd>
        <dt>Telefoonnummer(s)</dt>
        <dd>TODO</dd>
      </div>
      <div>
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
      </div>
    </dl>
  </div>
</template>

<script lang="ts" setup>
import { fullName } from "@/helpers/string";
import DutchDateTime from "@/components/DutchDateTime.vue";
import { Heading as UtrechtHeading } from "@utrecht/component-library-vue";
import { nextTick, ref, useId } from "vue";
import type { ContactverzoekOverzichtItem } from "./types";

const { level = 2 } = defineProps<{
  contactverzoeken: ContactverzoekOverzichtItem[];
  level: 1 | 2 | 3 | 4;
}>();
const generatedId = useId();

const capitalizeFirstLetter = (val: string) =>
  `${val?.[0]?.toLocaleUpperCase() || ""}${val?.substring(1) || ""}`;

const currentCv = ref<ContactverzoekOverzichtItem>();

const goToOverview = () => {
  if (!currentCv.value) return;
  const cvUrl = currentCv.value.url;
  currentCv.value = undefined;
  nextTick(() => {
    document.getElementById(`details_${cvUrl}`)?.focus();
  });
};
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
  background: var(--color-white);
  display: grid;
  grid-template-columns: minmax(min-content, max-content) auto;
  row-gap: 0.75rem;

  > div {
    display: grid;
    grid-template-columns: subgrid;
    grid-column: 1 / -1;
    padding-block: 0.5rem;
    border-bottom: 1px solid var(--color-accent);
  }
}

dd,
dt {
  padding-block: 0.5rem;
  padding-inline: 1rem;

  &:empty::before {
    content: "-";
  }
}

dt.intern {
  border-inline-start: 4px var(--color-primary) solid;
}

.details {
  display: flex;
  flex-direction: column;
  gap: 0.5rem;
}

button {
  align-self: start;
}

.intern {
  background-color: var(--color-secondary);
}
</style>
