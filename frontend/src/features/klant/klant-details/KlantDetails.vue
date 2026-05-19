<template>
  <article
    class="details-block"
    v-if="klant && klant.emailadressen?.length && klant.telefoonnummers?.length"
  >
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
        <ul v-else-if="klant.emailadressen">
          <li>
            {{ klant.emailadressen[0] }}
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
        <ul v-else-if="klant.telefoonnummers">
          <li>
            {{ klant.telefoonnummers[0] }}
          </li>
        </ul>
      </dd>
    </dl>
  </article>
</template>

<script lang="ts" setup>
import { ref, watchEffect, type PropType } from "vue";
import { Heading as UtrechtHeading } from "@utrecht/component-library-vue";
import {
  useContactmomentStore,
  type ContactmomentKlant,
} from "@/stores/contactmoment";

const store = useContactmomentStore();
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

const klant = ref<ContactmomentKlant | undefined>(undefined);
const error = ref<boolean>(false);

const emit = defineEmits<{
  error: [data: boolean];
  load: [];
  noData: [];
}>();

watchEffect(() => {
  try {
    klant.value = store.getKlantByInternalId(props.internalKlantId);
    emit("load");
  } catch {
    error.value = true;
  }
});

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
