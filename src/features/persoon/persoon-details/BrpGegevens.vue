<template>
  <article class="details-block" v-if="persoon">
    <utrecht-heading :level="2">BRP Gegevens</utrecht-heading>
    <p class="attention">
      Onderstaande gegevens mogen alleen worden gebruikt ter controle van de
      identiteit van de inwoner. Verstrek nooit de hier getoonde gegevens.
    </p>
    <dl>
      <dt>Naam</dt>
      <dd>
        {{
          [persoon.voornaam, persoon.voorvoegselAchternaam, persoon.achternaam]
            .filter(Boolean)
            .join(" ")
        }}
      </dd>
      <dt>Bsn</dt>
      <dd>{{ persoon.bsn }}</dd>
      <dt>Adres</dt>
      <div>
        <dd>{{ persoon.adresregel1 }}</dd>
        <dd>{{ persoon.adresregel2 }}</dd>
        <dd>{{ persoon.adresregel3 }}</dd>
      </div>
      <dt>Geboortedatum</dt>
      <dd>
        <dutch-date
          v-if="persoon.geboortedatum"
          :date="persoon.geboortedatum"
        />
      </dd>
      <dt>Geboorteplaats</dt>
      <dd>{{ persoon.geboorteplaats }}, {{ persoon.geboorteland }}</dd>
    </dl>
  </article>
</template>

<script setup lang="ts">
import { searchPersonen, type Persoon } from "@/services/brp";
import { Heading as UtrechtHeading } from "@utrecht/component-library-vue";
import DutchDate from "@/components/DutchDate.vue";
import { enforceOneOrZero, useLoader } from "@/services";
import { watchEffect } from "vue";
import { useContactmomentStore } from "@/stores/contactmoment";

const props = defineProps({
  internalKlantId: { type: String, required: true },
});

const store = useContactmomentStore();

const {
  data: persoon,
  loading,
  error,
} = useLoader(() => {
  const klant = store.getKlantByInternalId(props.internalKlantId);
  if (klant && klant.bsn) {
    return searchPersonen({ bsn: klant.bsn }).then(enforceOneOrZero);
  }
});

const emit = defineEmits<{
  load: [data: Persoon];
  loading: [data: boolean];
  error: [data: boolean];
}>();

watchEffect(() => persoon.value && emit("load", persoon.value));
watchEffect(() => emit("loading", loading.value));
watchEffect(() => emit("error", error.value));
</script>

<style>
.attention {
  background-color: var(--color-attention-background);
  border: 3px solid var(--color-attention);
  padding: 1em;
  color: var(--color-attention);
}
</style>
