<template>
  <summary>
    <span aria-describedby="datum-header" class="first-column">
      <DutchDate v-if="registratieDatum" :date="registratieDatum" />
    </span>
    <span aria-describedby="medewerker-header">{{
      fullName(contactmoment.medewerkerIdentificatie)
    }}</span>
    <span aria-describedby="kanaal-header">{{ contactmoment.kanaal }}</span>
    <ServiceDataWrapper :data="cmDetails">
      <template #success="{ data }">
        <span aria-describedby="gespreksresultaat-header">
          {{ data?.gespreksresultaat }}
        </span>
        <span aria-describedby="afdeling-header">
          {{ data?.verantwoordelijkeAfdeling }}
        </span>
      </template>
    </ServiceDataWrapper>
  </summary>
  <dl>
    <dt>Starttijd</dt>
    <dd>
      <DutchTime v-if="registratieDatum" :date="registratieDatum" />
    </dd>
    <dt v-if="contactmoment.zaaknummers.length">Zaaknummer</dt>
    <dd v-for="zaaknummer in contactmoment.zaaknummers" :key="zaaknummer">
      {{ zaaknummer }}
    </dd>
    <ServiceDataWrapper :data="cmDetails">
      <template #success="{ data: contactmoment }">
        <template v-if="contactmoment?.vraag">
          <dt>Vraag</dt>
          <dd class="tekst">{{ contactmoment.vraag }}</dd>
        </template>
        <template v-if="contactmoment?.specifiekeVraag">
          <dt>Specifieke vraag</dt>
          <dd class="tekst">{{ contactmoment.specifiekeVraag }}</dd>
        </template>
      </template>
    </ServiceDataWrapper>

    <dt>Notitie</dt>
    <dd class="tekst">{{ contactmoment.tekst }}</dd>
  </dl>
</template>

<script setup lang="ts">
import { fullName } from "@/helpers/string";
import { useContactmomentDetails } from "./service";
import ServiceDataWrapper from "@/components/ServiceDataWrapper.vue";
import DutchDate from "@/components/DutchDate.vue";
import DutchTime from "@/components/DutchTime.vue";
import { computed } from "vue";
import type { ContactmomentViewModel } from "../types";

const props = defineProps<{
  contactmoment: ContactmomentViewModel;
}>();

const registratieDatum = computed(
  () =>
    props.contactmoment.registratiedatum &&
    new Date(props.contactmoment.registratiedatum),
);

const cmDetails = useContactmomentDetails(() => props.contactmoment.url);
</script>
