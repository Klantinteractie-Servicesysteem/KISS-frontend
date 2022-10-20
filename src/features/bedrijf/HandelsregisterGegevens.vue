<template>
  <article class="details-block">
    <utrecht-heading model-value :level="2">
      Gegevens Handelsregister</utrecht-heading
    >
    <dl>
      <dt>KvK-nummer</dt>
      <dd>{{ bedrijf.kvknummer }}</dd>
      <dt>Vestigingsnummer</dt>
      <dd>
        {{ bedrijf.vestigingsnummer }}
      </dd>
      <dt>Postcode + huisnummer</dt>
      <dd>
        {{ [bedrijf.postcode, bedrijf.huisnummer].filter(Boolean).join(" ") }}
      </dd>
      <dt>E-mailadres</dt>
      <dd>{{ bedrijf.email }}</dd>
      <dt>Telefoonnummer</dt>
      <dd>{{ bedrijf.telefoonnummer }}</dd>
    </dl>
  </article>
</template>

<script setup lang="ts">
import type { BedrijfHandelsregister } from "./types";
import { UtrechtHeading } from "@utrecht/web-component-library-vue";

defineProps<{ bedrijf: BedrijfHandelsregister }>();
</script>
