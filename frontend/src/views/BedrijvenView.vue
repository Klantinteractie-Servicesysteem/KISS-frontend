<template>
  <section class="klant-panel">
    <utrecht-heading :level="1">Bedrijven</utrecht-heading>
    <bedrijf-zoeker />
  </section>
</template>

<script setup lang="ts">
import { BedrijfZoeker } from "@/features/bedrijf/bedrijf-zoeken";
import { Heading as UtrechtHeading } from "@utrecht/component-library-vue";
</script>
