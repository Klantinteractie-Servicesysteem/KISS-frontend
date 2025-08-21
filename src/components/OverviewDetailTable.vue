<template>
  <template v-if="!currentRecord">
    <utrecht-heading :level="level"
      ><slot :name="KNOWN_SLOTS.overview_heading"></slot
    ></utrecht-heading>
    <div class="table-wrapper">
      <table class="overview">
        <slot :name="KNOWN_SLOTS.table_caption"></slot>
        <thead>
          <tr>
            <th v-for="key in overviewColumns" :key="key">
              {{ headings[key] }}
            </th>
            <th><span class="sr-only">Details</span></th>
          </tr>
        </thead>
        <tbody>
          <tr v-for="cv in records" :key="getKey(cv)">
            <td
              v-for="column in overviewColumns"
              :key="`${getKey(cv)}_${column.toString()}`"
            >
              <!-- @vue-ignore -->
              <slot :name="column" :value="cv[column]">{{ cv[column] }}</slot>
            </td>
            <td>
              <button
                type="button"
                class="icon-after chevron-right"
                @click="currentRecord = cv"
                :id="`${generatedId}_details_${getKey(cv)}`"
              >
                <span class="sr-only">
                  <slot :name="KNOWN_SLOTS.detail_button" :record="cv"></slot>
                </span>
              </button>
            </td>
          </tr>
        </tbody>
      </table>
    </div>
  </template>
  <div v-else class="details">
    <button @click="goToOverview" class="icon-before chevron-left">
      <slot :name="KNOWN_SLOTS.back_button" :record="currentRecord"></slot>
    </button>
    <utrecht-heading :level="level" :id="generatedId"
      ><slot :name="KNOWN_SLOTS.detail_heading" :record="currentRecord"></slot
    ></utrecht-heading>
    <dl :aria-labelledby="generatedId">
      <div v-for="(group, cIdx) in detailGroups" :key="cIdx">
        <template v-for="(column, cIdx2) in group" :key="cIdx2">
          <dt :class="{ highlight: highlight?.includes(column) }">
            {{ headings[column] }}
          </dt>
          <dd :class="{ highlight: highlight?.includes(column) }">
            <!-- @vue-ignore -->
            <slot :name="column" :value="currentRecord[column]">{{
              currentRecord[column]
            }}</slot>
          </dd>
        </template>
      </div>
    </dl>
  </div>
</template>

<script
  lang="ts"
  setup
  generic="
    T,
    OverviewColumns extends Array<keyof T>,
    DetailGroupProperties extends Array<keyof T>
  "
>
import { Heading as UtrechtHeading } from "@utrecht/component-library-vue";
import { nextTick, ref, useId, watchEffect } from "vue";

const KNOWN_SLOTS = {
  table_caption: "table-caption",
  overview_heading: "overview-heading",
  detail_heading: "detail-heading",
  back_button: "back-button",
  detail_button: "detail-button",
} as const;

// we need to inform vue that the slot names are dynamic based on the props
type KnownSlotFunctions = {
  [KNOWN_SLOTS.table_caption](): unknown;
  [KNOWN_SLOTS.overview_heading](): unknown;
  [KNOWN_SLOTS.detail_heading](props: { record: T }): unknown;
  [KNOWN_SLOTS.back_button](props: { record: T }): unknown;
  [KNOWN_SLOTS.detail_button](props: { record: T }): unknown;
};

defineSlots<
  {
    [K in AllColumns]: (props: { value: NonNullable<T>[K] }) => unknown;
  } & KnownSlotFunctions
>();

type AllColumns = OverviewColumns[number] | DetailGroupProperties[number];

const { level = 2, keyProp } = defineProps<{
  /** The records to show. This is generic so we can type check the other props */
  records: Array<NonNullable<T>>;
  /** The columns to show in the overview table */
  overviewColumns: OverviewColumns;
  /** Grouped properties to show in the description list when you click on an item from the table */
  detailGroups: DetailGroupProperties[];
  /** The headings for all the properties you chose in overview-columns and detail-groups */
  headings: {
    [K in AllColumns]: string;
  };
  /** The properties that need to be highlighted */
  highlight?: DetailGroupProperties;
  /** The property that uniquely identifies a record */
  keyProp: keyof T;
  /** The level of the headings to be used for the overview and the details */
  level?: 1 | 2 | 3 | 4;
}>();

const emit = defineEmits<{
  (e: "itemSelected", id: string | undefined): void;
}>();

const generatedId = useId();

const currentRecord = ref<NonNullable<T>>();

const getKey = (v: T) => v[keyProp] as PropertyKey & string;

const goToOverview = () => {
  if (!currentRecord.value) return;
  const key = getKey(currentRecord.value);
  currentRecord.value = undefined;
  nextTick(() => {
    document.getElementById(`${generatedId}_details_${key}`)?.focus();
  });
};

watchEffect(() => {
  emit(
    "itemSelected",
    currentRecord.value ? getKey(currentRecord.value) : undefined,
  );
});
</script>

<style scoped>
dl {
  background: var(--color-white);
  display: grid;
  grid-template-columns: minmax(min-content, max-content) auto;
  row-gap: 0.75rem;

  > div {
    display: grid;
    grid-template-columns: subgrid;
    grid-column: 1 / -1;
    padding-block: var(--spacing-small);
    border-bottom: 1px solid var(--color-accent);
  }
}

dd,
dt {
  --dd-dt-padding-inline: var(--spacing-default);

  padding-block: var(--spacing-small);
  padding-inline: var(--dd-dt-padding-inline);

  &:empty::before {
    content: "-";
  }
}

dt.highlight {
  --highlight-border-width: 4px;

  border-inline-start: var(--highlight-border-width) var(--color-primary) solid;
  padding-inline-start: calc(
    var(--dd-dt-padding-inline) - var(--highlight-border-width)
  );
}

.details {
  display: flex;
  flex-direction: column;
  gap: 0.5rem;
}

button {
  align-self: start;
  cursor: pointer;
}

.highlight {
  background-color: var(--color-secondary);
}

td:last-of-type {
  text-align: end;
  padding-inline: 0;
  padding-block: var(--spacing-extrasmall);

  /* scroll shadow */
  background: inherit;
  position: sticky;
  inset-inline-end: 0;
  animation: scroll-shadow-button;
  animation-timeline: scroll(inline);

  /* icon */

  .icon-after {
    padding-inline: var(--utrecht-button-icon-gap);
    padding-block: var(--spacing-extrasmall);
  }
}

@keyframes scroll-shadow-button {
  from {
    filter: drop-shadow(-5px 0 10px rgb(0 0 0 / 25%));
    clip-path: inset(0 0 0 -100%);
  }

  to {
    clip-path: inset(0 0 0 -100%);
  }
}
</style>
