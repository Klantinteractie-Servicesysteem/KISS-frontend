<template><slot :enriched="result"></slot></template>
<script setup lang="ts">
import { mapServiceData, ServiceResult, type ServiceData } from "@/services";
import { computed, reactive } from "vue";
import { ensureKlantForBsn } from "../service";
import type { EnrichedPersoon, Persoon } from "../types";
import { useRouter } from "vue-router";
import { useEnrichedPersoon } from "./persoon-enricher";
import type { Klant } from "@/features/shared/types";

const props = defineProps<{ record: Klant | Persoon }>();

const [bsn, klantData, persoonData] = useEnrichedPersoon(() => props.record);

const getKlantUrl = (klant: Klant) => `/klanten/${klant.id}`;

function mapNaam(klantOrPersoon: Klant | Persoon | null) {
  return (
    klantOrPersoon &&
    [
      klantOrPersoon.voornaam,
      klantOrPersoon.voorvoegselAchternaam,
      klantOrPersoon.achternaam,
    ]
      .filter(Boolean)
      .join(" ")
  );
}

function mapTelefoonnummers(klant: Klant | null) {
  return (
    klant &&
    klant.telefoonnummers
      .map(({ telefoonnummer }) => telefoonnummer)
      .filter(Boolean)
      .join(", ")
  );
}

function mapEmails(klant: Klant | null) {
  return (
    klant &&
    klant.emails
      .map(({ email }) => email)
      .filter(Boolean)
      .join(", ")
  );
}

function mapLink(klant: Klant | null, naam: string | null) {
  return (
    klant && {
      to: getKlantUrl(klant),
      title: `Details ${naam}`,
    }
  );
}

function mapGeboortedatum(persoon: Persoon | null) {
  return persoon?.geboortedatum ?? null;
}

function mapPostcodeHuisnummer(persoon: Persoon | null) {
  return (
    persoon && [persoon.postcode, persoon.huisnummer].filter(Boolean).join(" ")
  );
}

const router = useRouter();

const create = async () => {
  if (!bsn.value) throw new Error();
  const newKlant = await ensureKlantForBsn(bsn.value);
  const url = getKlantUrl(newKlant);
  router.push(url);
};

const klantNaam = mapServiceData(klantData, mapNaam);
const persoonNaam = mapServiceData(persoonData, mapNaam);

const naam = computed<ServiceData<string | null>>(() => {
  if (klantNaam.loading && persoonNaam.loading) return klantNaam;
  if (klantNaam.success && klantNaam.data) return klantNaam;
  if (persoonNaam.success && persoonNaam.data) return persoonNaam;
  return ServiceResult.success(null);
});

const telefoonnummers = mapServiceData(klantData, mapTelefoonnummers);

const emails = mapServiceData(klantData, mapEmails);

const geboortedatum = mapServiceData(persoonData, mapGeboortedatum);

const postcodeHuisnummer = mapServiceData(persoonData, mapPostcodeHuisnummer);

const detailLink = computed(() => {
  const n = naam.value.success ? naam.value.data : null;
  return mapServiceData(klantData, (k) => mapLink(k, n));
});

const result: EnrichedPersoon = reactive({
  naam,
  bsn,
  telefoonnummers,
  emails,
  geboortedatum,
  postcodeHuisnummer,
  create,
  detailLink,
});
</script>
