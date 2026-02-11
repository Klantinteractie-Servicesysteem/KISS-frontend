<template>
  <article>
    <utrecht-heading :level="headingLevel" class="heading">
      {{ title }}
    </utrecht-heading>
    <nav>
      <ul>
        <li
          v-for="{ isActive, label, id, setActive } in mappedSections"
          :key="id + 'nav'"
          :class="{ 'is-active': isActive }"
        >
          <span v-if="isActive">{{ label }}</span>
          <a v-else :href="`#${id}`" @click.prevent="setActive">{{ label }}</a>
        </li>
      </ul>
    </nav>
    <section
      v-for="{ id, html, isActive, label } in mappedSections"
      :key="id + 'text'"
      :class="{ 'is-active': isActive }"
      :id="id"
    >
      <utrecht-heading :level="headingLevel + 1">{{ label }}</utrecht-heading>
      <div v-html="html" class="htmlcontent"></div>
    </section>
  </article>

  <content-feedback
    v-if="currentFeedbackSection"
    :name="title"
    :url="kennisartikelRaw.url"
    :current-section="currentFeedbackSection"
  />
</template>
<script setup lang="ts">
import { unescapedSanatizedWithIncreadesHeadingsHtml } from "@/helpers/html";
import { Heading as UtrechtHeading } from "@utrecht/component-library-vue";
import { nanoid } from "nanoid";
import { computed, ref, watch } from "vue";
import { ContentFeedback } from "./feedback/index";
import type { Kennisartikel } from "@/features/types";

const knownSections = {
  tekst: "Inleiding",
  procedureBeschrijving: "Aanvraag",
  vereisten: "Voorwaarden",
  bewijs: "Bewijs",
  kostenEnBetaalmethoden: "Kosten",
  uitersteTermijn: "Termijn",
  bezwaarEnBeroep: "Bezwaar",
  notice: "Bijzonderheden",
  wtdBijGeenReactie: "Geen reactie",
  contact: "Contact",
  deskMemo: "KCC",
} as const;

const componentId = nanoid();

const props = defineProps<{
  kennisartikelRaw: any;
  title: string;
  headingLevel: 2 | 3 | 4;
}>();

const currentSectionIndex = ref(0);

const currentFeedbackSection = computed(() => {
  const currentSection = mappedSections.value[currentSectionIndex.value];
  if (!currentSection) return undefined;
  return {
    label: currentSection.label,
    id: currentSection.key,
  };
});

const kennisartikel = computed<Record<string, string>>(() => {
  const { vertalingen } = props.kennisartikelRaw || {};
  if (!Array.isArray(vertalingen)) return {};
  return vertalingen.find(({ taal }) => taal === "nl") || {};
});

const processedSections = computed(() => {
  const allSections = Object.entries(knownSections).map(([key, label]) => ({
    label,
    key: key,
    text: kennisartikel.value[key],
  }));

  const sectionsWithActualText = allSections.filter(({ text }) => !!text);

  const sectionsWithProcessedHtml = sectionsWithActualText.map(
    ({ label, text, key }) => ({
      key: key,
      label,
      html: unescapedSanatizedWithIncreadesHeadingsHtml(
        text,
        props.headingLevel,
      ),
    }),
  );

  return sectionsWithProcessedHtml;
});

// seperate this computed variable for caching purposes: making a section active doesn't trigger the reprocessing of the source html
const mappedSections = computed(() =>
  processedSections.value.map((section, index) => ({
    ...section,
    id: componentId + index,
    isActive: index === currentSectionIndex.value,
    setActive() {
      currentSectionIndex.value = index;
    },
  })),
);

watch(
  () => props.kennisartikelRaw.uuid,
  () => {
    currentSectionIndex.value = 0;
  },
);

const KENNISARTIKEL_SELECTED = "kennisartikel-selected";

const emit = defineEmits<{
  (e: typeof KENNISARTIKEL_SELECTED, artikel: Kennisartikel): void;
}>();

watch(
  [processedSections, currentSectionIndex],
  ([s, sectionIndex]) => {
    if (!s.length) return;
    const sections = s
      .map(({ label }) => label)
      .filter((x) => x !== knownSections.tekst);

    emit(KENNISARTIKEL_SELECTED, {
      title: props.title,
      url: props.kennisartikelRaw.url,
      afdelingen: props.kennisartikelRaw.afdelingen,
      sections,
      sectionIndex,
    });
  },
  { immediate: true },
);
</script>

<style scoped lang="scss">
article {
  display: flex;
  flex-wrap: wrap;
  gap: var(--spacing-large);

  .heading {
    width: 100%;
  }

  > section {
    flex: 1;
    display: none;

    &.is-active {
      display: block;
    }
  }
}

nav ul {
  display: flex;
  flex-direction: column;
  gap: var(--spacing-extrasmall);

  li {
    border: 1px solid var(--color-tertiary);

    a,
    span {
      text-decoration: none;
      color: inherit;
      padding: var(--spacing-small);
      display: block;
    }
  }

  .is-active {
    background-color: var(--color-tertiary);
    color: var(--color-white);
    text-decoration: underline;
  }
}
</style>
