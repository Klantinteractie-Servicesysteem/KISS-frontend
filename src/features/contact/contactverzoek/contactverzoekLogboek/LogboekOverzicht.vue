<template>
  <div v-if="logboekData?.results.length">
    logboek {{ logboekData.results }}
  </div>
</template>

<script lang="ts" setup>
import { fetchLoggedIn } from "@/services";
import { ref, watchEffect } from "vue";

const props = defineProps<{
  contactverzoekId: string;
}>();

const logboekData = ref<{
  count: 0;
  next: null;
  previous: null;
  results: [];
} | null>();

watchEffect(async () => {
  const logboekUrl = `/api/logboek/api/v2/objects?data_attr=heeftBetrekkingOp__objectId__exact__${props.contactverzoekId}`;
  logboekData.value = null;
  const response = await fetchLoggedIn(logboekUrl);
  logboekData.value = await response.json();
});
</script>
