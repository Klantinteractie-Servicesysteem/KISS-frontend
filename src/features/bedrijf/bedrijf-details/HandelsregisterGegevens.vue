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
  findBedrijfInHandelsRegister,
  type Bedrijf,
  type BedrijfIdentifier,
} from "@/services/kvk";
import { Heading as UtrechtHeading } from "@utrecht/component-library-vue";
import { watchEffect } from "vue";

const props = defineProps<{ bedrijfIdentifier: BedrijfIdentifier }>();
const {
  data: bedrijf,
  loading,
  error,
} = useLoader(() => {
  if (props.bedrijfIdentifier)
    return findBedrijfInHandelsRegister(props.bedrijfIdentifier).then(
      enforceOneOrZero,
    );
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
