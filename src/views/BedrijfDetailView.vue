<template>
  <section>
    <utrecht-heading :level="1">Bedrijfsinformatie</utrecht-heading>
    <nav>
      <ul>
        <li>
          <router-link :to="{ name: 'bedrijven' }">{{
            "< Bedrijven zoeken"
          }}</router-link>
        </li>
      </ul>
    </nav>
    <simple-spinner v-if="klant.loading" />
    <bedrijf-details v-else-if="klant.success" :klant="klant.data" />
    <application-message
      v-if="klant.error"
      message="Er ging iets mis bij het ophalen van de klant. Probeer het later
      nog eens."
      messageType="error"
    />

    <simple-spinner v-if="bedrijf.loading" />
    <handelsregister-gegevens
      v-if="bedrijf.success && bedrijf.data"
      :bedrijf="bedrijf.data"
    />
    <application-message
      v-if="bedrijf.error"
      message="Er ging iets mis bij het ophalen van de Handelsregister gegevens. Probeer het later nog eens."
      messageType="error"
    />

    <simple-spinner v-if="contactverzoeken.loading" />
    <application-message
      v-if="contactverzoeken.error"
      message="Er ging iets mis bij het ophalen van de contactverzoeken. Probeer het later nog eens."
      messageType="error"
    />
    <template
      v-if="contactverzoeken.success && contactverzoeken.data.page.length"
    >
      <utrecht-heading :level="2">Contactverzoeken</utrecht-heading>

      <contactverzoeken-overzicht
        :contactverzoeken="contactverzoeken.data.page"
      />
    </template>

    <!-- Zaken -->

    <simple-spinner v-if="zaken.loading" />

    <application-message
      v-if="zaken.error"
      message="Er ging iets mis bij het ophalen van de zaken. Probeer het later nog eens."
      messageType="error"
    />

    <template v-if="zaken.success && zaken.data.page.length">
      <utrecht-heading :level="2"> Zaken </utrecht-heading>

      <zaken-overzicht
        :zaken="zaken.data.page"
        :vraag="contactmomentStore.huidigContactmoment?.huidigeVraag"
      />
    </template>

    <!-- Contactmomenten -->

    <simple-spinner v-if="contactmomenten.loading" />

    <application-message
      v-if="contactmomenten.error"
      message="Er ging iets mis bij het ophalen van de contactmomenten. Probeer het later nog eens."
      messageType="error"
    />

    <template v-if="contactmomenten.success && contactmomenten.data">
      <utrecht-heading :level="2"> Contactmomenten </utrecht-heading>

      <contactmomenten-overzicht :contactmomenten="contactmomenten.data.page">
        <template v-slot:zaken="{ zaken }">
          <template v-for="zaakurl in zaken" :key="zaakurl">
            <zaak-preview :zaakurl="zaakurl"></zaak-preview>
          </template>
        </template>
      </contactmomenten-overzicht>
      <!-- 
      <pagination
        class="pagination"
        :pagination="contactmomenten.data"
        @navigate="onContactmomentenNavigate"
      /> -->
    </template>
  </section>
</template>

<script setup lang="ts">
import { computed, ref, watch } from "vue";
import { Heading as UtrechtHeading } from "@utrecht/component-library-vue";
import { useContactmomentStore } from "@/stores/contactmoment";
import {
  ContactmomentenOverzicht,
  useContactverzoekenByKlantId,
} from "@/features/contactmoment";
import {
  useBedrijfByVestigingsnummer,
  HandelsregisterGegevens,
  BedrijfDetails,
  useKlantById,
} from "@/features/klant";
import ApplicationMessage from "@/components/ApplicationMessage.vue";
import SimpleSpinner from "@/components/SimpleSpinner.vue";
import ContactverzoekenOverzicht from "@/features/contactmoment/ContactverzoekenOverzicht.vue";
// import Pagination from "@/nl-design-system/components/Pagination.vue";
import { useContactmomentenByKlantId } from "@/features/contactmoment/service";
import {
  useZakenByVestigingsnummer,
  ZakenOverzicht,
} from "@/features/zaaksysteem";
import ZaakPreview from "@/features/zaaksysteem/components/ZaakPreview.vue";

const props = defineProps<{ bedrijfId: string }>();
const klantId = computed(() => props.bedrijfId);
const contactmomentStore = useContactmomentStore();
const klant = useKlantById(klantId);
const klantUrl = computed(() => (klant.success ? klant.data.url ?? "" : ""));

watch(
  () => klant.success && klant.data,
  (k) => {
    if (!k) return;
    contactmomentStore.setKlant({
      ...k,
      hasContactInformation: !!k.emailadres || !!k.telefoonnummer,
    });
  },
  { immediate: true }
);

const contactverzoekenPage = ref(1);
const contactverzoeken = useContactverzoekenByKlantId(
  klantId,
  contactverzoekenPage
);

const contactmomentenPage = ref(1);
const contactmomenten = useContactmomentenByKlantId(
  klantUrl,
  contactmomentenPage
);

// const onContactmomentenNavigate = (page: number) => {
//   contactmomentenPage.value = page;
// };

const getVestigingsnummer = () =>
  !klant.success || !klant.data.vestigingsnummer
    ? ""
    : klant.data.vestigingsnummer;
const klantVestigingsnummer = computed(getVestigingsnummer);

const zaken = useZakenByVestigingsnummer(klantVestigingsnummer);

const bedrijf = useBedrijfByVestigingsnummer(getVestigingsnummer);
</script>

<style scoped lang="scss">
nav {
  list-style: none;
}

section > * {
  margin-block-end: var(--spacing-large);
}

utrecht-heading {
  margin-block-end: 0;
}
</style>
