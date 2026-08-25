<template>
  <article>
    <utrecht-heading :level="headingLevel" class="heading">
      {{ title }}
    </utrecht-heading>

    <dl v-if="trefwoorden.length">
      <dt>Trefwoorden</dt>

      <dd>{{ trefwoorden.join(", ") }}</dd>
    </dl>

    <nav v-if="mappedSections.length > 1" aria-label="Secties">
      <ul>
        <li
          v-for="{ isActive, label, id, setActive } in mappedSections"
          :key="id + 'nav'"
        >
          <button
            type="button"
            :aria-current="isActive ? 'true' : undefined"
            :aria-controls="id"
            @click="setActive"
          >
            {{ label }}
          </button>
        </li>
      </ul>
    </nav>

    <section
      v-for="{ id, html, isActive, label } in mappedSections"
      :key="id + 'text'"
      :hidden="!isActive || undefined"
      :id="id"
    >
      <utrecht-heading :level="headingLevel + 1">{{ label }}</utrecht-heading>

      <div v-html="html" class="htmlcontent"></div>
    </section>
  </article>
</template>

<script setup lang="ts">
import { computed, ref, useId } from "vue";
import { Heading as UtrechtHeading } from "@utrecht/component-library-vue";
import { unescapedSanatizedWithIncreadesHeadingsHtml } from "@/helpers/html";

type KennisApiSectie = {
  type: string;
  inhoud: string;
  sortIndex?: number;
};

type KennisApiArtikel = {
  id: string;
  taal: string;
  titel: string;
  secties: KennisApiSectie[];
  trefwoorden?: string[];
};

const componentId = useId();

const props = defineProps<{
  kennisartikel?: KennisApiArtikel;
  title: string;
  headingLevel: 2 | 3 | 4;
}>();

const currentSectionIndex = ref(0);

const trefwoorden = computed(() => props.kennisartikel?.trefwoorden ?? []);

const processedSections = computed(() => {
  const secties = props.kennisartikel?.secties ?? [];

  return secties
    .filter((s) => s?.inhoud)
    .map((s, index: number) => ({
      label: s.type && s.type !== "generiek" ? s.type : `Sectie ${index + 1}`,
      html: unescapedSanatizedWithIncreadesHeadingsHtml(
        s.inhoud,
        props.headingLevel,
      ),
    }));
});

const mappedSections = computed(() =>
  processedSections.value.map((section, index) => ({
    ...section,
    id: `${componentId}-${index}`,
    isActive: index === currentSectionIndex.value,
    setActive() {
      currentSectionIndex.value = index;
    },
  })),
);
</script>

<style scoped lang="scss">
article {
  display: flex;
  flex-wrap: wrap;
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

  > section {
    flex: 1;

    &[hidden] {
      display: none;
    }
  }
}

nav ul {
  display: flex;
  flex-direction: column;
  gap: var(--spacing-extrasmall);

  li {
    border: 1px solid var(--color-tertiary);
  }

  button {
    cursor: pointer;
    padding: var(--spacing-small);

    &[aria-current] {
      color: var(--color-white);
      text-decoration: underline;
      background-color: var(--color-tertiary);
    }
  }
}
</style>
