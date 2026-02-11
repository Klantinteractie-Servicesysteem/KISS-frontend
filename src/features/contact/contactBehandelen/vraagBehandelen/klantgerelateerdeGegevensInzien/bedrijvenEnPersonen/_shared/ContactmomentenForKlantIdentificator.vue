<template>
  <contactmomenten-overzicht
    v-if="contactmomenten"
    :contactmomenten="contactmomenten"
  />
</template>
<script setup lang="ts">
import { useLoader } from "@/services/use-loader";
import { watchEffect } from "vue";
import ContactmomentenOverzicht from "../../../../../../../components/contactmomentenOverzicht/ContactmomentenOverzicht.vue";
import { fetchContactmomentenByKlantIdentificator } from "./fetch-contactmomenten-by-klant-identificator";
import { useSystemen } from "@/services/environment/fetch-systemen";
import type { ContactmomentViewModel } from "../../../../../types";
import type { KlantIdentificator } from "@/services/openklant/types";

const props = defineProps<{
  klantIdentificator: KlantIdentificator;
}>();

const emit = defineEmits<{
  load: [data: ContactmomentViewModel[]];
  loading: [data: boolean];
  error: [data: boolean];
}>();

const { systemen } = useSystemen();

const {
  data: contactmomenten,
  loading,
  error,
} = useLoader(() => {
  if (props.klantIdentificator && systemen.value)
    return fetchContactmomentenByKlantIdentificator(
      props.klantIdentificator,
      systemen.value,
    );
});

watchEffect(() => contactmomenten.value && emit("load", contactmomenten.value));
watchEffect(() => emit("loading", loading.value));
watchEffect(() => emit("error", error.value));
</script>
