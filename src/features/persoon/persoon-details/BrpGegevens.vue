<template>
  <article class="details-block">
    <utrecht-heading :level="2">BRP Gegevens</utrecht-heading>
    <application-message
      v-if="error"
      messageType="error"
      :message="'Er is een fout opgetreden'"
    />
    <utrecht-paragraph class="attention" v-if="persoon">
      Onderstaande gegevens mogen alleen worden gebruikt ter controle van de
      identiteit van de inwoner.<br />
      Verstrek nooit de hier getoonde gegevens.
    </utrecht-paragraph>
    <dl v-if="persoon">
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
import {
  Heading as UtrechtHeading,
  Paragraph as UtrechtParagraph,
} from "@utrecht/component-library-vue";
import DutchDate from "@/components/DutchDate.vue";
import { enforceOneOrZero, useLoader } from "@/services";
import { watchEffect } from "vue";
import { useContactmomentStore } from "@/stores/contactmoment";
import ApplicationMessage from "@/components/ApplicationMessage.vue";

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

<style lang="scss" scoped>
.attention {
  padding: var(--spacing-default);
  border: 1px solid var(--color-attention);
  outline-style: solid;
  color: var(--color-attention);
  background-color: var(--color-attention-background);
}
</style>
