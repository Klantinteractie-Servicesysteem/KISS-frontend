<template>
  <overview-detail-table
    @itemSelected="onContactverzoekSelected"
    :level="level"
    :records="mappedCvs"
    :overview-columns="[
      'registratiedatum',
      'onderwerp',
      'status',
      'behandelaar',
    ]"
    :detail-groups="[
      [
        'onderwerp',
        'toelichtingBijContactmoment',
        'zaaknummers',
        'toelichtingVoorCollega',
      ],
      ['klantnaam', 'emails', 'telefoonnummer1', 'otherTelefoonnummers'],
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
      telefoonnummer1: 'Eerste telefoonnummer',
      otherTelefoonnummers: 'Andere telefoonnummers',
    }"
    :highlight="['toelichtingVoorCollega']"
    key-prop="uuid"
  >
    <template #overview-heading
      ><slot name="overview-heading">Contactverzoeken</slot></template
    >
    <template #table-caption
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

  <logboek-overzicht
    class="logboek"
    v-if="selectedContactverzoekId"
    :contactverzoek-id="selectedContactverzoekId"
    :level="level"
  />
</template>

<script lang="ts" setup>
import { fullName } from "@/helpers/string";
import DutchDateTime from "@/components/DutchDateTime.vue";
import { computed, ref } from "vue";
import type { ContactverzoekOverzichtItem } from "./types";
import { DigitaalAdresTypes } from "@/services/openklant2";
import OverviewDetailTable from "@/components/OverviewDetailTable.vue";
import LogboekOverzicht from "../contactverzoekLogboek/LogboekOverzicht.vue";

const selectedContactverzoekId = ref<string | undefined>(undefined);
const onContactverzoekSelected = (id: string | undefined) =>
  (selectedContactverzoekId.value = id);

const { level = 2, contactverzoeken } = defineProps<{
  contactverzoeken: ContactverzoekOverzichtItem[];
  level?: 1 | 2 | 3 | 4;
}>();

const getKlantNaam = (betrokkene: ContactverzoekOverzichtItem["betrokkene"]) =>
  [
    betrokkene?.organisatie,
    betrokkene?.persoonsnaam && fullName(betrokkene?.persoonsnaam),
  ]
    .filter((x) => !!x)
    .join(" - ");

const mappedCvs = computed(() =>
  contactverzoeken.map((cv) => {
    const telefoonnummers =
      cv.betrokkene?.digitaleAdressen.filter(
        ({ soortDigitaalAdres }) =>
          soortDigitaalAdres == DigitaalAdresTypes.telefoonnummer,
      ) ?? [];

    return {
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

      telefoonnummer1: telefoonnummers?.[0]?.adres,

      otherTelefoonnummers: telefoonnummers
        .filter((_, i) => i > 0)
        .map(({ adres, omschrijving }) => `${adres} (${omschrijving})`)
        .join(", "),
    };
  }),
);

const prettifyStatus = (status: string) =>
  `${status[0]?.toUpperCase()}${status.substring(1).replace(/_/g, " ")}`;
</script>

<style scoped>
.logboek {
  margin-top: var(--spacing-default);
}
</style>
