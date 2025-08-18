<template>
  <time v-if="validated" :datetime="formatIsoDateTime(validated)">{{
    formatNlDateTime(validated)
  }}</time>
  <template v-else>N.v.t</template>
</template>

<script setup lang="ts">
import { computed } from "vue";

export type DateLike = string | number | Date | null | undefined;

const parseValidDate = (date: DateLike) => {
  if (!date) return undefined;
  date = new Date(date);
  const time = date.getTime();
  if (date instanceof Date && !isNaN(time)) return date;
  return undefined;
};

const formatNlDateTime = (date: string | number | Date | null | undefined) => {
  date = parseValidDate(date);
  if (!date) return undefined;
  const dateStr = date.toLocaleString("nl-NL", {
    day: "2-digit",
    month: "2-digit",
    year: "numeric",
  });
  const time = date.toLocaleString("nl-NL", {
    hour: "2-digit",
    minute: "2-digit",
  });
  return `${dateStr} ${time}`;
};

const formatIsoDateTime = (date: DateLike) => {
  return parseValidDate(date)?.toISOString();
};

const props = defineProps<{ date: DateLike }>();
const validated = computed(() => parseValidDate(props.date));
</script>
