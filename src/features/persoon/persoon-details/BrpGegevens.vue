<template>
  <article class="details-block" v-if="persoon">
    <utrecht-heading :level="2"> Gegevens BRP</utrecht-heading>
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
      <dt>Geboortedatum</dt>
      <dd>
        <dutch-date
          v-if="persoon.geboortedatum"
          :date="persoon.geboortedatum"
        />
      </dd>
      <dt>Geboorteplaats</dt>
      <dd>{{ persoon.geboorteplaats }}</dd>
      <dt>Geboorteland</dt>
      <dd>{{ persoon.geboorteland }}</dd>
      <template v-if="persoon.adresregel1">
        <dt>Adresregel 1</dt>
        <dd>{{ persoon.adresregel1 }}</dd>
      </template>
      <template v-if="persoon.adresregel2">
        <dt>Adresregel 2</dt>
        <dd>{{ persoon.adresregel2 }}</dd>
      </template>
      <template v-if="persoon.adresregel3">
        <dt>Adresregel 3</dt>
        <dd>{{ persoon.adresregel3 }}</dd>
      </template>
    </dl>
  </article>
</template>

<script setup lang="ts">
import { searchPersonen, type Persoon } from "@/services/brp";
import { Heading as UtrechtHeading } from "@utrecht/component-library-vue";
import DutchDate from "@/components/DutchDate.vue";
import { enforceOneOrZero, useLoader } from "@/services";
import { watchEffect } from "vue";
import {
  useContactmomentStore,
  type ContactmomentKlant,
} from "@/stores/contactmoment";

const props = defineProps({
  bsn: { type: String, required: false },
  internalKlantId: { type: String, required: false },
});

const store = useContactmomentStore();

const {
  data: persoon,
  loading,
  error,
} = useLoader(() => {
  // if (props.bsn) {
  //   return searchPersonen({ bsn: props.bsn }).then(enforceOneOrZero);
  // }
  if (props.internalKlantId) {
    const klant = getPersoonFromStoreByInternalId(props.internalKlantId);
    if (klant && klant.bsn) {
      return searchPersonen({ bsn: klant.bsn }).then(enforceOneOrZero);
    }
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

function getPersoonFromStoreByInternalId(
  internalKlantId: string,
): ContactmomentKlant | undefined {
  const x = store.huidigContactmoment?.huidigeVraag.klanten?.find(
    (x) => x.klant.internalId == internalKlantId,
  );
  return x?.klant;
}
</script>
