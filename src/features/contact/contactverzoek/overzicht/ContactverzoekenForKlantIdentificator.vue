<template>
  <contactverzoeken-overzicht
    v-if="contactverzoeken"
    :contactverzoeken="contactverzoeken"
  >
    <template v-for="(_, slotName) in $slots" #[slotName]="props">
      <slot :name="slotName" v-bind="props"></slot>
    </template>
  </contactverzoeken-overzicht>
</template>

<script lang="ts" setup>
import { useLoader } from "@/services/use-loader";
import { watchEffect } from "vue";
import ContactverzoekenOverzicht from "./ContactverzoekenOverzicht.vue";
import { fetchContactverzoekenByKlantIdentificator } from "./service";
import type { ContactverzoekOverzichtItem } from "./types";
import type { KlantIdentificator } from "../../types";
import { useSystemen } from "@/services/environment/fetch-systemen";

defineSlots();

const props = defineProps<{
  klantIdentificator: KlantIdentificator;
}>();

const emit = defineEmits<{
  load: [data: ContactverzoekOverzichtItem[]];
  loading: [data: boolean];
  error: [data: boolean];
}>();

const { systemen } = useSystemen();

const {
  data: contactverzoeken,
  loading,
  error,
} = useLoader(() => {
  if (props.klantIdentificator && systemen.value)
    return fetchContactverzoekenByKlantIdentificator(
      props.klantIdentificator,
      systemen.value,
    );
});

watchEffect(
  () => contactverzoeken.value && emit("load", contactverzoeken.value),
);
watchEffect(() => emit("loading", loading.value));
watchEffect(() => emit("error", error.value));
</script>
