<template>
  <back-link />
  <utrecht-heading :level="1">Bedrijfsinformatie</utrecht-heading>

  <tab-list v-model="currentTab">
    <tab-list-data-item
      label="Contactgegevens"
      :data="klant"
      :disabled="(k) => !k"
    >
      <template #success="{ data }">
        <klant-details :klant="data" />
      </template>
    </tab-list-data-item>
    <tab-list-data-item
      label="KvK-gegevens"
      :data="bedrijf"
      :disabled="(b) => !b"
    >
      <template #success="{ data }">
        <handelsregister-gegevens v-if="data" :bedrijf="data" />
      </template>
    </tab-list-data-item>
    <tab-list-item label="Contactmomenten">
      <template #default="{ setError, setLoading, setDisabled }">
        <contactmomenten-for-klant-url
          v-if="gebruikKlantInteracatiesApi != null"
          :klant-url="klantUrl"
          :gebruik-klant-interacties="gebruikKlantInteracatiesApi"
          @load="setDisabled(!$event?.page?.length)"
          @loading="setLoading"
          @error="setError"
        >
          <template #object="{ object }">
            <zaak-preview :zaakurl="object.object" />
          </template>
        </contactmomenten-for-klant-url>
      </template>
    </tab-list-item>
    <tab-list-data-item label="Zaken" :data="zaken" :disabled="(z) => !z.count">
      <template #success="{ data }">
        <zaken-overzicht
          :zaken="data.page"
          :vraag="contactmomentStore.huidigContactmoment?.huidigeVraag"
        />
      </template>
    </tab-list-data-item>
    <tab-list-item label="Contactverzoeken">
      <template #default="{ setError, setLoading, setDisabled }">
        <contactverzoeken-for-klant-url
          v-if="gebruikKlantInteracatiesApi != null"
          :klant-url="klantUrl"
          :gebruik-klant-interacties="gebruikKlantInteracatiesApi"
          @load="setDisabled(!$event?.page?.length)"
          @loading="setLoading"
          @error="setError"
        >
          <template #object="{ object }">
            <zaak-preview v-if="object.object" :zaakurl="object.object" />
          </template>
        </contactverzoeken-for-klant-url>
      </template>
    </tab-list-item>
  </tab-list>
</template>

<script setup lang="ts">
import { computed, onMounted, ref, watch } from "vue";
import { Heading as UtrechtHeading } from "@utrecht/component-library-vue";
import { useContactmomentStore } from "@/stores/contactmoment";
import { KlantDetails, useKlantById } from "@/features/klant/klant-details";
// import Pagination from "@/nl-design-system/components/Pagination.vue";
import {
  useZakenByKlantBedrijfIdentifier,
  ZakenOverzicht,
} from "@/features/zaaksysteem";
import ZaakPreview from "@/features/zaaksysteem/components/ZaakPreview.vue";
import { TabList, TabListDataItem, TabListItem } from "@/components/tabs";

import BackLink from "@/components/BackLink.vue";
import { HandelsregisterGegevens } from "@/features/bedrijf/bedrijf-details";
import { useBedrijfByIdentifier } from "@/features/bedrijf/use-bedrijf-by-identifier";
import type { BedrijfIdentifier } from "@/services/kvk";
import { useOpenKlant2 } from "@/services/openklant2/service";
import ContactverzoekenForKlantUrl from "@/features/contact/contactverzoek/overzicht/ContactverzoekenForKlantUrl.vue";
import ContactmomentenForKlantUrl from "@/features/contact/contactmoment/ContactmomentenForKlantUrl.vue";

const props = defineProps<{ bedrijfId: string }>();
const gebruikKlantInteracatiesApi = ref<boolean | null>(null);

const klantId = computed(() => props.bedrijfId);
const contactmomentStore = useContactmomentStore();

const klant = useKlantById(klantId, gebruikKlantInteracatiesApi);

const klantUrl = computed(() => (klant.success ? (klant.data.url ?? "") : ""));
const currentTab = ref("");

//const contactverzoekenPage = ref(1);

const getBedrijfIdentifier = (): BedrijfIdentifier | undefined => {
  if (!klant.success || !klant.data) return undefined;

  if (klant.data.vestigingsnummer && klant.data.kvkNummer)
    return {
      vestigingsnummer: klant.data.vestigingsnummer,
      kvkNummer: klant.data.kvkNummer,
    };

  if (klant.data.kvkNummer)
    return {
      kvkNummer: klant.data.kvkNummer,
    };

  if (klant.data.nietNatuurlijkPersoonIdentifier)
    return {
      //gechoogel met params verschil ok1 en esuite
      rsin: klant.data.nietNatuurlijkPersoonIdentifier,
    };

  if (klant.data.rsin)
    return {
      //gechoogel met params verschil ok1 en esuite
      rsin: klant.data.rsin,
    };
};

const bedrijf = useBedrijfByIdentifier(getBedrijfIdentifier);

const zaken = useZakenByKlantBedrijfIdentifier(() => {
  if (!bedrijf.success || !bedrijf.data?.kvkNummer) return undefined;
  if (bedrijf.data.vestigingsnummer)
    return { vestigingsnummer: bedrijf.data.vestigingsnummer };
  if (bedrijf.data.rsin)
    return { rsin: bedrijf.data.rsin, kvkNummer: bedrijf.data.kvkNummer };
});

watch(
  () =>
    klant.success && bedrijf.success
      ? ([klant.data, bedrijf.data] as const)
      : [],
  ([k, b]) => {
    if (!k || !b) return;
    contactmomentStore.setKlant({
      ...k,
      ...b,
      hasContactInformation:
        (k.emailadressen && k.emailadressen.length > 0) ||
        (k.telefoonnummers && k.telefoonnummers.length > 0),
    });
  },
  { immediate: true },
);

onMounted(async () => {
  gebruikKlantInteracatiesApi.value = await useOpenKlant2();
});
</script>
