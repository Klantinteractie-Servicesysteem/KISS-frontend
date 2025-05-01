<template>
  <tr class="row-link" v-if="item">
    <th scope="row" class="wrap">
      {{ item.bedrijfsnaam }}
    </th>
    <td>
      {{ item?.kvkNummer }}
    </td>
    <td>
      {{ item.vestigingsnummer }}
    </td>

    <td>
      {{ [item.postcode, item.huisnummer].join(" ") }}
    </td>
    <td class="wrap">
      <div v-if="loading" />
      <template v-if="!error">
        {{
          deKlantUitHetEigenRegisterMetCotnactgegevensUitAlleEigenRegisters?.emailadressen?.join(
            ", ",
          )
        }}
      </template>
    </td>
    <td class="wrap">
      <div class="skeleton" v-if="loading" />
      <template v-if="!error">
        {{
          deKlantUitHetEigenRegisterMetCotnactgegevensUitAlleEigenRegisters?.telefoonnummers.join(
            ", ",
          )
        }}
      </template>
    </td>
    <td>
      <!-- <template v-if="matchingKlant.success && matchingKlant.data"> -->
      <!-- <router-link
        :title="`Details ${naam}`"
        :to="getKlantUrl(bedrijf?.data?.i)"
       
      /> -->
      <!-- </template> -->
      <button v-if="item" type="button" title="Details" @click="navigate()" />
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
  type Systeem,
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
import { useLoader } from "@/services";
import { fetchKlantByInternalId } from "@/features/klant/klant-details/fetch-klant";

const props = defineProps<{
  item: Bedrijf;
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
////todo: hier moet ook gekeken worden of de gegevens eventueel in een van de niet default register te vinden zijn!!
// const matchingKlant = useKlantByBedrijfIdentifier(() => {
//   if (props.item._typeOfKlant === "klant") return undefined;

//   const { vestigingsnummer, kvkNummer } = props.item;

//   if (vestigingsnummer && kvkNummer)
//     return {
//       vestigingsnummer,
//       kvkNummer,
//     };

//   // if (rsin)
//   //   return {
//   //     rsin, //openklant1 gebruikte rsin. esuite kvknummer.
//   //   };

//   if (kvkNummer)
//     return {
//       kvkNummer, //openklant1 gebruikte rsin. esuite kvknummer.
//     };
// });

// const bedrijf = computed(() =>
//   props.item._typeOfKlant === "bedrijf"
//     ? { data: props.item, success: true, loading: false, error: false }
//     : { success: false, loading: false },
// );

const internalId: string | null = null;

const {
  data: deKlantUitHetEigenRegisterMetCotnactgegevensUitAlleEigenRegisters,
  loading,
  error,
} = useLoader(async () => {
  if (
    !systemen.loading.value &&
    !systemen.error.value &&
    systemen.systemen.value?.length &&
    systemen.defaultSysteem.value
  )
    return;

  // is this Klant in the inmemory store ?

  const klantIdentificator: KlantIdentificator = {
    vestigingsnummer:
      "vestigingsnummer" in props.item
        ? props.item.vestigingsnummer
        : undefined,
    kvkNummer: "kvkNummer" in props.item ? props.item.kvkNummer : undefined,
  };

  if (!systemen.defaultSysteem.value) {
    return;
  }

  if (systemen.defaultSysteem.value.registryVersion === registryVersions.ok2) {
    return await fetchKlantByKlantIdentificatorOk2(
      systemen.defaultSysteem.value.identifier,
      klantIdentificator,
    );
  } else {
    return await fetchKlantByKlantIdentificatorOk1(
      systemen.defaultSysteem.value.identifier,
      klantIdentificator,
    );
  }

  // return fetchKlantFromExternalRegistryByExternalId({
  //   externalId: bedrijfIdentifier,
  //   systemen: systemen.systemen.value as Systeem[],
  //   defaultSysteem: systemen.defaultSysteem.value as Systeem,
  // });

  //finally try to fetch klant form own registries (openklant, esuite), in order to find any phonenumeber or an email address
});

// const bedrijfIdentifier = computed<KlantBedrijfIdentifier | undefined>(() => {
//   const { kvkNummer, vestigingsnummer } = props.item ?? {};
//   if (vestigingsnummer && kvkNummer)
//     return {
//       vestigingsnummer,
//       kvkNummer,
//     };

//   if (kvkNummer)
//     return {
//       kvkNummer,
//     };

//   return undefined;
// });

// //kijk of de klant al bekend is in het eigen default register
// let klant : Klant | null = null;
// if (
//   !systemen.loading.value &&
//   !systemen.error.value &&
//   systemen.defaultSysteem.value &&
//   bedrijfIdentifier.value
// ) {
//   const klantIdentificator: KlantIdentificator = {
//     vestigingsnummer:
//       "vestigingsnummer" in bedrijfIdentifier.value
//         ? bedrijfIdentifier.value.vestigingsnummer
//         : undefined,
//     kvkNummer: "kvkNummer" in bedrijfIdentifier.value ? bedrijfIdentifier.value.kvkNummer : undefined,
//   };

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
// }

// let existingContactmomentKlant : ContactmomentKlant | null = null;
// let newContactmomentKlant : ContactmomentKlant | null = null;

// if (klant === null) {
//     //not an existing klant in the default reegistry
//     //create one for use during this session.
//      newContactmomentKlant = <ContactmomentKlant>{
//       ...bedrijf.value.data,
//       //verplichte velden...
//       id: "",
//       telefoonnummers: [],
//       emailadressen: [],
//       hasContactInformation: false,
//     };

//     //keep the klant in the store for now.
//     contactmomentStore.setKlant(newContactmomentKlant);

//   }

//   if (klant != null) {
//     //not an existing klant in the default reegistry
//     //create one for use during this session.
//      existingContactmomentKlant = <ContactmomentKlant>{
//       ...klant,
//       bedrijfsnaam: bedrijf.value.data?.bedrijfsnaam, //om een of andere reden slaan we de bedrijfsnaam niet op in openklant. om deze wel te kunnen tonen op het afhandelscherm enmen we hem dan maar over uit het register.
//       id: klant.id,
//       telefoonnummers: klant.telefoonnummers,
//       emailadressen: klant.emailadressen,
//       hasContactInformation:
//         klant?.telefoonnummers?.length > 0 || klant?.emailadressen?.length > 0,
//     };

//     //keep the klant in the store for now.
//     contactmomentStore.setKlant(existingContactmomentKlant);

//   }

//const naam = computed(() => bedrijf.value.data?.bedrijfsnaam || "");

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

async function navigate() {
  //////////////////////
  //do we now this klant already?

  // if (klant === null) {
  //   //not an existing klant in the default reegistry
  //   //create one for use during this session.
  //   const newContactmomentKlant = <ContactmomentKlant>{
  //     ...bedrijf,
  //     //verplichte velden...
  //     id: "",
  //     telefoonnummers: [],
  //     emailadressen: [],
  //     hasContactInformation: false,
  //   };

  //   //keep the klant in the store for now.
  //   contactmomentStore.setKlant(newContactmomentKlant);

  const klantenInStoreBijHuiduigeVraag =
    contactmomentStore.$state.huidigContactmoment?.huidigeVraag.klanten;

  const kvkklantInStore = klantenInStoreBijHuiduigeVraag?.find(
    (x) =>
      x.klant.id === props.item.kvkNummer ||
      x.klant.id === props.item.rsin ||
      x.klant.id === props.item.vestigingsnummer,
  );

  if (kvkklantInStore?.klant?.internalId) {
    return fetchKlantByInternalId({
      internalId: kvkklantInStore?.klant?.internalId,
      systemen: systemen.systemen.value as Systeem[],
      defaultSysteem: systemen.defaultSysteem.value as Systeem,
    });
  }

  //de klant is nog niet bekend in de inmemory store van dit contactmoment voeg toe
  //er wordt dan een tijdelijk id gegeenreerd om verdertijdens de afhandeling aan te refeeren

  const newContactmomentKlant = <ContactmomentKlant>{
    ...props.item,
    //verplichte velden...
    id: "",
    telefoonnummers: [],
    emailadressen: [],
    hasContactInformation: false,
  };

  contactmomentStore.setKlant(newContactmomentKlant);

  await router.push(`/bedrijven/${internalId}`);
}

//if (klant != null) {
// //not an existing klant in the default reegistry
// //create one for use during this session.
// const existingContactmomentKlant = <ContactmomentKlant>{
//   ...klant,
//   bedrijfsnaam: bedrijf.bedrijfsnaam, //om een of andere reden slaan we de bedrijfsnaam niet op in openklant. om deze wel te kunnen tonen op het afhandelscherm enmen we hem dan maar over uit het register.
//   id: klant.id,
//   telefoonnummers: klant.telefoonnummers,
//   emailadressen: klant.emailadressen,
//   hasContactInformation:
//     klant?.telefoonnummers?.length > 0 || klant?.emailadressen?.length > 0,
// };

// //keep the klant in the store for now.
// contactmomentStore.setKlant(existingContactmomentKlant);
//await router.push(`/bedrijven/${existingContactmomentKlant.internalId}`);
//}

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
//}

watchEffect(() => {
  if (props.autoNavigate) {
    navigate();
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
