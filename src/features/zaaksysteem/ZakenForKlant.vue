<template>
  <zaken-overzicht
    v-if="zaken"
    :zaken="zaken"
    :vraag="contactmomentStore.huidigContactmoment?.huidigeVraag"
  />
</template>

<script setup lang="ts">
import { useLoader } from "@/services";
import type { Systeem } from "@/services/environment/fetch-systemen";
import { watchEffect } from "vue";
import type { ZaakDetails } from "./types";
import { fetchZakenByBsn } from "./service";
import { useContactmomentStore } from "@/stores/contactmoment";
import ZakenOverzicht from "./ZakenOverzicht.vue";
import type { Persoon } from "@/services/brp";
import type { Bedrijf } from "@/services/kvk";

const props = defineProps<{
  klantIdentificator: Persoon | Bedrijf;
  systemen: Systeem[];
}>();

const emit = defineEmits<{
  load: [data: ZaakDetails[]];
  loading: [data: boolean];
  error: [data: boolean];
}>();

const contactmomentStore = useContactmomentStore();

const {
  data: zaken,
  loading,
  error,
} = useLoader(() => {
  if ("bsn" in props.klantIdentificator && props.klantIdentificator.bsn)
    return fetchZakenByBsn(props.systemen, props.klantIdentificator.bsn);

  if (
    "vestigingsnummer" in props.klantIdentificator &&
    props.klantIdentificator.vestigingsnummer
  )
    return;
});

watchEffect(() => zaken.value && emit("load", zaken.value));
watchEffect(() => emit("loading", loading.value));
watchEffect(() => emit("error", error.value));
</script>
