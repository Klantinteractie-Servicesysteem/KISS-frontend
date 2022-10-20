<template><slot :enriched="result"></slot></template>
<script setup lang="ts">
import { mapServiceData, ServiceResult } from "@/services";
import { computed, reactive } from "vue";
import type {
  BedrijfHandelsregister,
  BedrijfKlant,
  EnrichedBedrijf,
} from "../types";
import { useEnrichedBedrijf } from "./bedrijf-enricher";

const props = defineProps<{ record: BedrijfHandelsregister | BedrijfKlant }>();
const [_, klantData, handelsregisterData] = useEnrichedBedrijf(
  () => props.record
);

const klantEmail = mapServiceData(klantData, (k) =>
  k?.emails?.map(({ email }) => email).find(Boolean)
);
const klantTelefoon = mapServiceData(klantData, (k) =>
  k?.telefoonnummers?.map(({ telefoonnummer }) => telefoonnummer).find(Boolean)
);
const handelsEmail = mapServiceData(handelsregisterData, (h) => h?.email);
const handelsTelefoon = mapServiceData(handelsregisterData, (h) => h?.email);

const email = computed(() => {
  if (handelsEmail.success && handelsEmail.data) return handelsEmail;
  if (klantEmail.success && klantEmail.data) return klantEmail;
  if (handelsEmail.loading || klantEmail.loading)
    return ServiceResult.loading();
  return handelsEmail;
});

const telefoonnummer = computed(() => {
  if (handelsTelefoon.success && handelsTelefoon.data) return handelsEmail;
  if (klantTelefoon.success && klantTelefoon.data) return klantEmail;
  if (klantTelefoon.loading || handelsTelefoon.loading)
    return ServiceResult.loading();
  return klantTelefoon;
});

const result: EnrichedBedrijf = reactive({
  bedrijfsnaam: mapServiceData(klantData, (k) => k?.bedrijfsnaam ?? ""),
  kvknummer: mapServiceData(handelsregisterData, (h) => h?.kvknummer ?? ""),
  postcodeHuisnummer: mapServiceData(handelsregisterData, (h) =>
    [h?.postcode, h?.huisnummer].join(" ")
  ),
  email,
  telefoonnummer,
});
</script>
