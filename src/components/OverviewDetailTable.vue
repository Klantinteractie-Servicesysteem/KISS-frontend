<template>
  <template v-if="!currentCv">
    <utrecht-heading :level="level"
      ><slot :name="KNOWN_SLOTS.overview_heading"></slot
    ></utrecht-heading>
    <div class="table-wrapper">
      <table class="overview">
        <slot :name="KNOWN_SLOTS.caption"></slot>
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
                @click="currentCv = cv"
                :id="`details_${getKey(cv)}`"
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
      <slot :name="KNOWN_SLOTS.back_button" :record="currentCv"></slot>
    </button>
    <utrecht-heading :level="level" :id="generatedId"
      ><slot :name="KNOWN_SLOTS.detail_heading" :record="currentCv"></slot
    ></utrecht-heading>
    <dl :aria-labelledby="generatedId">
      <div v-for="(group, cIdx) in detailColumns" :key="cIdx">
        <template v-for="(column, cIdx2) in group" :key="cIdx2">
          <dt :class="{ highlight: highlight?.includes(column) }">
            {{ headings[column] }}
          </dt>
          <dd :class="{ highlight: highlight?.includes(column) }">
            <!-- @vue-ignore -->
            <slot :name="column" :value="currentCv[column]">{{
              currentCv[column]
            }}</slot>
          </dd>
        </template>
      </div>
    </dl>
  </div>
</template>

<script lang="ts">
const KNOWN_SLOTS = {
  caption: "caption",
  overview_heading: "overview-heading",
  detail_heading: "detail-heading",
  back_button: "back-button",
  detail_button: "detail-button",
} as const;

type KnownSlots = (typeof KNOWN_SLOTS)[keyof typeof KNOWN_SLOTS];
</script>

<script
  lang="ts"
  setup
  generic="T, Overview extends Array<keyof T>, Detail extends Array<keyof T>"
>
import { Heading as UtrechtHeading } from "@utrecht/component-library-vue";
import { nextTick, ref, useId } from "vue";
type KnownSlotFunctions = {
  [KNOWN_SLOTS.caption](): any;
  [KNOWN_SLOTS.overview_heading](): any;
  [KNOWN_SLOTS.detail_heading](props: { record: T }): any;
  [KNOWN_SLOTS.back_button](props: { record: T }): any;
  [KNOWN_SLOTS.detail_button](props: { record: T }): any;
};

type AllColumns = Overview[number] | Detail[number];

const { level = 2, keyProp } = defineProps<{
  records: Array<NonNullable<T>>;
  overviewColumns: Overview;
  detailColumns: Detail[];
  headings: {
    [K in AllColumns]: string;
  };
  highlight?: Detail;
  keyProp: keyof T;
  level?: 1 | 2 | 3 | 4;
}>();

defineSlots<
  {
    [K in Exclude<AllColumns, KnownSlots>]: (props: {
      value: NonNullable<T>[K];
    }) => any;
  } & KnownSlotFunctions
>();

const generatedId = useId();

const currentCv = ref<NonNullable<T>>();

const getKey = (v: T) => v[keyProp] as PropertyKey & string;

const goToOverview = () => {
  if (!currentCv.value) return;
  const cvUrl = getKey(currentCv.value);
  currentCv.value = undefined;
  nextTick(() => {
    document.getElementById(`details_${cvUrl}`)?.focus();
  });
};
</script>

<style scoped>
.max18char {
  max-width: 18ch;
  overflow: hidden;
  white-space: nowrap;
  text-overflow: ellipsis;
}

dl {
  background: var(--color-white);
  display: grid;
  grid-template-columns: minmax(min-content, max-content) auto;
  row-gap: 0.75rem;

  > div {
    display: grid;
    grid-template-columns: subgrid;
    grid-column: 1 / -1;
    padding-block: 0.5rem;
    border-bottom: 1px solid var(--color-accent);
  }
}

dd,
dt {
  padding-block: 0.5rem;
  padding-inline: 1rem;

  &:empty::before {
    content: "-";
  }
}

dt.highlight {
  border-inline-start: 4px var(--color-primary) solid;
}

.details {
  display: flex;
  flex-direction: column;
  gap: 0.5rem;
}

button {
  align-self: start;
}

.highlight {
  background-color: var(--color-secondary);
}

td:last-of-type {
  text-align: right;
  padding-inline-end: 0;
  padding-block: 0;

  .icon-after {
    padding-inline: 0.75rem;
    padding-block: 0.5rem;
  }
}
</style>
