<template>
  <article
    class="details-block"
    v-if="
      klant && (klant.emailadressen?.length || klant.telefoonnummers?.length)
    "
  >
    <header class="heading-container">
      <utrecht-heading :level="level">
        <span class="heading">Contactgegevens</span>
      </utrecht-heading>
    </header>
    <dl>
      <template v-if="klant.emailadressen?.length">
        <dt>E-mailadressen</dt>
        <dd>
          <ul>
            <li v-for="(email, idx) in klant.emailadressen" :key="idx">
              {{ email }}
            </li>
          </ul>
        </dd>
      </template>
      <template v-if="klant.telefoonnummers?.length">
        <dt>Telefoonnummers</dt>
        <dd>
          <ul>
            <li v-for="(telefoon, idx) in klant.telefoonnummers" :key="idx">
              {{ telefoon }}
            </li>
          </ul>
        </dd>
      </template>
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
