<template>
  <section>
    <expandable-table-list :items="contactmomenten" item-key="url">
      <template #header>
        <span id="datum-header" class="icon-after sort-descending">Datum</span>
        <span id="medewerker-header">Medewerker</span>
        <span id="kanaal-header">Kanaal</span>
        <span id="gespreksresultaat-header">Gespreksresultaat</span>
        <span id="afdeling-header">Afdeling</span>
      </template>

      <template v-slot:item="{ item }">
        <ContactmomentenOverzichtItem :contactmoment="item" />
      </template>
    </expandable-table-list>
  </section>
</template>

<script lang="ts" setup>
import type { ContactmomentViewModel } from "../types";
import ContactmomentenOverzichtItem from "./ContactmomentenOverzichtItem.vue";
import ExpandableTableList from "@/components/ExpandableTableList.vue";

defineProps<{
  contactmomenten: ContactmomentViewModel[];
}>();
</script>
