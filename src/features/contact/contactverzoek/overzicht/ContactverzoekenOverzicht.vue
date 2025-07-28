<template>
  <overview-detail-table
    :level="level"
    :records="mappedCvs"
    :overview-columns="[
      'registratiedatum',
      'onderwerp',
      'status',
      'behandelaar',
    ]"
    :detail-columns="[
      [
        'onderwerp',
        'toelichtingBijContactmoment',
        'zaaknummers',
        'toelichtingVoorCollega',
      ],
      ['klantnaam', 'emails', 'telefoonnummers'],
      ['registratiedatum', 'aangemaaktDoor', 'behandelaar', 'status', 'kanaal'],
    ]"
    :headings="{
      aangemaaktDoor: 'Aangemaakt door',
      behandelaar: 'Behandelaar',
      klantnaam: 'Klantnaam',
      kanaal: 'Kanaal',
      onderwerp: 'Onderwerp',
      registratiedatum: 'Aangemaakt op',
      toelichtingBijContactmoment: 'Informatie voor klant',
      toelichtingVoorCollega: 'Interne toelichting',
      zaaknummers: 'Gekoppelde zaak',
      status: 'Status',
      emails: 'E-mailadres',
      telefoonnummers: 'Telefoonnummer(s)',
    }"
    :highlight="['toelichtingVoorCollega']"
    key-prop="url"
  >
    <template #overview-heading
      ><slot name="overview-heading">Contactverzoeken</slot></template
    >
    <template #caption
      ><slot name="caption">
        <caption class="sr-only">
          Contactverzoeken
        </caption>
      </slot></template
    >
    <template #detail-heading>Contactverzoek</template>
    <template #back-button>Alle contactverzoeken</template>
    <template #detail-button="{ record }"
      >Details van het contactverzoek dat plaats vond op
      <dutch-date-time :date="record.registratiedatum"
    /></template>
    <template #registratiedatum="{ value }">
      <dutch-date-time :date="value" />
    </template>
    <template #toelichtingVoorCollega="{ value }">
      <span class="preserve-newline">{{ value }}</span>
    </template>
  </overview-detail-table>
</template>

<script lang="ts" setup>
import { fullName } from "@/helpers/string";
import DutchDateTime from "@/components/DutchDateTime.vue";
import { computed } from "vue";
import type { ContactverzoekOverzichtItem } from "./types";
import { DigitaalAdresTypes } from "@/services/openklant2";
import OverviewDetailTable from "@/components/OverviewDetailTable.vue";

const { level = 2, contactverzoeken } = defineProps<{
  contactverzoeken: ContactverzoekOverzichtItem[];
  level?: 1 | 2 | 3 | 4;
}>();

const getKlantNaam = (
  betrokkene: ContactverzoekOverzichtItem["betrokkene"],
) => {
  if (!betrokkene) return "";
  if (!betrokkene.organisatie) return fullName(betrokkene.persoonsnaam);
  if (betrokkene.persoonsnaam.achternaam)
    return `${fullName(betrokkene.persoonsnaam)} (${betrokkene.organisatie})`;
  return betrokkene.organisatie;
};

const mappedCvs = computed(() =>
  contactverzoeken.map((cv) => ({
    ...cv,
    status: prettifyStatus(cv.status),
    klantnaam: getKlantNaam(cv.betrokkene),
    zaaknummers: cv.zaaknummers.join(", "),

    emails: cv.betrokkene?.digitaleAdressen
      .filter(
        ({ soortDigitaalAdres }) =>
          soortDigitaalAdres == DigitaalAdresTypes.email,
      )
      .map(({ adres }) => adres)
      .join(", "),
    telefoonnummers: cv.betrokkene?.digitaleAdressen
      .filter(
        ({ soortDigitaalAdres }) =>
          soortDigitaalAdres == DigitaalAdresTypes.telefoonnummer,
      )
      .map(({ adres }) => adres)
      .join(", "),
  })),
);

const prettifyStatus = (status: string) =>
  `${status[0]?.toUpperCase()}${status.substring(1).replace(/_/g, " ")}`;
</script>

<style scoped>
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

td:last-of-type {
  text-align: right;
}
</style>
