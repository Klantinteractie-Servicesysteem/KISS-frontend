<template>
  <article class="details-block">
    <header class="heading-container">
      <utrecht-heading :level="level">
        <span class="heading">Contactgegevens</span>
      </utrecht-heading>
    </header>
    <dl>
      <dt>E-mailadressen</dt>
      <dd>
        <ul v-if="klant.emailadressen.length">
          <li v-for="(email, idx) in klant.emailadressen" :key="idx">
            {{ email }}
          </li>
        </ul>
      </dd>
      <dt>Telefoonnummers</dt>
      <dd>
        <ul v-if="klant.telefoonnummers.length">
          <li v-for="(telefoon, idx) in klant.telefoonnummers" :key="idx">
            {{ telefoon }}
          </li>
        </ul>
      </dd>
    </dl>
  </article>
</template>

<script lang="ts" setup>
import { type PropType } from "vue";
import { Heading as UtrechtHeading } from "@utrecht/component-library-vue";
import type { Klant } from "@/services/openklant2";

defineProps({
  klant: {
    type: Object as PropType<Klant>,
    required: true,
  },
  level: {
    type: Number as PropType<1 | 2 | 3 | 4 | 5>,
    default: 2,
  },
});
</script>

<style lang="scss" scoped>
.heading-container {
  display: flex;
  align-items: center;
  justify-content: space-between;

  .heading {
    display: flex;
    align-items: center;
    gap: var(--spacing-small);
  }
}
</style>
