<template>
  <utrecht-heading :level="2" modelValue>Algemeen</utrecht-heading>
  <dl>
    <dt>Titel</dt>
    <dd>{{ zaak.omschrijving }}</dd>
    <dt>Zaaktype</dt>
    <dd>{{ zaak.zaaktypeOmschrijving }}</dd>
    <dt>Zaaknummer</dt>
    <dd>{{ zaak.identificatie }}</dd>
    <dt>Indiendatum</dt>
    <dd>
      <time-or-onbekend class="value" :date="zaak.startdatum" />
    </dd>
    <dt>Aanvrager</dt>
    <dd>{{ zaak.aanvrager }}</dd>
    <dt>Status</dt>
    <dd>{{ zaak.status }}</dd>
    <dt>Behandelaar</dt>
    <dd>{{ zaak.behandelaar }}</dd>
    <dt>Toelichting</dt>
    <dd>{{ zaak.toelichting }}</dd>
  </dl>
</template>

<script setup lang="ts">
import type { ZaakDetails } from "../types";
import { Heading as UtrechtHeading } from "@utrecht/component-library-vue";
import TimeOrOnbekend from "./TimeOrOnbekend.vue";

defineProps<{
  zaak: ZaakDetails;
}>();
</script>

<style scoped lang="scss">
dl {
  margin-block-start: var(--line-height-default);
  columns: 2;
}

dd {
  break-before: avoid;
}

dt:not(:first-of-type) {
  margin-block-start: var(--line-height-default);
}
</style>
