<template>
  <article :class="{ read: read }">
    <small v-if="showType && bericht.type">
      {{ bericht.type }}
    </small>

    <div class="heading-container">
      <div class="heading-top-row">
        <time :datetime="bericht.date.toISOString()">{{
          localeString(bericht.date)
        }}</time>

        <menu>
          <li>
            <input
              v-if="contactmomentStore.contactmomentLoopt"
              class="save-bericht-to-contactmoment-checkbox"
              type="checkbox"
              title="Opslaan bij contactmoment"
              v-model="berichtSelectedInContactmoment"
              @click.stop="handleToggleBerichtInContactmoment"
            />
          </li>

          <li>
            <button
              @click="toggleRead"
              :title="`Markeer als ${read ? 'ongelezen' : 'gelezen'}`"
              class="toggle-read icon-after book"
              :disabled="toggleReadIsLoading"
            />
          </li>
        </menu>
      </div>

      <utrecht-heading model-value :level="level">
        <span class="title">{{ bericht.title }}</span>
      </utrecht-heading>
    </div>

    <div class="skills-container">
      <small
        v-for="(skill, i) in bericht.skills"
        :class="`category-${skill.split(' ').join('-')}`"
        :key="i"
      >
        {{ skill }}
      </small>
    </div>

    <utrecht-document model-value class="correct-header">
      <div v-html="sanitized" />
    </utrecht-document>
  </article>
</template>

<script lang="ts" setup>
import { computed, ref, watch, type PropType } from "vue";
import type { Werkbericht } from "./types";
import {
  UtrechtHeading,
  UtrechtDocument,
} from "@utrecht/web-component-library-vue";
import { readBericht, unreadBericht } from "./service";
import { sanitizeHtmlToBerichtFormat, increaseHeadings } from "@/helpers/html";
import { toast } from "@/stores/toast";
import { useContactmomentStore } from "@/stores/contactmoment";

type HeadingLevel = 1 | 2 | 3 | 4 | 5 | 6;

const props = defineProps({
  bericht: {
    type: Object as PropType<Werkbericht>,
    required: true,
  },
  level: {
    type: Number as PropType<HeadingLevel>,
    default: 3,
    validator: (val) => typeof val === "number" && val >= 1 && val <= 6,
  },
  showType: {
    type: Boolean,
    default: false,
  },
});

const contactmomentStore = useContactmomentStore();

const berichtSelectedInContactmoment = computed(() => {
  const { huidigContactmoment } = contactmomentStore;

  if (!huidigContactmoment) return false;

  const foundInNieuwsberichten =
    huidigContactmoment.huidigeVraag.nieuwsberichten.findIndex(
      (n) => n.nieuwsbericht.url === props.bericht.url
    ) !== -1;

  const foundInWerkinstructies =
    huidigContactmoment.huidigeVraag.werkinstructies.findIndex(
      (w) => w.werkinstructie.url === props.bericht.url
    ) !== -1;

  return foundInNieuwsberichten || foundInWerkinstructies;
});

const read = ref<boolean>(props.bericht.read);
watch(
  () => props.bericht.read,
  (newValue) => {
    read.value = newValue;
  }
);

const toggleReadIsLoading = ref<boolean>(false);

const toggleRead = async (): Promise<void> => {
  let toggleReadError = false;
  toggleReadIsLoading.value = true;

  if (read.value) {
    await unreadBericht(props.bericht.id).catch(() => (toggleReadError = true));
  }

  if (!read.value) {
    await readBericht(props.bericht.id).catch(() => (toggleReadError = true));
  }

  toggleReadIsLoading.value = false;

  if (!toggleReadError) {
    read.value = !read.value;
  }

  if (toggleReadError) {
    toast({
      text: "Oeps het lukt niet om dit bericht te markeren. Probeer het later opnieuw.",
      type: "error",
    });
  }
};

const localeString = (d: Date) =>
  d.toLocaleString("nl-NL", {
    day: "2-digit",
    month: "2-digit",
    year: "numeric",
    hour: "2-digit",
    minute: "2-digit",
  });

function processHtml(html: string) {
  const cleanedHtml = sanitizeHtmlToBerichtFormat(html);
  const increasedHeadings = increaseHeadings(cleanedHtml, props.level);
  return increasedHeadings;
}

const sanitized = computed(() => processHtml(props.bericht.content));

const handleToggleBerichtInContactmoment = (): void => {
  const type = props.bericht.type;
  const bericht = { url: props.bericht.url, title: props.bericht.title };

  type === "nieuws" && contactmomentStore.toggleNieuwsbericht(bericht);
  type === "werkinstructie" && contactmomentStore.toggleWerkinstructie(bericht);
};
</script>

<style lang="scss" scoped>
article {
  border-radius: var(--radius-medium);
  background-color: var(--color-secondary);
  padding: 0.75rem var(--text-margin);
  width: 100%;
  overflow: hidden;
  display: grid;
  gap: 0.75rem;

  time {
    color: var(--color-primary);
    display: block;
  }

  .heading-container {
    width: 100%;

    .heading-top-row {
      margin-block-end: var(--spacing-small);
      display: flex;
      justify-content: space-between;
      align-items: center;

      menu {
        display: flex;
        gap: var(--spacing-default);

        .save-bericht-to-contactmoment-checkbox {
          accent-color: var(--color-primary);
          transform: scale(1.25);
          margin: 0;
          margin-inline-start: 2px;
        }

        .toggle-read {
          color: var(--color-headings);
          position: relative;
          top: 4px;

          &:hover {
            color: var(--color-tertiary);
            cursor: pointer;
          }
          &:hover:disabled {
            cursor: wait;
          }
        }
      }
    }
  }

  .skills-container {
    & > small {
      font-size: 0.875rem;
    }

    & > *:not(:last-child) {
      margin-inline-end: var(--spacing-small);
    }
  }

  :deep(ul) {
    display: flex;
    flex-direction: column;
    gap: 0.25rem;
    list-style-type: disc;
    padding-left: var(--text-margin);
    line-height: var(--line-height-default);
  }

  :deep(p) {
    &:not(:first-child) {
      margin-top: 1em;
    }

    &:not(:last-child) {
      margin-bottom: 1em;
    }
  }

  &.read {
    .title {
      font-weight: normal;
    }

    .heading-top-row menu .toggle-read {
      color: var(--color-tertiary);
    }

    & > *:not(.heading-container) {
      display: none;
    }
  }

  .correct-header div {
    white-space: break-spaces;
  }
}
</style>
