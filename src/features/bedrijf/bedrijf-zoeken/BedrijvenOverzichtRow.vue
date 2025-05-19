<template>
  <tr class="row-link">
    <th scope="row" class="wrap">
      {{ item.bedrijfsnaam }}
    </th>
    <td>
      {{ item.kvkNummer }}
    </td>
    <td>
      {{ item.vestigingsnummer }}
    </td>

    <td>
      {{ [item.postcode, item.huisnummer].join(" ") }}
    </td>
    <td class="wrap">
      <div class="skeleton" v-if="matchingKlant.loading" />
      <template v-if="matchingKlant.success">
        {{ matchingKlant.data?.emailadressen?.join(", ") }}
      </template>
    </td>
    <td class="wrap">
      <div class="skeleton" v-if="matchingKlant.loading" />
      <template v-if="matchingKlant.success">
        {{ matchingKlant.data?.telefoonnummers.join(", ") }}
      </template>
    </td>
    <td>
      <button
        type="button"
        :title="`Details ${item.bedrijfsnaam}`"
        @click="setKlantAndNavigate"
      />
    </td>
  </tr>
</template>
<script lang="ts" setup>
import { watchEffect } from "vue";

import type { Bedrijf } from "@/services/kvk";
import { useRouter } from "vue-router";
import { useContactmomentStore } from "@/stores/contactmoment";
import { useKlantByBedrijfIdentifier } from "./use-klant-by-bedrijf-identifier";

const props = defineProps<{
  item: Bedrijf;
  autoNavigate?: boolean;
}>();

const matchingKlant = useKlantByBedrijfIdentifier(() => props.item);

const contactmomentStore = useContactmomentStore();

const router = useRouter();

const setKlant = () =>
  contactmomentStore.setKlant({
    ...props.item,
    telefoonnummers: [],
    emailadressen: [],
  });

async function setKlantAndNavigate() {
  setKlant();
  await router.push("/bedrijven/details");
}

watchEffect(async () => {
  if (props.autoNavigate && props.item) {
    await setKlantAndNavigate();
  }
});
</script>

<style scoped lang="scss">
td:empty::after {
  content: "-";
}

.skeleton {
  min-height: 1rem;
}
</style>
