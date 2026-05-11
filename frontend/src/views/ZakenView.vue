<template>
  <section>
    <utrecht-heading :level="1" class="zaak-title"> Zaken </utrecht-heading>
    <zaak-zoeker />
  </section>
</template>

<script setup lang="ts">
import { Heading as UtrechtHeading } from "@utrecht/component-library-vue";
import { ZaakZoeker } from "@/features/zaaksysteem";
</script>

<style scoped lang="scss">
section > *:not(:last-child) {
  margin-block-end: var(--spacing-large);
}
</style>
