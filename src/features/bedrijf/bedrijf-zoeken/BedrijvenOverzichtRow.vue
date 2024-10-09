<template>
  <tr class="row-link">
    <th scope="row" class="wrap">
      <div class="skeleton" v-if="bedrijf.loading" />
      <template v-else-if="bedrijf.success">{{
        bedrijf.data?.bedrijfsnaam
      }}</template>
    </th>
    <td>
      <div class="skeleton" v-if="bedrijf.loading" />
      <template v-if="bedrijf.success">{{ bedrijf.data?.type }}</template>
    </td>
    <td>
      {{ bedrijf.data?.kvkNummer || klant.data?.kvkNummer }}
    </td>
    <td>
      <div class="skeleton" v-if="bedrijf.loading" />
      <template v-if="bedrijf.success">{{
        [bedrijf.data?.postcode, bedrijf.data?.huisnummer].join(" ")
      }}</template>
    </td>
    <td class="wrap">
      <div class="skeleton" v-if="klant.loading" />
      <template v-if="klant.success">{{
        klant.data?.emailadressen?.join(", ")
      }}</template>
    </td>
    <td class="wrap">
      <div class="skeleton" v-if="klant.loading" />
      <template v-if="klant.success">{{
        klant.data?.telefoonnummers.join(", ")
      }}</template>
    </td>
    <td>
      <div class="skeleton" v-if="klant.loading || bedrijf.loading" />
      <router-link
        v-if="klant.data"
        :title="`Details ${naam}`"
        :to="getKlantUrl(klant.data)"
        @click="setCache(klant.data, bedrijf.data)"
      />
      <button
        v-else-if="bedrijf.data && bedrijfIdentifier"
        type="button"
        title="Aanmaken"
        @click="navigate(bedrijf.data, bedrijfIdentifier)"
      />
    </td>
  </tr>
</template>
<script lang="ts" setup>
import { computed, watchEffect } from "vue";
import { useBedrijfByIdentifier } from "../use-bedrijf-by-identifier";
import { useKlantByBedrijfIdentifier } from "./use-klant-by-bedrijf-identifier";
import type { Bedrijf, BedrijfIdentifier } from "@/services/kvk";
import { useRouter } from "vue-router";
import { mutate } from "swrv";
import { ensureKlantForBedrijfIdentifier } from "./ensure-klant-for-bedrijf-identifier";
import type { Klant as KlantOpenKlant1 } from "@/services/openklant1/types";
import type { Klant as KlantOpenKlant2 } from "@/services/openklant2/types";

const props = defineProps<{ item: Bedrijf | KlantOpenKlant1 | KlantOpenKlant2; autoNavigate?: boolean }>();

const matchingBedrijf = useBedrijfByIdentifier(() => {
  // wordt niet meer gebruikt, alleen als we een klant hebben maar telefoonnumer en email adres is eruitgesloopt
  if (props.item._typeOfKlant === "bedrijf") return undefined;
  const { vestigingsnummer, rsin } = props.item;
  if (vestigingsnummer)
    return {
      vestigingsnummer,
    };
  if (rsin)
    return {
      rsin,
    };
});

const matchingKlant = useKlantByBedrijfIdentifier(() => {
  // we hebben al een klant, we hoeven die niet meer op te zoeken
  if (props.item._typeOfKlant === "klant") return undefined;
  const { vestigingsnummer, rsin } = props.item;
  if (vestigingsnummer)
    return {
      vestigingsnummer,
    };
  if (rsin)
    return {
      rsin,
    };
});

const bedrijf = computed(() =>
  props.item._typeOfKlant === "bedrijf"
    ? { data: props.item, success: true, loading: false, error: false }
    : { ...matchingBedrijf },
);

const klant = computed(() =>
  props.item._typeOfKlant === "klant"
    ? { data: props.item, success: true, loading: false, error: false }
    : { ...matchingKlant },
);

const naam = computed(
  () =>
    bedrijf.value.data?.bedrijfsnaam || klant.value.data?.bedrijfsnaam || "",
);

const bedrijfIdentifier = computed<BedrijfIdentifier | undefined>(() => {
  const { rsin, kvkNummer, vestigingsnummer } = bedrijf.value.data ?? {};
  if (vestigingsnummer)
    return {
      vestigingsnummer,
    };
  if (rsin)
    return {
      rsin,
      kvkNummer,
    };
  return undefined;
});

const router = useRouter();

const getKlantUrl = (klant: KlantOpenKlant1 | KlantOpenKlant2) => `/bedrijven/${klant.id}`;

const setCache = (klant: KlantOpenKlant1 | KlantOpenKlant2, bedrijf?: Bedrijf | null) => {
  mutate(klant.id, klant);
  const bedrijfId = bedrijf?.vestigingsnummer || bedrijf?.rsin;
  if (bedrijfId) {
    mutate("bedrijf" + bedrijfId, bedrijf);
  }
};

async function navigate(bedrijf: Bedrijf, identifier: BedrijfIdentifier) {

  const bedrijfsnaam = bedrijf.bedrijfsnaam; 
  const klant = await ensureKlantForBedrijfIdentifier(identifier, bedrijfsnaam);

  setCache(klant, bedrijf);

  const url = getKlantUrl(klant);
  await router.push(url);
}

watchEffect(() => {
  if (
    props.autoNavigate &&
    klant.value.success &&
    bedrijf.value.data &&
    bedrijfIdentifier.value
  ) {
    navigate(bedrijf.value.data, bedrijfIdentifier.value);
  }
});
</script>

<style scoped lang="scss">
td:empty::after {
  content: "-";
}

.skeleton {
  min-height: 1rem;
}
</style>
