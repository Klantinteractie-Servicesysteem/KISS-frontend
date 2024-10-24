<template>
  <service-data-wrapper :data="cm">
    <template #success="{ data }">
      <template v-if="data?.medewerkerIdentificatie">
        <dt>Aangemaakt door</dt>
        <dd>{{ fullName(data.medewerkerIdentificatie) }}</dd>
      </template>
      <slot
        name="object"
        :object="object"
        v-for="(object, k) in data?.objectcontactmomenten ?? []"
        :key="k"
      >
      </slot>
    </template>
  </service-data-wrapper>
  <contactmoment-details-context :url="url">
    <template #details="{ details }">
      <template v-if="details?.vraag">
        <dt>Vraag</dt>
        <dd>{{ details.vraag }}</dd>
      </template>
      <template v-if="details?.specifiekeVraag">
        <dt>Specifieke vraag</dt>
        <dd>{{ details.specifiekeVraag }}</dd>
      </template>
    </template>
  </contactmoment-details-context>

  <service-data-wrapper :data="cm">
    <template #success="{ data }">
      <template v-if="data?.tekst">
        <dt>Toelichting</dt>
        <dd>{{ data.tekst }}</dd>
      </template>
    </template>
  </service-data-wrapper>
</template>
<script setup lang="ts">
import ServiceDataWrapper from "@/components/ServiceDataWrapper.vue";
import { useContactmomentByUrl } from "./service";
import { fullName } from "@/helpers/string";
import ContactmomentDetailsContext from "./ContactmomentDetailsContext.vue";

const props = defineProps<{ url: string }>();
const cm = useContactmomentByUrl(() => props.url);
</script>
