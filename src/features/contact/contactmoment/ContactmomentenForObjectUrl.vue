<template>
  <contactmomenten-overzicht
    v-if="contactmomenten?.page"
    :contactmomenten="contactmomenten.page"
  />
</template>
<script setup lang="ts">
import type { PaginatedResult } from "@/services";
import { useLoader } from "@/services/use-loader";
import { watchEffect } from "vue";
import ContactmomentenOverzicht from "./ContactmomentenOverzicht.vue";
import { useSystemen } from "@/services/environment/fetch-systemen";
import { fetchContactmomentenByObjectUrl } from "./fetch-contactmomenten-by-object-url";
import type { ContactmomentViewModel } from "../types";

const props = defineProps<{
  objectUrl: string;
}>();

const emit = defineEmits<{
  load: [data: PaginatedResult<ContactmomentViewModel>];
  loading: [data: boolean];
  error: [data: boolean];
}>();

const { defaultSysteem } = useSystemen();

const {
  data: contactmomenten,
  loading,
  error,
} = useLoader(() => {
  if (props.objectUrl && defaultSysteem.value)
    return fetchContactmomentenByObjectUrl(
      defaultSysteem.value,
      props.objectUrl,
    );
});

watchEffect(() => contactmomenten.value && emit("load", contactmomenten.value));
watchEffect(() => emit("loading", loading.value));
watchEffect(() => emit("error", error.value));
</script>
