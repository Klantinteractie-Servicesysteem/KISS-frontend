<template>
  <article>
    <utrecht-heading :level="headingLevel" class="heading">
      {{ title }}
    </utrecht-heading>

 <section
     
      class="is-active"
   
    >
      <utrecht-heading :level="headingLevel + 1">Samenvatting</utrecht-heading>
    
    <vue-markdown :source="kennisartikelRaw.samenvatting" />
    </section>

  <br/>
  
<!-- 
      <nav>
      <ul>
        <li
          v-for="{  labels } in kennisartikelRaw.content"
          :key="labels[0] + 'nav'"
          
        >
       
          <a  :href="`#${labels[0]}`" >{{ labels[0] }}</a>
        </li>
      </ul>
    </nav>
  nn -->
    <section
      v-for="{  labels , content} in kennisartikelRaw.content"
      :key="labels[0] + 'text'"
 
    >
      <utrecht-heading :level="headingLevel + 1">{{ labels[0] }}</utrecht-heading>
  
       <vue-markdown :source="content"  />
    </section>
<!-- <br/> -->
       <!-- <pre style="width:800px">{{ kennisartikelRaw }}</pre>   -->
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
import { ContentFeedback } from "../feedback/index";
import type { Kennisartikel } from "./types";
import VueMarkdown from 'vue-markdown-render'

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

:deep(ul) {
    margin-left: 20px;
        list-style: disc;
  }
 
:deep(ol) {
    margin-left: 20px;
 
  }

article {




  display: flex;
  flex-wrap: wrap;
  gap: 20px;
    flex-direction: column;
  .heading {
    width: 100%;
  }

  > section {
    flex: 1;
   // display: none;

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
