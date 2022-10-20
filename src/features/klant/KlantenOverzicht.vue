<template>
  <table class="overview">
    <slot name="caption" />
    <template v-if="records.length">
      <thead>
        <tr>
          <th>Naam</th>
          <th>E-mailadres</th>
          <th>Tel. nummer</th>
          <th>Bsn</th>
          <th>Geb. datum</th>
          <th class="wrap">Postcode + Huisnummer</th>
          <th class="row-link-header">Details</th>
        </tr>
      </thead>
      <tbody>
        <personen-enrich-context :records="records">
          <template #default="{ enriched }">
            <klanten-overzicht-row :result="enriched" />
          </template>
        </personen-enrich-context>
      </tbody>
    </template>
  </table>
</template>

<script lang="ts" setup>
import KlantenOverzichtRow from "./KlantenOverzichtRow.vue";
import type { Persoon } from "./types";
import PersonenEnrichContext from "./persoon-enricher/PersonenEnrichContext.vue";
import type { Klant } from "../shared/types";
defineProps<{ records: Klant[] | Persoon[] }>();
</script>
