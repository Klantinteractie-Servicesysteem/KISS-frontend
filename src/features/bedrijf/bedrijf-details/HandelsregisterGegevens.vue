<template>
  <article class="details-block" v-if="bedrijf">
    <utrecht-heading :level="2"> Gegevens Handelsregister</utrecht-heading>
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
      <dt>Postcode</dt>
      <dd>{{ bedrijf.postcode }}</dd>
      <dt>Plaats</dt>
      <dd>{{ bedrijf.woonplaats }}</dd>
    </dl>
  </article>
</template>

<script setup lang="ts">
import { enforceOneOrZero, useLoader } from "@/services";
import {
  searchBedrijvenInHandelsRegister,
  searchBedrijvenInHandelsRegisterByIdentifier,
  type Bedrijf,
  type BedrijfIdentifier,
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
  console.log(props.internalKlantId);
  const klant = store.getKlantByInternalId(props.internalKlantId);
  if (klant && klant.bedrijfIdentifier) {
    console.log(555);

    const mappedBedrijfsIdentifier =
      "vestigingsnummer" in klant.bedrijfIdentifier
        ? { vestigingsnummer: klant.bedrijfIdentifier.vestigingsnummer }
        : { rsin: klant.bedrijfIdentifier.nietNatuurlijkPersoonIdentifier };

    return searchBedrijvenInHandelsRegisterByIdentifier(
      mappedBedrijfsIdentifier,
    ).then(enforceOneOrZero);
  }
  // if (props.bedrijfIdentifier)
  //   return searchBedrijvenInHandelsRegister(props.bedrijfIdentifier).then(
  //     enforceOneOrZero,
  //   );
});

const emit = defineEmits<{
  load: [data: Bedrijf];
  loading: [data: boolean];
  error: [data: boolean];
}>();

watchEffect(() => bedrijf.value && emit("load", bedrijf.value));
watchEffect(() => emit("loading", loading.value));
watchEffect(() => emit("error", error.value));

function getPersoonFromStoreByInternalId(internalKlantId: string) {
  throw new Error("Function not implemented.");
}
</script>
