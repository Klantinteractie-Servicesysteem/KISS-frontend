<template>
  <div class="table-wrapper">
    <table class="overview">
      <slot name="caption"></slot>
      <template v-if="zaken.length > 0">
        <thead>
          <tr>
            <th>Zaaknummer</th>
            <th>Aanvrager</th>
            <th>Zaaktype</th>
            <th>Status</th>
            <th>Behandelaar</th>
            <th>Indiendatum</th>
            <th class="row-link-header">Details</th>
          </tr>
        </thead>
        <tbody>
          <tr v-for="zaak in zaken" :key="zaak.url" class="row-link">
            <th scope="row">{{ zaak.identificatie }}</th>
            <td class="wrap">{{ zaak.aanvrager }}</td>
            <td class="wrap">{{ zaak.zaaktypeOmschrijving }}</td>
            <td class="wrap">{{ zaak.status }}</td>
            <td>{{ zaak.behandelaar }}</td>
            <td>
              <time-or-onbekend :date="zaak.startdatum" />
            </td>
            <td class="link">
              <router-link
                :to="`/zaken/${zaak.url.split('/').pop()}?zaaksysteemId=${zaak.zaaksysteemId ? encodeURIComponent(zaak.zaaksysteemId) : ''}`"
                :title="`Details ${zaak.identificatie}`"
              ></router-link>
            </td>
          </tr>
        </tbody>
      </template>
    </table>
  </div>
</template>

<script lang="ts" setup>
import type { Vraag } from "@/stores/contactmoment/index";
import TimeOrOnbekend from "./components/TimeOrOnbekend.vue";
import type { ZaakDetails } from "./types";

defineProps<{
  zaken: ZaakDetails[];
  vraag: Vraag | undefined;
}>();
</script>
