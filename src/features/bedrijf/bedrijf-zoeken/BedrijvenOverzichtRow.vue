<template>
  <tr class="row-link">
    <th scope="row" class="wrap">
      <div class="skeleton" v-if="bedrijf.loading" />
      <template v-else-if="bedrijf.success">
        {{ bedrijf.data?.bedrijfsnaam }}
      </template>
    </th>
    <td>
      {{ bedrijf.data?.kvkNummer }}
    </td>
    <td>
      <div class="skeleton" v-if="bedrijf.loading" />
      <template v-if="bedrijf.success">
        {{ bedrijf.data?.vestigingsnummer }}
      </template>
    </td>

    <td>
      <div class="skeleton" v-if="bedrijf.loading" />
      <template v-if="bedrijf.success">
        {{ [bedrijf.data?.postcode, bedrijf.data?.huisnummer].join(" ") }}
      </template>
    </td>
    <td class="wrap">
      <div class="skeleton" v-if="matchingKlant.loading" />
      <template v-if="matchingKlant.success">
        {{ matchingKlant.data?.emailadressen?.join(", ") }}
      </template>
    </td>
    <td class="wrap">
      <div class="skeleton" v-if="matchingKlant.loading" />
      <template v-if="matchingKlant.success">
        {{ matchingKlant.data?.telefoonnummers.join(", ") }}
      </template>
    </td>
    <td>
      <div class="skeleton" v-if="matchingKlant.loading || bedrijf.loading" />

      <template v-if="matchingKlant.success && matchingKlant.data">
        <router-link
          :title="`Details ${naam}`"
          :to="getKlantUrl(matchingKlant.data)"
          @click="setCache(matchingKlant.data, bedrijf.data)"
        />
      </template>
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

import { useKlantByBedrijfIdentifier } from "./use-klant-by-bedrijf-identifier";
import type { Bedrijf, BedrijfIdentifier } from "@/services/kvk";
import { useRouter } from "vue-router";
import { mutate } from "swrv";
import { ensureKlantForBedrijfIdentifier } from "./ensure-klant-for-bedrijf-identifier";
import type { Klant } from "@/services/openklant/types";
import type { KlantBedrijfIdentifier } from "@/services/openklant2";

const props = defineProps<{
  item: Bedrijf | Klant;
  autoNavigate?: boolean;
}>();

// const matchingBedrijf = useBedrijfByIdentifier(() => {
// wordt niet meer gebruikt, alleen relevant als we een klant hebben en kvkv gegevens erbij willen zoeken/
// maar dat was alleen relevant toen een klant ook uit openklant gevonden kon worden adhv telefoonnumer en email, maar dat is momenteel niet meer mogelijk
//   if (props.item._typeOfKlant === "bedrijf") return undefined;
//   const { vestigingsnummer, rsin } = props.item;
//   if (vestigingsnummer)
//     return {
//       vestigingsnummer,
//     };
//   if (rsin)
//     return {
//       rsin,
//     };
// });

const matchingKlant = useKlantByBedrijfIdentifier(() => {
  if (props.item._typeOfKlant === "klant") return undefined;

  const { vestigingsnummer, kvkNummer } = props.item;

  if (vestigingsnummer && kvkNummer)
    return {
      vestigingsnummer,
      kvkNummer,
    };

  // if (rsin)
  //   return {
  //     rsin, //openklant1 gebruikte rsin. esuite kvknummer.
  //   };

  if (kvkNummer)
    return {
      kvkNummer, //openklant1 gebruikte rsin. esuite kvknummer.
    };
});

const bedrijf = computed(() =>
  props.item._typeOfKlant === "bedrijf"
    ? { data: props.item, success: true, loading: false, error: false }
    : { success: false, loading: false },
);

const naam = computed(() => bedrijf.value.data?.bedrijfsnaam || "");

const bedrijfIdentifier = computed<KlantBedrijfIdentifier | undefined>(() => {
  const { kvkNummer, vestigingsnummer } = bedrijf.value.data ?? {};
  if (vestigingsnummer && kvkNummer)
    return {
      vestigingsnummer,
      kvkNummer,
    };

  if (kvkNummer)
    return {
      kvkNummer,
    };

  return undefined;
});

const router = useRouter();

const getKlantUrl = (klant: Klant) => `/bedrijven/${klant.id}`;

const setCache = (klant: Klant, bedrijf?: Bedrijf | null) => {
  mutate(klant.id, klant);
  const bedrijfId = bedrijf?.vestigingsnummer || bedrijf?.rsin;

  if (bedrijfId) {
    mutate("bedrijf" + bedrijfId, bedrijf);
  }
};

async function navigate(bedrijf: Bedrijf, identifier: KlantBedrijfIdentifier) {
  const bedrijfsnaam = bedrijf.bedrijfsnaam;
  const klant = await ensureKlantForBedrijfIdentifier(identifier, bedrijfsnaam);

  setCache(klant, bedrijf);

  const url = getKlantUrl(klant);
  await router.push(url);
}

watchEffect(() => {
  if (
    props.autoNavigate &&
    matchingKlant.success &&
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
