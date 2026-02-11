<template>
  <contactmomenten-overzicht
    v-if="contactmomenten?.page"
    :contactmomenten="contactmomenten.page"
  />
</template>
<script setup lang="ts">
import type { PaginatedResult } from "@/services";
import { useLoader } from "@/services/use-loader";
import { computed, watchEffect } from "vue";
import ContactmomentenOverzicht from "../../../../../../components/contactmomentenOverzicht/ContactmomentenOverzicht.vue";
import { useSystemen } from "@/services/environment/fetch-systemen";
import { fetchContactmomentenByObjectUrl } from "./fetch-contactmomenten-by-object-url";
import type { ContactmomentViewModel } from "@/features/contact/types";

const props = defineProps<{
  objectUrl: string;
  systeemId: string;
}>();

const emit = defineEmits<{
  load: [data: PaginatedResult<ContactmomentViewModel>];
  loading: [data: boolean];
  error: [data: boolean];
}>();

const { systemen } = useSystemen();
const systeem = computed(() =>
  systemen.value?.find(({ identifier }) => identifier === props.systeemId),
);

const {
  data: contactmomenten,
  loading,
  error,
} = useLoader(() => {
  if (props.objectUrl && systeem.value)
    return fetchContactmomentenByObjectUrl(systeem.value, props.objectUrl);
});

watchEffect(() => contactmomenten.value && emit("load", contactmomenten.value));
watchEffect(() => emit("loading", loading.value));
watchEffect(() => emit("error", error.value));
</script>
