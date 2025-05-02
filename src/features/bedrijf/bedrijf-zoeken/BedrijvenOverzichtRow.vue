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
import { watchEffect } from "vue";

import {
  searchBedrijvenInHandelsRegisterByRsin,
  type Bedrijf,
} from "@/services/kvk";
import { useRouter } from "vue-router";
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
import type { KlantIdentificator } from "@/features/contact/types";
import { enforceOneOrZero, useLoader } from "@/services";
import {
  enrichKlantWithContactDetails,
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

  // find the klant in OpenKlant

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

  let klant = null;

  if (systemen.defaultSysteem.value.registryVersion === registryVersions.ok2) {
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

  if (!klant) return null;

  //we need to now the id later on but we cant put the klant in the websites inMemory store yet
  //since only the selected klant is linked to the current contactmoment and saved in the inMemory store
  //to prevent an other lookup in OpenKlant when we navigate, we'll store the id in a variable for now
  KlantRegisterKlantId = klant.id;

  if (heeftContactgegevens(klant)) return klant;

  // For non-natural persons, we have EITHER an RSIN OR a Chamber of Commerce number (kvknummer),
  // depending on whether the default system is ok1 or ok2.
  // To translate this to the other systems,
  // we need BOTH. So we first need to fetch the company again.

  const bedrijf = await searchBedrijvenInHandelsRegisterByRsin(
    klant.rsin ||
      klant.nietNatuurlijkPersoonIdentifier ||
      klant.kvkNummer ||
      "",
  ).then(enforceOneOrZero);

  if (!bedrijf) return klant;

  klant.kvkNummer = bedrijf.kvkNummer;
  klant.rsin = bedrijf.rsin;

  enrichKlantWithContactDetails(
    klant,
    systemen.systemen.value,
    systemen.defaultSysteem.value,
  );

  return klant;
});

const router = useRouter();

async function navigate() {
  const klantenInStoreBijHuiduigeVraag =
    contactmomentStore.$state.huidigContactmoment?.huidigeVraag.klanten;

  const kvkklantInStore = klantenInStoreBijHuiduigeVraag?.find(
    (x) =>
      (!props.item.kvkNummer || x.klant.kvkNummer === props.item.kvkNummer) &&
      (!props.item.rsin || x.klant.rsin === props.item.rsin) &&
      (!props.item.vestigingsnummer ||
        x.klant.vestigingsnummer === props.item.vestigingsnummer),
  );

  if (kvkklantInStore && KlantRegisterKlantId) {
    kvkklantInStore.klant.id = KlantRegisterKlantId; //we hebben al alle klanten in openklant opgezocht, maar alleen de klant waar we op klikken zit in de store. het openklant id hebben we wel nodig. dus nog even vastleggen
    await router.push(`/bedrijven/${kvkklantInStore.klant.internalId}`);
    return;
  }

  //todo: do we still need caching???

  //klant is not yet in website store.
  //add the klant and navigate to details page with the generated internal id
  const newContactmomentKlant = <ContactmomentKlant>{
    ...props.item,
    //verplichte velden...
    id: "",
    telefoonnummers: [],
    emailadressen: [],
    hasContactInformation: false,
  };

  contactmomentStore.setKlant(newContactmomentKlant);

  await router.push(`/bedrijven/${newContactmomentKlant.internalId}`);
}

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
