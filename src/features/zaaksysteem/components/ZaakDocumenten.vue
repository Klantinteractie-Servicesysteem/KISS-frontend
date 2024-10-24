<template>
  <utrecht-heading :level="2" modelValue>Documenten</utrecht-heading>

  <template v-if="zaak.documenten?.length">
    <table>
      <thead>
        <tr>
          <th>Naam</th>
          <th>Formaat</th>
          <th>Creatiedatum</th>
          <th>Vertrouwelijk</th>
          <th>Downloaden</th>
        </tr>
      </thead>
      <tbody>
        <tr v-for="document in zaak.documenten" :key="document.id">
          <td class="wrap">{{ document.titel }}</td>
          <td>{{ formatBytes(document.bestandsomvang) }}</td>
          <td>{{ formatDateOnly(document.creatiedatum) }}</td>
          <td class="vertrouwelijkheid-label">
            {{ document.vertrouwelijkheidaanduiding }}
          </td>
          <td>
            <utrecht-button
              @click.prevent="download(document)"
              appearance="secondary-action-button"
            >
              Downloaden
            </utrecht-button>
          </td>
        </tr>
      </tbody>
    </table>
  </template>

  <span v-if="!zaak.documenten?.length">Geen documenten gevonden.</span>
</template>

<script setup lang="ts">
import type { ZaakDetails, ZaakDocument } from "./../types";
import { Heading as UtrechtHeading } from "@utrecht/component-library-vue";
import { formatDateOnly } from "@/helpers/date";
import { formatBytes } from "@/helpers/formatBytes";
import { throwIfNotOk } from "@/services";
import { Button as UtrechtButton } from "@utrecht/component-library-vue";
import { fetchWithZaaksysteemId } from "@/services/openzaak";

const props = defineProps<{
  zaak: ZaakDetails;
}>();
// bij het implementeren van meerdere zaaksystemen is gekozen om de zaaksysteemid mee te geven in de header
// zodat de requests en querystrings verder zo min mogelijk afwijken van de api standaard
// dit betekent dat we het downloaden van documenten op een omslachtige manier moeten doen,
// omdat je in een gewone link geen header mee kan geven.
async function download(doc: ZaakDocument) {
  const url = doc.url + "/download";
  const blob = await fetchWithZaaksysteemId(props.zaak.zaaksysteemId, url)
    .then(throwIfNotOk)
    .then((r) => r.blob());
  const objectUrl = URL.createObjectURL(blob);
  const a = document.createElement("a");
  a.style.display = "none";
  a.href = objectUrl;
  a.download = doc.bestandsnaam || doc.titel;
  a.target = "_blank";
  document.body.appendChild(a);
  a.click();
  URL.revokeObjectURL(objectUrl);
  a.remove();
}
</script>

<style scoped lang="scss">
section {
  padding: var(--spacing-large);

  & > *:not(:last-child) {
    margin-block-end: var(--spacing-large);
  }
}

table {
  width: 100%;

  thead {
    color: var(--color-white);
    background: var(--color-tertiary);

    th {
      font-weight: normal;
      text-align: start;
    }
  }

  th,
  td {
    padding-block: var(--spacing-default);
    padding-inline-start: var(--spacing-default);

    &:not(.wrap) {
      white-space: nowrap;
    }

    &:last-child {
      padding-inline-end: var(--spacing-default);
    }
  }

  tbody > tr {
    background: var(--color-white);
    border-bottom: 2px solid var(--color-tertiary);
  }

  .vertrouwelijkheid-label::first-letter {
    text-transform: capitalize;
  }
}
</style>
