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
import {
  fetchSystemen,
  registryVersions,
  useSystemen,
} from "@/services/environment/fetch-systemen";
import { useOrganisatieIds } from "@/stores/user";
import {
  ensureOk2Klant,
  fetchKlantByKlantIdentificatorOk2,
} from "@/services/openklant2";
import { ensureOk1Klant } from "@/services/openklant1";
import type { Klant } from "@/services/openklant/types";
import {
  useContactmomentStore,
  type ContactmomentKlant,
} from "@/stores/contactmoment";

const props = defineProps<{
  records: Persoon[];
  navigateOnSingleResult?: boolean;
}>();
const router = useRouter();
const systemen = useSystemen();
const contactmomentStore = useContactmomentStore();

const getKlantUrl = (klant: Klant) => `/personen/${klant.id}`;

// const ensureKlantForBsn = async (parameters: { bsn: string }) => {
//   const systemen = await fetchSystemen();
//   const defaultSysteem = systemen.find(({ isDefault }) => isDefault);

//   if (!defaultSysteem) {
//     throw new Error("Geen default register gevonden");
//   }

//   return defaultSysteem.registryVersion === registryVersions.ok2
//     ? await ensureOk2Klant(defaultSysteem.identifier, parameters)
//     : await ensureOk1Klant(
//         defaultSysteem.identifier,
//         parameters,
//         useOrganisatieIds().value[0] || "",
//       );
// };

const navigate = async (persoon: Persoon) => {
  const { bsn } = persoon;
  if (!bsn) throw new Error("BSN is required");

  //const klant = await ensureKlantForBsn({ bsn });

  if (
    !systemen.loading.value &&
    !systemen.error.value &&
    systemen.defaultSysteem.value
  ) {
    const klant = await fetchKlantByKlantIdentificatorOk2(
      systemen.defaultSysteem.value.identifier,
      { bsn: bsn },
    );

    await mutate("persoon" + bsn, persoon);

    if (klant) {
      await mutate(klant.id, klant);
      await router.push(getKlantUrl(klant));
    } else {
      //deze persoon is niet bekend in het klantregister. we slaan de gegevens uit het brp op in de store en maken de klant, met die gegevens, zonodig aan bij het opslaan van een contactmoment

      const newKlant = <ContactmomentKlant>{
        ...persoon,

        //verplichte velden... todo: alternatief voor id verzinnen?
        id: "",
        telefoonnummers: [],
        emailadressen: [],
        hasContactInformation: false,
      };

      contactmomentStore.setKlant(newKlant);

      await router.push("/personen/brp/" + newKlant.internalId);
    }
  }
};

watchEffect(async () => {
  if (props.navigateOnSingleResult && props.records.length === 1) {
    await navigate(props.records[0]);
  }
});
</script>
