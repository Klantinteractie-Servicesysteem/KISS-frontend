<template>
  <table class="overview">
    <slot name="caption" />
    <template v-if="records.length">
      <thead>
        <tr>
          <th>Naam</th>
          <th>Type</th>
          <th>KvK-nummer</th>
          <th class="wrap">Postcode + Huisnummer</th>
          <th>E-mailadres</th>
          <th>Tel. nummer</th>
          <th class="row-link-header">Details</th>
        </tr>
      </thead>
      <tbody>
        <bedrijven-overzicht-row
          v-for="(record, idx) in records"
          :key="idx"
          :item="record"
          :auto-navigate="navigateOnSingleResult && records.length === 1"
        />
      </tbody>
    </template>
  </table>
</template>

<script lang="ts" setup>
import type { Bedrijf } from "@/services/kvk";
import BedrijvenOverzichtRow from "./BedrijvenOverzichtRow.vue";

defineProps<{
  records: Array<Bedrijf>;
  navigateOnSingleResult?: boolean;
}>();
</script>
