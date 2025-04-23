<template>
  <back-link />
  <utrecht-heading :level="1">Bedrijfsinformatie</utrecht-heading>
  <tab-list v-model="currentTab">
    <tab-list-item label="Contactgegevens">
      <template #default="{ setError, setLoading, setDisabled }">
        <klant-details
          :internalKlantId="internalKlantId"
          @load="
            klant = $event;
            setDisabled($event == null);
          "
          @loading="setLoading"
          @error="setError"
        />
      </template>
    </tab-list-item>
    <tab-list-item label="KvK-gegevens">
      <template #default="{ setError, setLoading }">
        <handelsregister-gegevens
          :internalKlantId="internalKlantId"
          @load="bedrijf = $event"
          @loading="setLoading"
          @error="setError"
        />
      </template>
    </tab-list-item>

    <tab-list-item label="Contactmomenten">
      <template #default="{ setError, setLoading, setDisabled }">
        <contactmomenten-for-klant-identificator
          v-if="bedrijf"
          :klant-identificator="bedrijf"
          @load="setDisabled(!$event?.length)"
          @loading="setLoading"
          @error="setError"
        />
      </template>
    </tab-list-item>

    <tab-list-item label="Zaken">
      <template #default="{ setError, setLoading, setDisabled }">
        <utrecht-heading :level="2"> Zaken </utrecht-heading>

        <zaken-for-klant
          v-if="bedrijf"
          :klant-identificator="bedrijf"
          :vraag="contactmomentStore.huidigContactmoment?.huidigeVraag"
          @load="setDisabled(!$event?.length)"
          @loading="setLoading"
          @error="setError"
        />
      </template>
    </tab-list-item>

    <tab-list-item label="Contactverzoeken">
      <template #default="{ setError, setLoading, setDisabled }">
        <contactverzoeken-for-klant-identificator
          v-if="bedrijf"
          :klant-identificator="bedrijf"
          @load="setDisabled(!$event?.length)"
          @loading="setLoading"
          @error="setError"
        />
      </template>
    </tab-list-item>
  </tab-list>
</template>

<script setup lang="ts">
import { computed, ref, watch } from "vue";
import { Heading as UtrechtHeading } from "@utrecht/component-library-vue";
import { useContactmomentStore } from "@/stores/contactmoment";
import { KlantDetails } from "@/features/klant/klant-details";
import { TabList, TabListItem } from "@/components/tabs";
import BackLink from "@/components/BackLink.vue";
import { HandelsregisterGegevens } from "@/features/bedrijf/bedrijf-details";
import type { Bedrijf, BedrijfIdentifier } from "@/services/kvk";
import ContactverzoekenForKlantIdentificator from "@/features/contact/contactverzoek/overzicht/ContactverzoekenForKlantIdentificator.vue";
import ContactmomentenForKlantIdentificator from "@/features/contact/contactmoment/ContactmomentenForKlantIdentificator.vue";
import type { Klant } from "@/services/openklant/types";
import ZakenForKlant from "@/features/zaaksysteem/ZakenForKlant.vue";

defineProps<{ internalKlantId: string }>();

const contactmomentStore = useContactmomentStore();

const currentTab = ref("");
const klant = ref<Klant>();
const bedrijf = ref<Bedrijf>();

// const getBedrijfIdentifier = (): BedrijfIdentifier | undefined => {
//   if (!klant.value) return undefined;
//   if (klant.value.vestigingsnummer)
//     return {
//       vestigingsnummer: klant.value.vestigingsnummer,
//     };
//   // if (klant.data.rsin)
//   //   return {
//   //     rsin: klant.data.rsin,
//   //     kvkNummer: klant.data.kvkNummer,
//   //   };
//   if (klant.value.nietNatuurlijkPersoonIdentifier)
//     return {
//       //gechoogel met params verschil ok1 en esuite
//       rsin: klant.value.nietNatuurlijkPersoonIdentifier,
//     };
//   if (klant.value.rsin)
//     return {
//       //gechoogel met params verschil ok1 en esuite
//       rsin: klant.value.rsin,
//     };
// };

//const bedrijfIdentifier = computed(getBedrijfIdentifier);

// watch(
//   () =>
//     klant.value && bedrijf.value ? ([klant.value, bedrijf.value] as const) : [],
//   ([k, b]) => {
//     if (!k || !b) return;
//     contactmomentStore.setKlant({
//       ...k,
//       ...b,
//       hasContactInformation:
//         (k.emailadressen && k.emailadressen.length > 0) ||
//         (k.telefoonnummers && k.telefoonnummers.length > 0),
//     });
//   },
//   { immediate: true },
// );
</script>
