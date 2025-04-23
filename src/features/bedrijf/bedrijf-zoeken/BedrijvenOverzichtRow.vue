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
import {
  fetchKlantByKlantIdentificatorOk2,
  type KlantBedrijfIdentifier,
} from "@/services/openklant2";
import {
  useContactmomentStore,
  type ContactmomentKlant,
} from "@/stores/contactmoment";
import type { KlantIdentificator } from "@/features/contact/types";

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

// const getKlantUrl = (internalId: string | undefined) =>
//   `/bedrijven/${internalId}`;

// const setCache = (klant: Klant, bedrijf?: Bedrijf | null) => {
//   mutate(klant.id, klant);
//   const bedrijfId = bedrijf?.vestigingsnummer || bedrijf?.rsin;

//   if (bedrijfId) {
//     mutate("bedrijf" + bedrijfId, bedrijf);
//   }
// };

async function navigate(bedrijf: Bedrijf, identifier: KlantBedrijfIdentifier) {
  //////////////////////
  //do we now this klant already?
  let klant = null;
  if (
    !systemen.loading.value &&
    !systemen.error.value &&
    systemen.defaultSysteem.value
  ) {
    const klantIdentificator: KlantIdentificator = {
      vestigingsnummer:
        "vestigingsnummer" in identifier
          ? identifier.vestigingsnummer
          : undefined,
      kvkNummer: "kvkNummer" in identifier ? identifier.kvkNummer : undefined,
    };

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
  }

  if (klant === null) {
    //not an existing klant in the default reegistry
    //create one for use during this session.
    const newKlant = <ContactmomentKlant>{
      ...bedrijf,
      //verplichte velden...
      id: "",
      telefoonnummers: [],
      emailadressen: [],
      hasContactInformation: false,
    };

    //keep the klant in the store for now.
    console.log("set klant newklant", newKlant);
    contactmomentStore.setKlant(newKlant);
    await router.push(`/bedrijven/${newKlant.internalId}`);
  }

  if (klant != null) {
    //not an existing klant in the default reegistry
    //create one for use during this session.
    const existingKlant = <ContactmomentKlant>{
      ...klant,
      bedrijfsnaam: bedrijf.bedrijfsnaam, //om een of andere reden slaan we de bedrijfsnaam niet op in openklant. om deze wel te kunnen tonen op het afhandelscherm enmen we hem dan maar over uit het register.
      id: klant.id,
      telefoonnummers: klant.telefoonnummers,
      emailadressen: klant.emailadressen,
      hasContactInformation:
        klant?.telefoonnummers?.length > 0 || klant?.emailadressen?.length > 0,
    };

    //keep the klant in the store for now.
    console.log("set klant existingklant", existingKlant);
    contactmomentStore.setKlant(existingKlant);
    await router.push(`/bedrijven/${existingKlant.internalId}`);
  }

  //////////////////////////

  // //const bedrijfsnaam = bedrijf.bedrijfsnaam;
  // //const klant = await ensureKlantForBedrijfIdentifier(identifier, bedrijfsnaam);

  // const klantIdentificator =
  //   "vestigingsnummer" in identifier
  //     ? { vestigingsnummer: identifier.vestigingsnummer }
  //     : "rsin" in identifier
  //       ? { rsin: identifier.rsin }
  //       : "kvkNummer" in identifier
  //         ? { kvkNummer: identifier.kvkNummer }
  //         : null;

  // if (
  //   !systemen.loading.value &&
  //   !systemen.error.value &&
  //   systemen.defaultSysteem.value
  // ) {
  //   let klant = null;

  //   //todo: dit eerder doen, zodat we de klant gegevens (email/te nr al in de zoekresultaten lijst kunnen tonen)
  //   if (
  //     systemen.defaultSysteem.value.registryVersion === registryVersions.ok2
  //   ) {
  //     klant = await fetchKlantByKlantIdentificatorOk2(
  //       systemen.defaultSysteem.value.identifier,
  //       klantIdentificator,
  //     );
  //   } else {
  //     klant = await fetchKlantByKlantIdentificatorOk1(
  //       systemen.defaultSysteem.value.identifier,
  //       klantIdentificator,
  //     );
  //   }

  //   if (klant) {
  //     //  setCache(klant, bedrijf);

  //     const existingKlant = <ContactmomentKlant>{
  //       ...klant,
  //       ...bedrijf,
  //       //verplichte velden...
  //       id: klant.id,
  //       telefoonnummers: klant.telefoonnummers,
  //       emailadressen: klant.emailadressen,
  //       hasContactInformation:
  //         klant?.telefoonnummers?.length > 0 ||
  //         klant?.emailadressen?.length > 0,
  //     };
  //   }
  //}
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
