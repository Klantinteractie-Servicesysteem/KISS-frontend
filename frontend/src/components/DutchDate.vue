<template>
  <time :datetime="formatIsoDate(date)">{{ formatDateOnly(date) }}</time>
</template>
<script setup lang="ts">
import { formatDateOnly, formatIsoDate } from "@/helpers/date";
defineProps<{ date: Date }>();
</script>
