<template>
  <article class="klant-panel">
    <utrecht-heading :level="1">Personen</utrecht-heading>
    <persoon-zoeker />
  </article>
</template>

<script setup lang="ts">
import { Heading as UtrechtHeading } from "@utrecht/component-library-vue";
import { PersoonZoeker } from "@/features/contact/contactBehandelen/vraagBehandelen/klantgerelateerdeGegevensInzien/bedrijvenEnPersonen/personen/persoon-zoeken";
</script>
