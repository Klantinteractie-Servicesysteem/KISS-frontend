<template>
  <article>
    <utrecht-heading :level="headingLevel" class="heading">
      {{ title }}
    </utrecht-heading>

    <dl v-if="trefwoorden.length">
      <dt>Trefwoorden</dt>

      <dd>{{ trefwoorden.join(", ") }}</dd>
    </dl>

    <section v-if="antwoordSection">
      <utrecht-heading :level="headingLevel + 1">Antwoord</utrecht-heading>

      <div v-html="antwoordSection" class="htmlcontent"></div>
    </section>

    <section v-if="toelichtingSection">
      <utrecht-heading :level="headingLevel + 1"
        >Interne toelichting</utrecht-heading
      >
      <div v-html="toelichtingSection" class="htmlcontent"></div>
    </section>
  </article>
</template>

<script setup lang="ts">
import { computed } from "vue";
import { Heading as UtrechtHeading } from "@utrecht/component-library-vue";
import { unescapedSanatizedWithIncreadesHeadingsHtml } from "@/helpers/html";

type KennisApiVac = {
  id: string;
  taal: string;
  vraag: string;
  antwoord: string;
  toelichting?: string;
  trefwoorden?: string[];
};

const props = defineProps<{
  vac?: KennisApiVac;
  title: string;
  headingLevel: 2 | 3 | 4;
}>();

const trefwoorden = computed(() => props.vac?.trefwoorden ?? []);

const antwoordSection = computed(() =>
  props.vac?.antwoord
    ? unescapedSanatizedWithIncreadesHeadingsHtml(
        props.vac.antwoord,
        props.headingLevel,
      )
    : null,
);

const toelichtingSection = computed(() =>
  props.vac?.toelichting
    ? unescapedSanatizedWithIncreadesHeadingsHtml(
        props.vac.toelichting,
        props.headingLevel,
      )
    : null,
);
</script>

<style scoped lang="scss">
article {
  display: flex;
  flex-direction: column;
  gap: var(--spacing-large);

  [class^="utrecht-heading"] {
    inline-size: 100%;
  }

  > dl {
    display: grid;
    grid-template-columns: max-content 1fr;
    gap: var(--spacing-small);
    inline-size: 100%;

    dt {
      font-weight: bold;

      &::after {
        content: ":";
      }
    }

    dd {
      margin: 0;

      &:not(:last-child)::after {
        content: ",";
      }
    }
  }
}
</style>
