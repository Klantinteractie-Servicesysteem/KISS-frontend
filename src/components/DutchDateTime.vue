<template>
  <time :datetime="iso">{{ formatted }}</time>
</template>
<script setup lang="ts">
import { formatDateAndTime } from "@/helpers/date";
import { computed } from "vue";
const props = defineProps<{ date: Date | string }>();
const date = computed(() => new Date(props.date));
const iso = computed(() => date.value.toISOString());
const formatted = computed(() => formatDateAndTime(date.value));
</script>
