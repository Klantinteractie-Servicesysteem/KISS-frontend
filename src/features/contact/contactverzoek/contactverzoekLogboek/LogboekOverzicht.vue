<template>
  <div v-if="logboekData?.results.length">
    logboek {{ logboekData.results }}
  </div>
</template>

<script lang="ts" setup>
import { fetchLoggedIn, parseJson, throwIfNotOk } from "@/services";
import { toast } from "@/stores/toast";
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
  await fetchLoggedIn(logboekUrl)
    .then(throwIfNotOk)
    .then(parseJson)
    .then((r) => {
      logboekData.value = r;
    })
    .catch(() =>
      toast({
        text: "Er is een fout opgetreden bij het ophalen van het contactverzoek logboek. Probeer het later opnieuw.",
        type: "error",
      }),
    );
});
</script>
