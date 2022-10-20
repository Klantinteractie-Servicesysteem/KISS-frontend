<template>
  <tr class="row-link">
    <th scope="row" class="wrap">
      <div class="skeleton" v-if="result.bedrijfsnaam.loading" />
      <template v-else-if="result.bedrijfsnaam.success">{{
        result.bedrijfsnaam.data
      }}</template>
    </th>
    <td>
      <div class="skeleton" v-if="result.kvknummer.loading" />
      <template v-if="result.kvknummer.success">{{
        result.kvknummer.data
      }}</template>
    </td>
    <td>
      <div class="skeleton" v-if="result.postcodeHuisnummer.loading" />
      <template v-if="result.postcodeHuisnummer.success">{{
        result.postcodeHuisnummer.data
      }}</template>
    </td>
    <td class="wrap">
      <div class="skeleton" v-if="result.telefoonnummer.loading" />
      <template v-if="result.telefoonnummer.success">{{
        result.telefoonnummer.data
      }}</template>
    </td>
    <td class="wrap">
      <div class="skeleton" v-if="result.email.loading" />
      <template v-if="result.email.success">{{ result.email.data }}</template>
    </td>
    <td>
      <div class="skeleton" v-if="result.detailLink.loading" />
      <template v-if="result.detailLink.success">
        <router-link
          v-if="result.detailLink.data"
          v-bind="result.detailLink.data"
        />
        <button
          type="button"
          title="Aanmaken"
          v-else
          @click="result.create()"
        />
      </template>
    </td>
  </tr>
</template>
<script lang="ts" setup>
import type { EnrichedBedrijf } from "./types";

defineProps<{ result: EnrichedBedrijf }>();
</script>

<style scoped lang="scss">
td:empty::after {
  content: "-";
}

.skeleton {
  min-height: 1rem;
}
</style>
