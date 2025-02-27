<template>
  <div class="klant-panel">
    <utrecht-heading :level="1">Contactverzoeken</utrecht-heading>
    <contactverzoeken-zoeker>
      <template #object="{ object }">
        <zaak-preview
          v-if="object.object && defaultSysteem"
          :zaakurl="object.object"
          :systeem-id="defaultSysteem.identifier"
        />
      </template>
    </contactverzoeken-zoeker>
  </div>
</template>

<script setup lang="ts">
import ContactverzoekenZoeker from "@/features/contact/contactverzoek/overzicht/ContactverzoekenZoeker.vue";
import ZaakPreview from "@/features/zaaksysteem/components/ZaakPreview.vue";
import { useLoader } from "@/services";
import { fetchSystemen } from "@/services/environment/fetch-systemen";
import { Heading as UtrechtHeading } from "@utrecht/component-library-vue";
import { computed } from "vue";

const { data: systemen } = useLoader(() => fetchSystemen());
const defaultSysteem = computed(() =>
  systemen.value?.find(({ isDefault }) => isDefault),
);
</script>
