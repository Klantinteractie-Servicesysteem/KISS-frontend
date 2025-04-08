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

      <!-- <template v-if="matchingKlant.success && matchingKlant.data"> -->
      <!-- <router-link
        :title="`Details ${naam}`"
        :to="getKlantUrl(bedrijf?.data?.i)"
       
      /> -->
      <!-- </template> -->
      <button
        v-if="bedrijf.data && bedrijfIdentifier"
        type="button"
        title="Details"
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
import {
  registryVersions,
  useSystemen,
} from "@/services/environment/fetch-systemen";
import { fetchKlantByKlantIdentificatorOk1 } from "@/services/openklant1";
import { fetchKlantByKlantIdentificatorOk2 } from "@/services/openklant2";
import {
  useContactmomentStore,
  type ContactmomentKlant,
} from "@/stores/contactmoment";

const props = defineProps<{
  item: Bedrijf | Klant;
  autoNavigate?: boolean;
}>();

const systemen = useSystemen();
const contactmomentStore = useContactmomentStore();
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

/// hier doen wat nu bij naviaget gebeurd: het ophalen van de klant uit het register. dit is puur extra om telnr en email alvast te kunnen tonen
const matchingKlant = useKlantByBedrijfIdentifier(() => {
  if (props.item._typeOfKlant === "klant") return undefined;

  const { vestigingsnummer, kvkNummer } = props.item;

  if (vestigingsnummer)
    return {
      vestigingsnummer,
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

  if (kvkNummer)
    return {
      kvkNummer,
    };
  return undefined;
});

const router = useRouter();

const getKlantUrl = (internalId: string | undefined) =>
  `/bedrijven/${internalId}`;

// const setCache = (klant: Klant, bedrijf?: Bedrijf | null) => {
//   mutate(klant.id, klant);
//   const bedrijfId = bedrijf?.vestigingsnummer || bedrijf?.rsin;

//   if (bedrijfId) {
//     mutate("bedrijf" + bedrijfId, bedrijf);
//   }
// };

async function navigate(bedrijf: Bedrijf, identifier: BedrijfIdentifier) {
  //const bedrijfsnaam = bedrijf.bedrijfsnaam;
  //const klant = await ensureKlantForBedrijfIdentifier(identifier, bedrijfsnaam);

  const klantIdentificator =
    "vestigingsnummer" in identifier
      ? { vestigingsnummer: identifier.vestigingsnummer }
      : "rsin" in identifier
        ? { rsin: identifier.rsin, kvkNummer: identifier.kvkNummer }
        : { kvkNummer: identifier.kvkNummer };

  if (
    !systemen.loading.value &&
    !systemen.error.value &&
    systemen.defaultSysteem.value
  ) {
    let klant = null;

    //todo: dit eerder doen, zodat we de klant gegevens (email/te nr al in de zoekresultaten lijst kunnen tonen)
    if (
      systemen.defaultSysteem.value.registryVersion === registryVersions.ok2
    ) {
      klant = await fetchKlantByKlantIdentificatorOk2(
        systemen.defaultSysteem.value.identifier,
        klantIdentificator,
      );
    } else {
      klant = await fetchKlantByKlantIdentificatorOk1(
        systemen.defaultSysteem.value.identifier,
        klantIdentificator,
      );
    }

    if (klant) {
      //  setCache(klant, bedrijf);

      const existingKlant = <ContactmomentKlant>{
        ...klant,
        ...bedrijf,
        //verplichte velden...
        id: klant.id,
        telefoonnummers: klant.telefoonnummers,
        emailadressen: klant.emailadressen,
        hasContactInformation:
          klant?.telefoonnummers?.length > 0 ||
          klant?.emailadressen?.length > 0,
      };

      // const x = {
      //   ...existingKlant,
      //   ...bedrijf,
      //   hasContactInformation:
      //     (klant.emailadressen && klant.emailadressen.length > 0) ||
      //     (klant.telefoonnummers && klant.telefoonnummers.length > 0),
      // };

      contactmomentStore.setKlant(existingKlant);

      const url = getKlantUrl(existingKlant.internalId);
      await router.push(url);
    } else {
      //klant aanmaken  in store
      //..
      //..
      //..
      //..
    }
  }
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
