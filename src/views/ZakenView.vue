<template>
  <section>
    <utrecht-heading :level="1" class="zaak-title"> Zaken </utrecht-heading>
    <zaak-zoeker v-if="systemen" :systemen="systemen" />
  </section>
</template>

<script setup lang="ts">
import { Heading as UtrechtHeading } from "@utrecht/component-library-vue";
import { ZaakZoeker } from "@/features/zaaksysteem";
import { useLoader } from "@/services";
import { fetchSystemen } from "@/services/environment/fetch-systemen";

const { data: systemen } = useLoader(() => fetchSystemen());
</script>

<style scoped lang="scss">
section > *:not(:last-child) {
  margin-block-end: var(--spacing-large);
}
</style>
