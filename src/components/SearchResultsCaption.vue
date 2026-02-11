<template>
  <caption>
    <p v-if="resultCount === 0">
      Geen resultaten gevonden {{ zoekTermenCaption }}
    </p>
    <p v-else-if="resultCount === 1">
      1 resultaat gevonden {{ zoekTermenCaption }}
    </p>
    <p v-else>{{ resultCount }} resultaten gevonden {{ zoekTermenCaption }}</p>
  </caption>
</template>

<script setup lang="ts">
import type { BedrijvenQuery } from "@/features/contact/contactBehandelen/vraagBehandelen/klantgerelateerdeGegevensInzien/bedrijvenEnPersonen/bedrijven/bedrijf-zoeken/use-search-bedrijven";
import { formatDateOnly } from "@/helpers/date";
import type { Paginated, PaginatedResult } from "@/services";
import type { PersoonQuery } from "@/services/brp";
import { computed } from "vue";

const props = defineProps<{
  results: Paginated<unknown> | PaginatedResult<unknown> | unknown[];
  zoekTermen?: PersoonQuery | BedrijvenQuery;
}>();

const zoekTermenCaption = computed(() => {
  if (!props.zoekTermen) {
    return "";
  }

  if ("bsn" in props.zoekTermen) {
    return `voor '${props.zoekTermen.bsn}'.`;
  } else if ("geslachtsnaamGeboortedatum" in props.zoekTermen) {
    const { geboortedatum, geslachtsnaam } =
      props.zoekTermen.geslachtsnaamGeboortedatum;
    return `voor '${geslachtsnaam}, ${formatDateOnly(geboortedatum)}'.`;
  } else if ("postcodeHuisnummer" in props.zoekTermen) {
    const { postcode, huisnummer, huisletter, toevoeging } =
      props.zoekTermen.postcodeHuisnummer;
    return `voor '${postcode.numbers}${postcode.letters}, ${huisnummer}${huisletter ? `, ${huisletter}` : ""}${toevoeging ? `, ${toevoeging}` : ""}'.`;
  } else if ("postcodeHuisnummerAchternaam" in props.zoekTermen) {
    const { postcode, huisnummer, huisletter, toevoeging, achternaam } =
      props.zoekTermen.postcodeHuisnummerAchternaam;
    return `voor '${postcode.numbers}${postcode.letters}, ${huisnummer}${huisletter ? `, ${huisletter}` : ""}${toevoeging ? `, ${toevoeging}` : ""}${achternaam ? `, ${achternaam}` : ""}'.`;
  } else if ("handelsnaam" in props.zoekTermen) {
    return `voor '${props.zoekTermen.handelsnaam}'.`;
  } else if ("kvkNummer" in props.zoekTermen) {
    return `voor '${props.zoekTermen.kvkNummer}'.`;
  } else if ("vestigingsnummer" in props.zoekTermen) {
    return `voor '${props.zoekTermen.vestigingsnummer}'.`;
  } else {
    return "";
  }
});

const resultCount = computed(() => {
  if (
    "totalRecords" in props.results &&
    typeof props.results.totalRecords === "number"
  )
    return props.results.totalRecords;
  if ("count" in props.results && typeof props.results.count === "number")
    return props.results.count;
  if ("page" in props.results) return props.results.page.length;
  return props.results.length;
});
</script>

<style lang="scss" scoped>
caption {
  text-align: left;
  margin-block-end: var(--spacing-default);

  > * {
    &:first-child {
      border-bottom: 1px solid var(--color-tertiary);
      padding-block-end: var(--spacing-small);
    }

    &:last-child {
      margin-block-start: var(--spacing-small);
    }
  }
}
</style>
