<template>
  <back-link />
  <utrecht-heading :level="1">Bedrijfsinformatie</utrecht-heading>
  <application-message
    messageType="error"
    message="Deze pagina werkt tijdelijk niet, totdat story PC-341 is uitgevoerd"
  />
  <!-- <tab-list v-model="currentTab">
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
    <tab-list-data-item
      label="Contactmomenten"
      :data="contactmomenten"
      :disabled="(c) => !c.count"
    >
      <template #success="{ data }">
        <contactmomenten-overzicht :contactmomenten="data.page">
          <template #object="{ object }">
            <zaak-preview :zaakurl="object.object" />
          </template>
        </contactmomenten-overzicht>
      </template>
    </tab-list-data-item>
    <tab-list-data-item label="Zaken" :data="zaken" :disabled="(z) => !z.count">
      <template #success="{ data }">
        <zaken-overzicht
          :zaken="data.page"
          :vraag="contactmomentStore.huidigContactmoment?.huidigeVraag"
        />
      </template>
    </tab-list-data-item>
    <tab-list-data-item
      label="Contactverzoeken"
      :data="contactverzoeken"
      :disabled="(c) => !c.page.length"
    >
      <template #success="{ data }">
        <contactverzoeken-overzicht :contactverzoeken="data.page">
          <template #onderwerp="{ contactmomentUrl }">
            <contactmoment-details-context :url="contactmomentUrl">
              <template #details="{ details }">
                {{ details?.vraag || details?.specifiekeVraag }}
              </template>
            </contactmoment-details-context>
          </template>
          <template #contactmoment="{ url }">
            <contactmoment-preview :url="url">
              <template #object="{ object }">
                <zaak-preview v-if="object.object" :zaakurl="object.object" />
              </template>
            </contactmoment-preview>
          </template>
        </contactverzoeken-overzicht>
      </template>
    </tab-list-data-item>
  </tab-list> -->
</template>

<script setup lang="ts">
import ApplicationMessage from "@/components/ApplicationMessage.vue";
import { computed, ref, watch } from "vue";
import { Heading as UtrechtHeading } from "@utrecht/component-library-vue";
import { useContactmomentStore } from "@/stores/contactmoment";
import { ContactmomentenOverzicht } from "@/features/contact/contactmoment";
import {
  useBedrijfByIdentifier,
  HandelsregisterGegevens,
  KlantDetails,
  useKlantById,
} from "@/features/klant";
// import Pagination from "@/nl-design-system/components/Pagination.vue";
import { useContactmomentenByKlantId } from "@/features/contact/contactmoment/service";
import {
  useZakenByKlantBedrijfIdentifier,
  ZakenOverzicht,
} from "@/features/zaaksysteem";
import ZaakPreview from "@/features/zaaksysteem/components/ZaakPreview.vue";
import { TabList, TabListDataItem } from "@/components/tabs";
import { useContactverzoekenByKlantId } from "@/features/contact/contactverzoek/overzicht/service";
import ContactverzoekenOverzicht from "@/features/contact/contactverzoek/overzicht/ContactverzoekenOverzicht.vue";
import ContactmomentPreview from "@/features/contact/contactmoment/ContactmomentPreview.vue";
import BackLink from "@/components/BackLink.vue";
import ContactmomentDetailsContext from "@/features/contact/contactmoment/ContactmomentDetailsContext.vue";
import type { BedrijfIdentifier } from "@/features/klant/bedrijf/types";
const props = defineProps<{ bedrijfId: string }>();
const klantId = computed(() => props.bedrijfId);
const contactmomentStore = useContactmomentStore();
const klant = useKlantById(klantId);
const klantUrl = computed(() => (klant.success ? klant.data.url ?? "" : ""));
const currentTab = ref("");
watch(
  () => klant.success && klant.data,
  (k) => {
    if (!k) return;
    contactmomentStore.setKlant({
      ...k,
      hasContactInformation: !!k.emailadres || !!k.telefoonnummer,
    });
  },
  { immediate: true },
);

const contactverzoekenPage = ref(1);
const contactverzoeken = useContactverzoekenByKlantId(
  klantUrl,
  contactverzoekenPage,
);
const contactmomenten = useContactmomentenByKlantId(klantUrl);

const getBedrijfIdentifier = (): BedrijfIdentifier | undefined => {
  if (!klant.success || !klant.data) return undefined;
  if (klant.data.vestigingsnummer)
    return {
      vestigingsnummer: klant.data.vestigingsnummer,
    };
  if (klant.data.nietNatuurlijkPersoonIdentifier)
    return {
      nietNatuurlijkPersoonIdentifier:
        klant.data.nietNatuurlijkPersoonIdentifier,
    };
};

const zaken = useZakenByKlantBedrijfIdentifier(getBedrijfIdentifier);

const bedrijf = useBedrijfByIdentifier(getBedrijfIdentifier);
</script>
