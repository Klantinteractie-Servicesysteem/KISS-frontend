<template>
  <article class="details-block" v-if="bedrijf">
    <utrecht-heading :level="2">Gegevens Handelsregister</utrecht-heading>
    <dl>
      <dt>Bedrijfsnaam</dt>
      <dd>{{ bedrijf.bedrijfsnaam }}</dd>
      <dt>KvK-nummer</dt>
      <dd>{{ bedrijf.kvkNummer }}</dd>
      <dt>Vestigingsnummer</dt>
      <dd>
        {{ bedrijf.vestigingsnummer }}
      </dd>
      <dt>Adres</dt>
      <div>
        <dd>
          {{
            [
              bedrijf.straatnaam,
              bedrijf.huisnummer,
              bedrijf.huisletter,
              bedrijf.huisnummertoevoeging,
            ]
              .filter(Boolean)
              .join(" ")
          }}
        </dd>
        <dd>{{ bedrijf.postcode }} {{ bedrijf.woonplaats }}</dd>
      </div>
    </dl>
  </article>
</template>

<script setup lang="ts">
import { enforceOneOrZero, useLoader } from "@/services";
import {
  searchBedrijvenInHandelsRegisterByKvkNummer,
  searchBedrijvenInHandelsRegisterByVestigingEnKvkNummer,
  type Bedrijf,
} from "@/services/kvk";
import { Heading as UtrechtHeading } from "@utrecht/component-library-vue";
import { watchEffect } from "vue";
import { useContactmomentStore } from "@/stores/contactmoment";

const props = defineProps<{ internalKlantId: string }>();

const store = useContactmomentStore();

const {
  data: bedrijf,
  loading,
  error,
} = useLoader(() => {
  const klant = store.getKlantByInternalId(props.internalKlantId);
  if (klant) {
    if (klant.vestigingsnummer && klant.kvkNummer) {
      return searchBedrijvenInHandelsRegisterByVestigingEnKvkNummer(
        klant.vestigingsnummer,
        klant.kvkNummer,
      ).then(enforceOneOrZero);
    }
    if (klant.kvkNummer) {
      return searchBedrijvenInHandelsRegisterByKvkNummer(klant.kvkNummer).then(
        enforceOneOrZero,
      );
    }
  }
});

const emit = defineEmits<{
  load: [data: Bedrijf];
  loading: [data: boolean];
  error: [data: boolean];
}>();

watchEffect(() => bedrijf.value && emit("load", bedrijf.value));
watchEffect(() => emit("loading", loading.value));
watchEffect(() => emit("error", error.value));
</script>
