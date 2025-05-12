<template>
  <article class="details-block" v-if="klant">
    <header class="heading-container">
      <utrecht-heading :level="level">
        <span class="heading">Contactgegevens</span>
      </utrecht-heading>
    </header>
    <dl>
      <dt>E-mailadressen</dt>
      <dd>
        <ul v-if="klant.emailadressen && klant.emailadressen.length">
          <li v-for="(email, idx) in klant.emailadressen" :key="idx">
            {{ email }}
          </li>
        </ul>
        <ul v-else-if="klant.emailadres">
          <li>
            {{ klant.emailadres }}
          </li>
        </ul>
      </dd>
      <dt>Telefoonnummers</dt>
      <dd>
        <ul v-if="klant.telefoonnummers && klant.telefoonnummers.length">
          <li v-for="(telefoon, idx) in klant.telefoonnummers" :key="idx">
            {{ telefoon }}
          </li>
        </ul>
        <ul v-else-if="klant.telefoonnummer">
          <li>
            {{ klant.telefoonnummer }}
          </li>
        </ul>
      </dd>
    </dl>
  </article>
</template>

<script lang="ts" setup>
import { watchEffect, type PropType } from "vue";
import { Heading as UtrechtHeading } from "@utrecht/component-library-vue";
import type { Klant } from "@/services/openklant/types";
import { useLoader } from "@/services";
import { fetchKlantByInternalId } from "./fetch-klant";
import { useSystemen } from "@/services/environment/fetch-systemen";

const { systemen, defaultSysteem } = useSystemen();

const props = defineProps({
  internalKlantId: {
    type: String,
    required: true,
  },
  level: {
    type: Number as PropType<1 | 2 | 3 | 4 | 5>,
    default: 2,
  },
});

const {
  data: klant,
  loading,
  error,
} = useLoader(() => {
  if (
    !props.internalKlantId ||
    !defaultSysteem.value ||
    !systemen.value?.length
  )
    return;

  return fetchKlantByInternalId({
    internalId: props.internalKlantId,
    systemen: systemen.value,
    defaultSysteem: defaultSysteem.value,
  });
});

const emit = defineEmits<{
  load: [data: Klant];
  loading: [data: boolean];
  error: [data: boolean];
  noData: [];
}>();

watchEffect(() => {
  if (klant.value) {
    emit("load", klant.value);
  }

  if (
    !loading.value &&
    !error.value &&
    (!klant.value ||
      (!klant.value.emailadressen?.length &&
        !klant.value.telefoonnummers?.length))
  ) {
    emit("noData");
  }
});
watchEffect(() => emit("loading", loading.value));
watchEffect(() => emit("error", error.value));
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
