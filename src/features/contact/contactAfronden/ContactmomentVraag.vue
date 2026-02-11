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
import type { Kennisartikel } from "@/features/types";
import { type Bron, type Vraag } from "@/stores/contactmoment/index";
import { onMounted, ref, computed } from "vue";

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

  const sectionIndex = vraag.value.vraag?.sectionIndex;

  vraagOptions.value = computed(() => [
    ...vraag.value.websites.map((item) => item.website),
    ...vraag.value.kennisartikelen.flatMap((item) => [
      item.kennisartikel,
      ...(item.kennisartikel as Kennisartikel).sections.map((section) => ({
        ...item.kennisartikel,
        title: [item.kennisartikel.title, section].join(" - "),
      })),
    ]),
    ...vraag.value.nieuwsberichten.map((item) => item.nieuwsbericht),
    ...vraag.value.werkinstructies.map((item) => item.werkinstructie),
    ...vraag.value.vacs.map((item) => item.vac),
  ]).value;

  if (vraag.value.vraag) {
    selectedVraag.value = vraag.value.vraag;
  }

  if (sectionIndex !== undefined) {
    const vraagIndex = vraagOptions.value.indexOf(
      vraag.value.vraag as {
        title: string;
        url: string;
        sectionIndex?: number | undefined;
      },
    );

    if (vraagIndex !== -1) {
      const newVraag = vraagOptions.value[sectionIndex + vraagIndex];
      if (newVraag !== undefined) {
        selectedVraag.value = newVraag;

        vraag.value.vraag = selectedVraag.value as Bron;
      }
    }
  }
});
</script>
