<template>
  <utrecht-heading :level="1">Kanalen</utrecht-heading>
  <kanalen-overzicht></kanalen-overzicht>
  <menu>
    <router-link
      to="/Beheer/kanaal/"
      title="toevoegen"
      class="utrecht-button utrecht-button--primary-action icon icon-after plus icon-only"
    >
    </router-link>
  </menu>
</template>

<script setup lang="ts">
import { Heading as UtrechtHeading } from "@utrecht/component-library-vue";
import { KanalenOverzicht } from "../../../features/beheer/Kanalen";
</script>

<style scoped lang="scss">
li h2 {
  margin-block-start: var(--spacing-large);
}
</style>
