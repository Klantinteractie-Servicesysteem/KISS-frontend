<template>
  <ul
    v-if="items.length"
    class="overview expandable-list"
    :data-column-count="columnCount"
  >
    <li class="header-row" ref="headerRef">
      <slot name="header"></slot>
    </li>
    <li v-for="item in items" :key="item[itemKey]" class="expandable-item">
      <details @click="toggleDetails">
        <slot name="item" :item="item"></slot>
      </details>
    </li>
  </ul>
</template>

<script lang="ts" setup>
// TODO we now render a ul within a table, this is not valid HTML
// let's try to refactor this, possibly to a regular table with expandable extra rows
import { computed } from "vue";
import { ref } from "vue";

defineProps<{
  items: any[];
  itemKey: string;
}>();

const toggleDetails = (e: Event) => {
  e.preventDefault();
  // if the summary element is the target, we are either
  // 1. clicking the chevron rendered via css with ::after
  // 2. using keyboard navigation and pressing enter
  // these are the only cases in which we want to open/close the details
  if (e.target instanceof HTMLElement && e.target.tagName === "SUMMARY") {
    const details = e.target.closest("details");
    if (details) {
      details.open = !details.open;
    }
  }
};

const headerRef = ref<HTMLElement>();
const columnCount = computed(() => headerRef.value?.childElementCount || 0);
</script>

<style lang="scss" scoped>
ul {
  --column-count: 4;
  --columns: repeat(var(--column-count), 1fr) /* regular columns */
    calc(1rem + var(--column-padding) * 2); // column for chevron

  --spinner-size: 1em;
  --utrecht-focus-outline-offset: 0;

  display: grid;
  list-style: none;
  padding: 0;

  @for $i from 1 through 10 {
    &[data-column-count="#{$i}"] {
      --column-count: #{$i};
    }
  }
}

.header-row {
  display: grid;
  grid-template-columns: var(--columns);
}

:deep(summary) {
  gap: 0;
  list-style: none;
  display: grid;
  grid-template-columns: var(--columns);

  &::after {
    justify-self: center;
  }
}

:deep(details) {
  display: grid;
  background: var(--color-white);
}

:deep(dt) {
  font-weight: bold;
}

:deep(dl) {
  display: grid;
  grid-template-columns: var(--columns);

  dt {
    grid-column: 1 / 1;
  }

  dd {
    // fill the rest of the columns except the chevron column
    grid-column: 2 / -2;
  }
}

:deep(.tekst) {
  max-width: 90ch;
  white-space: pre-wrap;
}
</style>
