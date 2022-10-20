<template>
  <table class="overview">
    <slot name="caption" />
    <template v-if="records.length">
      <thead>
        <tr>
          <th>Naam</th>
          <th>KvK-nummer</th>
          <th class="wrap">Postcode + Huisnummer</th>
          <th>E-mailadres</th>
          <th>Tel. nummer</th>
        </tr>
      </thead>
      <tbody>
        <bedrijven-enrich-context :records="records">
          <template #default="{ enriched }">
            <bedrijven-overzicht-row :result="enriched" />
          </template>
        </bedrijven-enrich-context>
      </tbody>
    </template>
  </table>
</template>

<script lang="ts" setup>
import BedrijvenEnrichContext from "./enricher/BedrijvenEnrichContext.vue";
import BedrijvenOverzichtRow from "./BedrijvenOverzichtRow.vue";
import type { BedrijfHandelsregister, BedrijfKlant } from "./types";

defineProps<{ records: Array<BedrijfHandelsregister | BedrijfKlant> }>();
</script>
