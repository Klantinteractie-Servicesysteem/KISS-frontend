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
      <template v-if="!error && data">
        {{ data.emailadressen?.join(", ") }}
      </template>
    </td>
    <td class="wrap">
      <div class="skeleton" v-if="loading" />
      <template v-if="!error && data">
        {{ data.telefoonnummers.join(", ") }}
      </template>
    </td>
    <td>
      <button v-if="item" type="button" title="Details" @click="navigate()" />
    </td>
  </tr>
</template>
<script lang="ts" setup>
import { type Bedrijf } from "@/services/kvk";
import { useRouter } from "vue-router";
import { useSystemen } from "@/services/environment/fetch-systemen";
import {
  useContactmomentStore,
  type ContactmomentKlant,
} from "@/stores/contactmoment/index";
import { useLoader } from "@/services";
import { ref, watchEffect } from "vue";
import type { Klant, KlantIdentificator } from "@/services/openklant/types";
import {
  fetchKlantByKlantIdentificatorOk,
  fetchKlantFromNonDefaultSystems,
  heeftContactgegevens,
} from "@/services/openklant/service";

const props = defineProps<{
  item: Bedrijf;
  autoNavigate?: boolean;
}>();

const systemen = useSystemen();
const contactmomentStore = useContactmomentStore();

const klant = ref<Klant | null>();

//Please note, the implementation for bedrijven is a litle more complex than for personen.
//for personen we only show brp data on the searchresult list.
//We can get the klant from OpenKlant when we navigate to the details page
//that's when the klant will be linked to the current contactmoment and
//that's when we ensure its in the inmemory store of this website
//For bedrijven however, we need to show contactdetails from OpenKlant in the results list
//therefore we need to fetch them from openKlant right away,
//but we can only link the klant to the current contactmoment and place the klant in the website inmemory store
//when we navigate to the details page

const { loading, error, data } = useLoader(async () => {
  if (
    systemen.loading.value ||
    systemen.error.value ||
    !systemen.systemen.value?.length ||
    !systemen.defaultSysteem.value
  )
    return;

  const data: { telefoonnummers: string[]; emailadressen: string[] } = {
    telefoonnummers: [],
    emailadressen: [],
  };

  // find the klant in the Klant registry

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

  klant.value = await fetchKlantByKlantIdentificatorOk(
    klantIdentificator,
    systemen.defaultSysteem.value,
  );

  //return the Klant from the default registry if it has contactdetails
  if (klant.value && heeftContactgegevens(klant.value)) {
    data.telefoonnummers = klant.value.telefoonnummers;
    data.emailadressen = klant.value.emailadressen;
  } else {
    const nonDefaultKlant = await fetchKlantFromNonDefaultSystems(
      systemen.systemen.value,
      systemen.defaultSysteem.value,
      props.item.kvkNummer,
      props.item.vestigingsnummer,
      undefined,
    );

    if (nonDefaultKlant) {
      data.telefoonnummers = nonDefaultKlant.telefoonnummers;
      data.emailadressen = nonDefaultKlant.emailadressen;
    }
  }

  return data;
});

const router = useRouter();

async function navigate() {
  if (!data.value) {
    return;
  }

  const klantFromInternalStore = contactmomentStore.getBedrijfsKlant(
    props.item.kvkNummer,
    props.item.vestigingsnummer,
  );

  if (klantFromInternalStore) {
    // deze klant is al bekend in de applicatie store.
    // maak hem de actieve klant
    // en navigeer naar de detailpagina
    contactmomentStore.setAsActiveKlant(klantFromInternalStore);
    await router.push("/bedrijven/" + klantFromInternalStore.internalId);
  } else {
    // als de klant nog niet in de in memory application store bekend is,
    // dan nu de gegevens opzoeken en toevoegen
    // en navigeer naar de detailpagina

    // in tegenstelling tot wat we bij personen doen, hoeven we de klant hier niet meer op te zoeken
    // omdat de contactgegevens in de zoekresultten tabel getoond worden, hebben we de klant en eventuele contactgegevens uit andere rgisteres al opgehaald

    const storeKlant = <ContactmomentKlant>{
      id: klant.value?.id,
      telefoonnummers: data.value.telefoonnummers,
      emailadressen: data.value.emailadressen,
      hasContactInformation:
        data.value.telefoonnummers?.length > 0 ||
        data.value.emailadressen?.length > 0,
      bedrijfsnaam: props.item.bedrijfsnaam,
      vestigingsnummer: props.item.vestigingsnummer,
      kvkNummer: props.item.kvkNummer,
    };

    contactmomentStore.setKlant(storeKlant);
    contactmomentStore.setAsActiveKlant(storeKlant);
    await router.push("/bedrijven/" + storeKlant.internalId);
  }
}

watchEffect(() => {
  if (data && props.autoNavigate) navigate();
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
