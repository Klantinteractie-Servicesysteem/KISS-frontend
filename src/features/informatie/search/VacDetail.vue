<template>
  <article>
    <utrecht-heading :level="headingLevel" class="heading">
      {{ title }}
    </utrecht-heading>
    <section v-if="antwoordSection">
      <utrecht-heading :level="headingLevel + 1">{{
        antwoordSection.label
      }}</utrecht-heading>
      <div v-html="antwoordSection.html" class="htmlcontent"></div>
    </section>
    <section v-if="toelichtingSection">
      <utrecht-heading :level="headingLevel + 1">{{
        toelichtingSection.label
      }}</utrecht-heading>
      <div v-html="toelichtingSection.html" class="htmlcontent"></div>
    </section>
  </article>
  <content-feedback
    v-if="antwoordSection"
    :name="CurrentAntwoordFeedbackSection.label"
    :url="unknownURL"
    :current-section="CurrentAntwoordFeedbackSection"
  />
</template>
<script setup lang="ts">
import { unescapedSanatizedWithIncreadesHeadingsHtml } from "@/helpers/html";
import { Heading as UtrechtHeading } from "@utrecht/component-library-vue";
import { ContentFeedback } from "./feedback/index";
import { computed } from "vue";
import type { CurrentFeedbackSection } from "./feedback/types";

const knownSections = {
  toelichting: "toelichting",
  antwoord: "antwoord",
} as const;

const props = defineProps<{
  raw: any;
  title: string;
  headingLevel: 2 | 3 | 4;
}>();

const CurrentAntwoordFeedbackSection: CurrentFeedbackSection = {
  label: props.title,
};

const unknownURL = "onbekend";

const getSection = (
  sectionName: string,
  sectionLabel: string | undefined,
): { label: string; html: string } | null => {
  const section = props.raw[sectionName];

  return section
    ? {
        label: sectionLabel ? sectionLabel : sectionName,
        html: unescapedSanatizedWithIncreadesHeadingsHtml(
          section,
          props.headingLevel,
        ),
      }
    : null;
};

const antwoordSection = computed(() =>
  getSection(knownSections.antwoord, "Antwoord"),
);

const toelichtingSection = computed(() =>
  getSection(knownSections.toelichting, "Interne toelichting"),
);
</script>

<style scoped lang="scss">
article {
  display: flex;
  flex-flow: column wrap;
  gap: var(--spacing-large);

  .heading {
    width: 100%;
  }
}
</style>
