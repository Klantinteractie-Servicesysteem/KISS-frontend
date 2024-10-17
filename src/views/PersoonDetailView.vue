<template>
  <back-link />

  <utrecht-heading :level="1">Persoonsinformatie</utrecht-heading>

  <tab-list v-model="activeTab">
    <tab-list-data-item label="Contactgegevens" :data="klant">
      <template #success="{ data }">
        <klant-details :klant="data" />
      </template>
    </tab-list-data-item>

    <tab-list-data-item label="BRP gegevens" :data="persoon">
      <template #success="{ data }">
        <brp-gegevens v-if="data" :persoon="data" />
      </template>
    </tab-list-data-item>

    <tab-list-data-item
      label="Contactmomenten"
      :data="contactmomenten"
      :disabled="(c) => !c.count"
    >
      <template #success="{ data }">
        <utrecht-heading :level="2"> Contactmomenten </utrecht-heading>

        <contactmomenten-overzicht :contactmomenten="data.page">
          <template v-slot:object="{ object }">
            <zaak-preview :zaakurl="object.object"></zaak-preview>
          </template>
        </contactmomenten-overzicht>
      </template>
    </tab-list-data-item>

    <tab-list-data-item label="Zaken" :data="zaken" :disabled="(c) => !c.count">
      <template #success="{ data }">
        <utrecht-heading :level="2"> Zaken </utrecht-heading>

        <zaken-overzicht
          :zaken="data.page"
          :vraag="contactmomentStore.huidigContactmoment?.huidigeVraag"
        />
      </template>
    </tab-list-data-item>

    <tab-list-data-item
      label="Contactverzoeken"
      :data="contactverzoeken"
      :disabled="(c) => !c.count"
    >
      <template #success="{ data }">
        <utrecht-heading :level="2">Contactverzoeken</utrecht-heading>

        <contactverzoeken-overzicht :contactverzoeken="data.page">
          <template #onderwerp="{ contactmomentUrl }">
            <contactmoment-details-context :url="contactmomentUrl">
              <template #details="{ details }">
                {{ details?.vraag || details?.specifiekeVraag }}
              </template>
            </contactmoment-details-context>
          </template>

          <!-- voor OK1/esuite moeten gegevens die bij het contactmoment en niet bij het contactverzoek horen apart opgehaald worden-->
          <template
            v-if="!gebruikKlantInteracatiesApi"
            #contactmoment="{ url }"
          >
            <contactmoment-preview :url="url">
              <template #object="{ object }">
                <zaak-preview :zaakurl="object.object" />
              </template>
            </contactmoment-preview>
          </template>
        </contactverzoeken-overzicht>
      </template>
    </tab-list-data-item>
  </tab-list>
</template>

<script setup lang="ts">
import { computed, onMounted, ref, watch } from "vue";
import { Heading as UtrechtHeading } from "@utrecht/component-library-vue";
import { useContactmomentStore } from "@/stores/contactmoment";
import { ContactmomentenOverzicht } from "@/features/contact/contactmoment";
import { KlantDetails, useKlantById } from "@/features/klant/klant-details";
import {
  isOk2DefaultContactenApi,
  useContactmomentenByKlantId,
} from "@/features/contact/contactmoment/service";
import { useZakenByBsn } from "@/features/zaaksysteem";
import ZakenOverzicht from "@/features/zaaksysteem/ZakenOverzicht.vue";
import ZaakPreview from "@/features/zaaksysteem/components/ZaakPreview.vue";
import ContactverzoekenOverzicht from "@/features/contact/contactverzoek/overzicht/ContactverzoekenOverzicht.vue";
import ContactmomentPreview from "@/features/contact/contactmoment/ContactmomentPreview.vue";
import { TabList, TabListDataItem } from "@/components/tabs";
import BackLink from "@/components/BackLink.vue";
import ContactmomentDetailsContext from "@/features/contact/contactmoment/ContactmomentDetailsContext.vue";
import {
  usePersoonByBsn,
  BrpGegevens,
} from "@/features/persoon/persoon-details";
import { useContactverzoekenByKlantId } from "@/features/contact/contactverzoek/overzicht/service";

const activeTab = ref("");

const props = defineProps<{ persoonId: string }>();
const klantId = computed(() => props.persoonId);
const contactmomentStore = useContactmomentStore();
const klant = useKlantById(klantId);
const klantUrl = computed(() => (klant.success ? klant.data.url ?? "" : ""));

const gebruikKlantInteracatiesApi = ref<boolean | null>(null);

const contactverzoeken = useContactverzoekenByKlantId(
  klantUrl,
  gebruikKlantInteracatiesApi,
  "wasPartij__url"
);

const contactmomenten = useContactmomentenByKlantId(
  klantUrl,
  gebruikKlantInteracatiesApi,
);

onMounted(async () => {
  gebruikKlantInteracatiesApi.value = await isOk2DefaultContactenApi();
});

const getBsn = () => (!klant.success || !klant.data.bsn ? "" : klant.data.bsn);
const klantBsn = computed(getBsn);

const zaken = useZakenByBsn(klantBsn);
const persoon = usePersoonByBsn(getBsn);

watch(
  [() => klant.success && klant.data, () => persoon.success && persoon.data],
  ([k, p]) => {
    if (!k) return;
    contactmomentStore.setKlant({
      ...k,
      ...p,
      hasContactInformation:
        !!k.emailadressen.length || !!k.telefoonnummers.length,
    });
  },
  { immediate: true },
);
</script>
