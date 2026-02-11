<template>
  <div class="table-wrapper">
    <table class="overview">
      <slot name="caption" />
      <template v-if="records.length">
        <thead>
          <tr>
            <th>Naam</th>
            <th>Geboortedatum</th>
            <th>Adresregel 1</th>
            <th>Adresregel 2</th>
            <th>Bsn</th>
            <th class="row-link-header">Details</th>
          </tr>
        </thead>
        <tbody>
          <tr class="row-link" v-for="(record, idx) in records" :key="idx">
            <th scope="row" class="wrap">
              {{
                [
                  record.voornaam,
                  record.voorvoegselAchternaam,
                  record.achternaam,
                ]
                  .filter(Boolean)
                  .join(" ")
              }}
            </th>
            <td>
              <dutch-date
                v-if="record.geboortedatum"
                :date="record.geboortedatum"
              />
            </td>
            <td class="wrap">
              {{ record.adresregel1 }}
            </td>
            <td class="wrap">
              {{ record.adresregel2 }}
            </td>
            <td>
              {{ record.bsn }}
            </td>
            <td>
              <button type="button" title="Details" @click="navigate(record)" />
            </td>
          </tr>
        </tbody>
      </template>
    </table>
  </div>
</template>

<script lang="ts" setup>
import DutchDate from "@/components/DutchDate.vue";
import type { Persoon } from "@/services/brp";
import { useRouter } from "vue-router";
import { watchEffect } from "vue";
import { useSystemen } from "@/services/environment/fetch-systemen";
import {
  useContactmomentStore,
  type ContactmomentKlant,
} from "@/stores/contactmoment/index";
import {
  fetchKlantByKlantIdentificatorOk,
  fetchKlantFromNonDefaultSystems,
  heeftContactgegevens,
} from "@/services/openklant/service";

const props = defineProps<{
  records: Persoon[];
  navigateOnSingleResult?: boolean;
}>();
const router = useRouter();
const systemen = useSystemen();
const contactmomentStore = useContactmomentStore();

const navigate = async (persoon: Persoon) => {
  if (!persoon.bsn) throw new Error("BSN is required");

  const klantFromInternalStore = contactmomentStore.getBrpKlant(persoon.bsn);

  if (klantFromInternalStore) {
    // deze klant is al bekend in de applicatie store.
    // maak hem de actieve klant
    // en navigeer naar de detailpagina
    contactmomentStore.setAsActiveKlant(klantFromInternalStore);
    await router.push("/personen/" + klantFromInternalStore.internalId);
  } else {
    // als de klant nog niet in de in memory application store bekend is,
    // dan nu de gegevens opzoeken en toevoegen
    // en navigeer naar de detailpagina
    if (
      !systemen.loading.value &&
      !systemen.error.value &&
      systemen.defaultSysteem.value
    ) {
      // zoek de klant in het defaultregister.
      const klant = await fetchKlantByKlantIdentificatorOk(
        { bsn: persoon.bsn },
        systemen.defaultSysteem.value,
      );

      let telefoonnummers = klant?.telefoonnummers ?? [];
      let emailadressen = klant?.emailadressen ?? [];

      // if the klant is not found in the default registry
      // or if the klant doesn't have contactdetausl in the default registry
      // we will attempt to find those in any of the other registries
      if (!klant || !heeftContactgegevens(klant)) {
        if (systemen.systemen.value) {
          const fallbackKlant = await fetchKlantFromNonDefaultSystems(
            systemen.systemen.value,
            systemen.defaultSysteem.value,
            undefined,
            undefined,
            persoon.bsn,
          );
          // als er een fallback klant gevonden is
          // dan nemen we daar de contactgegevens van over
          if (fallbackKlant && heeftContactgegevens(fallbackKlant)) {
            telefoonnummers = fallbackKlant.telefoonnummers;
            emailadressen = fallbackKlant.emailadressen;
          }
        }
      }

      const storeKlant = <ContactmomentKlant>{
        id: klant?.id,
        telefoonnummers: telefoonnummers,
        emailadressen: emailadressen,
        hasContactInformation:
          telefoonnummers?.length > 0 || emailadressen?.length > 0,
        achternaam: persoon.achternaam,
        voornaam: persoon.voornaam,
        voorvoegselAchternaam: persoon.voorvoegselAchternaam,
        bsn: persoon.bsn,
      };

      contactmomentStore.setKlant(storeKlant);
      contactmomentStore.setAsActiveKlant(storeKlant);
      await router.push("/personen/" + storeKlant.internalId);
    }
  }
};

watchEffect(async () => {
  if (props.navigateOnSingleResult && props.records.length === 1) {
    await navigate(props.records[0]);
  }
});
</script>
