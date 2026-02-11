<template>
  <label
    class="utrecht-form-label"
    v-if="organisatorischeEenheidVragenSets.length > 0"
  >
    <span>Onderwerp</span>

    <select
      class="utrecht-select utrecht-select--html-select"
      name="VragenSets"
      v-model="vragenSetId"
      @change="setOnderwerp"
    >
      <option :value="0">Geen</option>
      <option
        v-for="item in organisatorischeEenheidVragenSets"
        :key="item.id"
        :value="item.id"
      >
        {{ item.titel }}
      </option>
    </select>
  </label>
</template>

<script setup lang="ts">
import { computed, ref, useModel, watch } from "vue";
import type {
  ContactVerzoekVragenSet,
  TypeOrganisatorischeEenheid,
} from "@/features/types";

const props = defineProps<{
  organisatorischeEenheidId?: string; // de organisatorische eenheid waarvan vragensets getoond mogen worden in de keuzelijst
  organisatorischeEenheidSoort?: TypeOrganisatorischeEenheid;
  vragenSets: ContactVerzoekVragenSet[]; // alle vragensets
  vragenSetIdMap: Map<TypeOrganisatorischeEenheid, number | undefined>; // map van vragenSetIds op organisatorischeEenheidSoort
  contactVerzoekVragenSet?: ContactVerzoekVragenSet; // de (voor)geselecteerde vragenset
}>();

const vragenSets = useModel(props, "vragenSets");
const vragenSetIdMap = useModel(props, "vragenSetIdMap");
const contactVerzoekVragenSet = useModel(props, "contactVerzoekVragenSet");

const vragenSetId = ref<number>();

// subset van vragensets horende bij de geselecteerde organisatorische eenheid
const organisatorischeEenheidVragenSets = computed(() => {
  return vragenSets.value.filter(
    (s) =>
      s.organisatorischeEenheidId == props.organisatorischeEenheidId &&
      s.organisatorischeEenheidSoort == props.organisatorischeEenheidSoort,
  );
});

watch(
  organisatorischeEenheidVragenSets,
  (value) => {
    // clear if no soort or empty set
    if (!props.organisatorischeEenheidSoort || !value.length) {
      contactVerzoekVragenSet.value = undefined;
      vragenSetId.value = undefined;

      return;
    }

    const mapId = vragenSetIdMap.value.get(props.organisatorischeEenheidSoort);

    // get vragenSet by mapped vragenSetId van een organisatorische eenheid, anders prefill with first
    if (mapId !== undefined) {
      contactVerzoekVragenSet.value = value.find((set) => set.id === mapId);
      vragenSetId.value = mapId;
    } else {
      contactVerzoekVragenSet.value = value[0];
      vragenSetId.value = value[0].id;

      vragenSetIdMap.value.set(props.organisatorischeEenheidSoort, value[0].id);
    }
  },
  { immediate: true },
);

const setOnderwerp = () => {
  if (!props.organisatorischeEenheidSoort) return;

  // wanneer een item uit de lijst gekozen is, de bijbehorende vragenset opzoeken
  contactVerzoekVragenSet.value = vragenSets.value.find(
    (set) => set.id === vragenSetId.value,
  );

  vragenSetIdMap.value.set(
    props.organisatorischeEenheidSoort,
    vragenSetId.value,
  );
};
</script>
