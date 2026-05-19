<template>
  <time :datetime="date.toISOString()">{{ formatTimeOnly(date) }}</time>
</template>
<script setup lang="ts">
import { formatTimeOnly } from "@/helpers/date";
defineProps<{ date: Date }>();
</script>
