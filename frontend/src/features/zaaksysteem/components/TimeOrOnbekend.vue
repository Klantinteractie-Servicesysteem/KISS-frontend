<template>
  <dutch-date :date="date" v-if="date" />
  <span v-else>Onbekend</span>
</template>
<script setup lang="ts">
import DutchDate from "@/components/DutchDate.vue";

defineProps<{ date: Date | undefined }>();
</script>
