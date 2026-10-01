<template>
  <select
    v-model="selectedVraag"
    :id="'hoofdvraag' + idx"
    class="utrecht-select utrecht-select--html-select"
    required
    @update:model-value="updateModelValue"
  >
    <option
      v-for="(item, itemIdx) in vraagOptions"
      :key="itemIdx + '|' + idx"
      :value="item"
    >
      {{ item.title }}
    </option>
    <option :value="undefined">Anders</option>
  </select>
</template>

<script lang="ts" setup>
import { type Bron, type Vraag } from "@/stores/contactmoment";
import { onMounted, ref } from "vue";
import type { Kennisartikel } from "@/features/search/types";
const vraagOptions = ref<Bron[]>([]);
const props = defineProps<{
  modelValue: Bron | undefined;
  idx: number; // Define the 'idx' prop as a number
  vraag: Vraag; // Define the 'vraag' prop as an object
}>();

// Use a local data property to store the selected value
const selectedVraag = ref<Bron>();

const emit = defineEmits<{
  "update:modelValue": [Bron];
}>();

function updateModelValue(v: Bron) {
  emit("update:modelValue", v);
}

onMounted(() => {
  if (!props.vraag) return;
  const vraag = ref(props.vraag as Vraag);

  // display only title if it has one section, else one postfixed option per section.
  const kennisartikelBlocks = vraag.value.kennisartikelen.map((item) => {
    const sections = (item.kennisartikel as Kennisartikel).sections;
    return sections.length > 0
      ? sections.map((section) => ({
          ...item.kennisartikel,
          title: [item.kennisartikel.title, section].join(" - "),
        }))
      : [item.kennisartikel];
  });

  vraagOptions.value = [
    ...vraag.value.websites.map((item) => item.website),
    ...kennisartikelBlocks.flat(),
    ...vraag.value.nieuwsberichten.map((item) => item.nieuwsbericht),
    ...vraag.value.werkinstructies.map((item) => item.werkinstructie),
    ...vraag.value.vacs.map((item) => item.vac),
  ];

  if (vraag.value.vraag) {
    selectedVraag.value = vraag.value.vraag;

    const articleIdx = vraag.value.kennisartikelen.findIndex(
      (item) => item.kennisartikel === vraag.value.vraag,
    );

    if (articleIdx !== -1) {
      const sectionIndex =
        (vraag.value.vraag as Kennisartikel).sectionIndex ?? 0;
      const newVraag = kennisartikelBlocks[articleIdx]?.[sectionIndex];

      if (newVraag !== undefined) {
        selectedVraag.value = newVraag;

        vraag.value.vraag = selectedVraag.value as Bron;
      }
    }
  }
});
</script>
