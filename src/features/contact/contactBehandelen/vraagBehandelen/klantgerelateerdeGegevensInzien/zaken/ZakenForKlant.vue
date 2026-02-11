<template>
  <zaken-overzicht
    v-if="zaken"
    :zaken="zaken"
    :vraag="contactmomentStore.huidigContactmoment?.huidigeVraag"
  />
</template>

<script setup lang="ts">
import { useLoader } from "@/services";
import { useSystemen } from "@/services/environment/fetch-systemen";
import { watchEffect } from "vue";
import type { ZaakDetails } from "./types";
import { fetchZakenByBsn, fetchZakenByKlantBedrijfIdentifier } from "./service";
import { useContactmomentStore } from "@/stores/contactmoment/index";
import ZakenOverzicht from "./ZakenOverzicht.vue";
import type { Persoon } from "@/services/brp";
import type { Bedrijf } from "@/services/kvk";

const props = defineProps<{
  klantIdentificator: Persoon | Bedrijf;
}>();

const emit = defineEmits<{
  load: [data: ZaakDetails[]];
  loading: [data: boolean];
  error: [data: boolean];
}>();

const contactmomentStore = useContactmomentStore();

const { systemen } = useSystemen();

const {
  data: zaken,
  loading,
  error,
} = useLoader(() => {
  if (!systemen.value) return;

  if ("bsn" in props.klantIdentificator && props.klantIdentificator.bsn)
    return fetchZakenByBsn(systemen.value, props.klantIdentificator.bsn);

  if (
    "kvkNummer" in props.klantIdentificator &&
    props.klantIdentificator.kvkNummer
  )
    return fetchZakenByKlantBedrijfIdentifier(
      systemen.value,
      props.klantIdentificator,
    );
});

watchEffect(() => zaken.value && emit("load", zaken.value));
watchEffect(() => emit("loading", loading.value));
watchEffect(() => emit("error", error.value));
</script>
