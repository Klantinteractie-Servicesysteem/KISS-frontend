<template>
  <overview-detail-table
    :records="mappedContactmomenten"
    :overview-columns="[
      'registratiedatum',
      'medewerkerIdentificatie',
      'kanaal',
      'gespreksresultaat',
      'verantwoordelijkeAfdeling',
    ]"
    :detail-groups="[
      [
        'registratiedatum',
        'medewerkerIdentificatie',
        'kanaal',
        'gespreksresultaat',
        'tekst',
        'zaaknummers',
        'vraag',
        'specifiekeVraag',
      ],
    ]"
    :key-prop="'url'"
    :headings="{
      kanaal: 'Kanaal',
      medewerkerIdentificatie: 'Aangemaakt door',
      registratiedatum: 'Aangemaakt op',
      tekst: 'Notitie',
      gespreksresultaat: 'Gespreksresultaat',
      verantwoordelijkeAfdeling: 'Afdeling',
      zaaknummers: 'Gekoppelde zaak',
      vraag: 'Vraag',
      specifiekeVraag: 'Specifieke vraag',
    }"
  >
    <template #overview-heading>Contactmomenten</template>
    <template #table-caption
      ><slot name="caption">
        <caption class="sr-only">
          Contactmomenten
        </caption>
      </slot></template
    >
    <template #detail-button="{ record }"
      >Details van het contactmoment dat plaats vond op
      <dutch-date-time :date="record.registratiedatum" />
    </template>
    <template #detail-heading>Contactmoment</template>
    <template #back-button>Alle contactmomenten</template>
    <template #registratiedatum="{ value }">
      <dutch-date-time :date="value" />
    </template>
  </overview-detail-table>
</template>

<script lang="ts" setup>
import OverviewDetailTable from "@/components/OverviewDetailTable.vue";
import type { ContactmomentViewModel } from "../../features/contact/types";
import { fullName } from "@/helpers/string";
import DutchDateTime from "@/components/DutchDateTime.vue";
import type { ContactmomentDetails } from "../../features/contact/contactBehandelen/vraagBehandelen/klantgerelateerdeGegevensInzien/types";
import { computed } from "vue";

const props = defineProps<{
  contactmomenten: Array<
    ContactmomentViewModel & Partial<ContactmomentDetails>
  >;
}>();

const mappedContactmomenten = computed(() =>
  props.contactmomenten.map((cm) => ({
    ...cm,
    zaaknummers: cm.zaaknummers.join(", "),
    medewerkerIdentificatie: fullName(cm.medewerkerIdentificatie),
  })),
);
</script>
