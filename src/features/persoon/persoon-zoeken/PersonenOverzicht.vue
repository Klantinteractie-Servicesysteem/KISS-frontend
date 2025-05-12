<template>
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
              [record.voornaam, record.voorvoegselAchternaam, record.achternaam]
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
</template>

<script lang="ts" setup>
import DutchDate from "@/components/DutchDate.vue";
import type { Persoon } from "@/services/brp";
import { useRouter } from "vue-router";
import { mutate } from "swrv";
import { watchEffect } from "vue";
import { useSystemen } from "@/services/environment/fetch-systemen";
import {
  useContactmomentStore,
  type ContactmomentKlant,
} from "@/stores/contactmoment";
import { fetchKlantByKlantIdentificatorOk } from "@/features/klant/klant-details/fetch-klant";

const props = defineProps<{
  records: Persoon[];
  navigateOnSingleResult?: boolean;
}>();
const router = useRouter();
const systemen = useSystemen();
const contactmomentStore = useContactmomentStore();

const navigate = async (persoon: Persoon) => {
  const { bsn } = persoon;
  if (!bsn) throw new Error("BSN is required");

  if (
    !systemen.loading.value &&
    !systemen.error.value &&
    systemen.defaultSysteem.value
  ) {
    const klant = await fetchKlantByKlantIdentificatorOk(
      { bsn: bsn },
      systemen.defaultSysteem.value,
    );

    await mutate("persoon" + bsn, persoon);

    if (klant) {
      //a persoon from Brp, who is allready stored in OpenKlant

      await mutate(klant.id, klant);

      const existingKlant = <ContactmomentKlant>{
        ...klant,

        //verplichte velden...
        id: klant.id,
        telefoonnummers: klant.telefoonnummers,
        emailadressen: klant.emailadressen,
        hasContactInformation:
          klant?.telefoonnummers?.length > 0 ||
          klant?.emailadressen?.length > 0,
      };

      contactmomentStore.setKlant(existingKlant);

      await router.push("/personen/" + existingKlant.internalId);
    } else {
      //a persoon from Brp, who doesn't have a record in OpenKlant yet.
      //Store the info from the Brp in the in memory store. When a contactmometn is saved,we will save this person in OpenKlant

      const newKlant = <ContactmomentKlant>{
        ...persoon,
        //verplichte velden...
        id: "",
        telefoonnummers: [],
        emailadressen: [],
        hasContactInformation: false,
      };

      contactmomentStore.setKlant(newKlant);

      await router.push("/personen/" + newKlant.internalId);
    }
  }
};

watchEffect(async () => {
  if (props.navigateOnSingleResult && props.records.length === 1) {
    await navigate(props.records[0]);
  }
});
</script>
