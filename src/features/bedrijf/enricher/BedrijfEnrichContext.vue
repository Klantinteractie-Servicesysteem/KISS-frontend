<template><slot :enriched="result"></slot></template>
<script setup lang="ts">
import { mapServiceData, ServiceResult } from "@/services";
import { computed, reactive } from "vue";
import { useRouter } from "vue-router";
import { ensureKlantForVestigingsnummer } from "../service";
import type {
  BedrijfHandelsregister,
  BedrijfKlant,
  EnrichedBedrijf,
} from "../types";
import { useEnrichedBedrijf } from "./bedrijf-enricher";

const props = defineProps<{ record: BedrijfHandelsregister | BedrijfKlant }>();
const [vestigingsnummer, klantData, handelsregisterData] = useEnrichedBedrijf(
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

const getKlantUrl = (klant: BedrijfKlant) => `/klanten/${klant.id}`;

function mapLink(klant: BedrijfKlant | null, naam: string | null) {
  return (
    klant && {
      to: getKlantUrl(klant),
      title: `Details ${naam}`,
    }
  );
}

const detailLink = computed(() => {
  const n = klantData.success ? klantData.data?.bedrijfsnaam : null;
  return mapServiceData(klantData, (k) => mapLink(k, n ?? null));
});

const router = useRouter();

const create = async () => {
  if (!vestigingsnummer.value) throw new Error();
  const newKlant = await ensureKlantForVestigingsnummer(vestigingsnummer.value);
  const url = getKlantUrl(newKlant);
  router.push(url);
};

const result: EnrichedBedrijf = reactive({
  bedrijfsnaam: mapServiceData(klantData, (k) => k?.bedrijfsnaam ?? ""),
  kvknummer: mapServiceData(handelsregisterData, (h) => h?.kvknummer ?? ""),
  postcodeHuisnummer: mapServiceData(handelsregisterData, (h) =>
    [h?.postcode, h?.huisnummer].join(" ")
  ),
  email,
  telefoonnummer,
  detailLink,
  create,
});
</script>
