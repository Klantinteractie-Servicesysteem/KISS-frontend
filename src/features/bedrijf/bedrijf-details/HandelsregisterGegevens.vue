<template>
  <article class="details-block">
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
import type { Bedrijf } from "@/services/kvk";
import { Heading as UtrechtHeading } from "@utrecht/component-library-vue";

defineProps<{ bedrijf: Bedrijf }>();
</script>
