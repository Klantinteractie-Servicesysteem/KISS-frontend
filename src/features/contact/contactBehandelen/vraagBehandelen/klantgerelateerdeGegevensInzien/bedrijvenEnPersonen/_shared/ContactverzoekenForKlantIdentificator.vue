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
import ContactverzoekenOverzicht from "@/components/contactverzoekenOverzicht/ContactverzoekenOverzicht.vue";

import { useSystemen } from "@/services/environment/fetch-systemen";
import type { KlantIdentificator } from "@/services/openklant/types";

import type { ContactverzoekOverzichtItem } from "../../contactverzoeken/types";
import { fetchContactverzoekenByKlantIdentificator } from "../../service";

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
