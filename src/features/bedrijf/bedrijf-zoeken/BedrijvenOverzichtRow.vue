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
          KlantUitdefaultKlantRegisterMetContactgegevensUitAlleKlantRegisters?.emailadressen?.join(
            ", ",
          )
        }}
      </template>
    </td>
    <td class="wrap">
      <div class="skeleton" v-if="loading" />
      <template v-if="!error">
        {{
          KlantUitdefaultKlantRegisterMetContactgegevensUitAlleKlantRegisters?.telefoonnummers.join(
            ", ",
          )
        }}
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
} from "@/stores/contactmoment";
import type { KlantIdentificator } from "@/features/contact/types";
import { useLoader } from "@/services";
import {
  fetchKlantByKlantIdentificatorOk,
  fetchKlantFromNonDefaultSystems,
  heeftContactgegevens,
} from "@/features/klant/klant-details/fetch-klant";

const props = defineProps<{
  item: Bedrijf;
  autoNavigate?: boolean;
}>();

const systemen = useSystemen();
const contactmomentStore = useContactmomentStore();

//Please note, the implementation for bedrijven is a litle more complex than for personen.
//for personen we only show brp data on the searchresult list.
//We can get the klant from OpenKlant when we navigate to the details page
//that's when the klant will be linked to the current contactmoment and
//that's when we ensure its in the inmemory store of this website
//For bedrijven however, we need to show contactdetails from OpenKlant in the results list
//therefore we need to fetch them from openKlant right away,
//but we can only link the klant to the current contactmoment and place the klant in the website inmemory store
//when we navigate to the details page

let KlantRegisterKlantId: string | null = null;

const {
  data: KlantUitdefaultKlantRegisterMetContactgegevensUitAlleKlantRegisters,
  loading,
  error,
} = useLoader(async () => {
  if (
    systemen.loading.value ||
    systemen.error.value ||
    !systemen.systemen.value?.length ||
    !systemen.defaultSysteem.value
  )
    return;

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

  const klant = await fetchKlantByKlantIdentificatorOk(
    klantIdentificator,
    systemen.defaultSysteem.value,
  );

  if (klant) {
    //we need to now the id later on but we cant put the klant in the websites inMemory store yet
    //since only the selected klant is linked to the current contactmoment and saved in the inMemory store
    //to prevent an other lookup in OpenKlant when we navigate, we'll store the id in a variable for now
    KlantRegisterKlantId = klant.id;

    //return the Klant from the default registry if it has contactdetails

    if (props.autoNavigate) navigate();

    if (heeftContactgegevens(klant)) return klant;
  }

  const nonDefaultKlant = await fetchKlantFromNonDefaultSystems(
    systemen.systemen.value,
    systemen.defaultSysteem.value,
    props.item.kvkNummer,
    props.item.vestigingsnummer,
    undefined,
    klant?.id ?? "",
  );

  return nonDefaultKlant ?? klant;
});

const router = useRouter();

async function navigate() {
  const klantenInStoreBijHuiduigeVraag =
    contactmomentStore.$state.huidigContactmoment?.huidigeVraag.klanten;

  const kvkKlantInStore = klantenInStoreBijHuiduigeVraag?.find(
    (x) =>
      (!props.item.kvkNummer || x.klant.kvkNummer === props.item.kvkNummer) &&
      (!props.item.vestigingsnummer ||
        x.klant.vestigingsnummer === props.item.vestigingsnummer),
  );

  if (kvkKlantInStore && KlantRegisterKlantId) {
    kvkKlantInStore.klant.id = KlantRegisterKlantId; //we hebben al alle klanten in openklant opgezocht, maar alleen de klant waar we op klikken zit in de store. het openklant id hebben we wel nodig. dus nog even vastleggen
    await router.push(`/bedrijven/${kvkKlantInStore.klant.internalId}`);
    return;
  }

  //klant is not yet in website store.
  //add the klant and navigate to details page with the generated internal id
  const newContactmomentKlant = <ContactmomentKlant>{
    ...props.item,
    id: KlantRegisterKlantId,
    telefoonnummers: [],
    emailadressen: [],
    hasContactInformation: false,
  };

  contactmomentStore.setKlant(newContactmomentKlant);

  await router.push(`/bedrijven/${newContactmomentKlant.internalId}`);
}
</script>

<style scoped lang="scss">
td:empty::after {
  content: "-";
}

.skeleton {
  min-height: 1rem;
}
</style>
