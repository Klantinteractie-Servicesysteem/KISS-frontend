<template>
  <input
    v-bind="$attrs"
    :id="id"
    :required="required"
    type="search"
    autocomplete="off"
    role="combobox"
    :aria-expanded="showList ? 'true' : 'false'"
    :aria-controls="listboxId"
    :aria-owns="listboxId"
    aria-autocomplete="list"
    aria-haspopup="listbox"
    @input="onInput"
    :value="modelValue"
    ref="inputRef"
    :disabled="disabled"
    @keydown.down.prevent="nextIndex"
    @keydown.up.prevent="previousIndex"
    @keydown.enter="selectItem(true)"
    @mouseenter="setMinIndex"
    @focus="onFocus"
    @blur="onBlur"
    :aria-activedescendant="
      showList ? `${listboxId}_${activeIndex}` : undefined
    "
  />
  <simple-spinner v-if="loading" class="spinner small" />
  <ul
    v-if="!loading && listItems.length && showList"
    class="utrecht-textbox"
    role="listbox"
    :id="listboxId"
    :aria-label="optionsLabel"
    :aria-required="required ? 'true' : undefined"
    ref="ulref"
    @mousedown="selectItem()"
  >
    <li
      v-for="(r, i) in listItems"
      :key="i"
      @mouseover="handleHover(i)"
      :class="{ active: i === activeIndex }"
      role="option"
      :id="`${listboxId}_${i}`"
    >
      <p>{{ r.value }}</p>
      <p v-if="r.description && showDescription">{{ r.description }}</p>
    </li>
  </ul>
</template>

<script lang="ts">
export default {
  inheritAttrs: false,
};
</script>

<script lang="ts" setup>
import { computed } from "vue";
import { ref, watch } from "vue";
import { nanoid } from "nanoid";
import { focusNextFormItem } from "@/helpers/html";
import SimpleSpinner from "./SimpleSpinner.vue";

export type DatalistItem = {
  value: string;
  description?: string;
};

const props = withDefaults(
  defineProps<{
    modelValue: string | undefined;
    listItems: DatalistItem[];
    exactMatch: boolean;
    required: boolean;
    disabled: boolean;
    loading: boolean;
    placeholder?: string;
    id?: string;
    showDescription?: boolean;
    optionsLabel: string;
  }>(),
  {
    showDescription: true,
  },
);

const listboxId = nanoid();

const minIndex = computed(() => (props.exactMatch ? 0 : -1));
const activeIndex = ref(minIndex.value);

function nextIndex() {
  if (
    showList.value &&
    activeIndex.value < props.listItems.length - 1 &&
    props.listItems.length
  ) {
    activeIndex.value += 1;
  } else {
    activeIndex.value = minIndex.value;
  }
  scrollIntoView();
}

function previousIndex() {
  if (!showList.value || !props.listItems.length) {
    activeIndex.value = minIndex.value;
  } else if (activeIndex.value > minIndex.value) {
    activeIndex.value -= 1;
  } else {
    activeIndex.value = props.listItems.length - 1;
  }
  scrollIntoView();
}

function setMinIndex() {
  activeIndex.value = minIndex.value;
}

function selectItem(focusNext = false) {
  showList.value = false;
  const item = props.listItems[activeIndex.value];
  if (item) {
    emit("update:modelValue", item.value);
  }
  if (focusNext && inputRef.value) {
    focusNextFormItem(inputRef.value);
  } else {
    setTimeout(() => {
      inputRef.value?.focus?.();
      showList.value = false;
    }, 100);
  }
}

const emit = defineEmits<{ "update:modelValue": [string] }>();

const inputRef = ref<HTMLInputElement>();
const ulref = ref();

function onInput(e: Event) {
  showList.value = true;
  if (!(e.currentTarget instanceof HTMLInputElement)) return;
  emit("update:modelValue", e.currentTarget.value);
}

function onFocus() {
  showList.value = !showList.value;
}

function onBlur() {
  showList.value = false;
}

const isScrolling = ref(false);

const showList = ref<boolean>(false);

watch(
  () => props.listItems,
  (r) => {
    activeIndex.value = Math.max(
      minIndex.value,
      Math.min(activeIndex.value, r.length - 1),
    );
  },
);

const matchingResult = computed(() => {
  if (
    Array.isArray(props.listItems) &&
    props.listItems.some((x) => x.value === props.modelValue)
  ) {
    return props.modelValue;
  }
  return "";
});

const validity = computed(() => {
  if (!props.modelValue && props.required) return "";
  if (!matchingResult.value && !!props.modelValue && props.exactMatch)
    return "Kies een optie uit de lijst.";
  return "";
});

watch([inputRef, validity], ([r, v]) => {
  if (!(r instanceof HTMLInputElement)) return;
  r.setCustomValidity(v);
});

watch(
  () => props.listItems,
  (r) => {
    activeIndex.value = Math.max(
      minIndex.value,
      Math.min(activeIndex.value, props.listItems.length - 1),
    );
  },
  { immediate: true, deep: true },
);

function isInViewport(el: HTMLElement) {
  const rect = el.getBoundingClientRect();
  return (
    rect.top >= 0 &&
    rect.left >= 0 &&
    rect.bottom <=
      (window.innerHeight || document.documentElement.clientHeight) &&
    rect.right <= (window.innerWidth || document.documentElement.clientWidth)
  );
}

function handleHover(i: number) {
  // ignore hover if we're scrolling, it's probably accidental
  if (isScrolling.value) return;
  activeIndex.value = i;
}

let timeoutId: number | NodeJS.Timeout;

function scrollIntoView() {
  const el = ulref.value;
  if (!(el instanceof HTMLElement)) return;
  const matchingLi = el.getElementsByTagName("li").item(activeIndex.value);

  if (matchingLi && !isInViewport(matchingLi)) {
    // when we scroll, the cursor can accidentally end up on an item
    // in that case, we don't want that item to be selected, because we are navigating using the arrow keys.
    // so let's wait for a bit before allowing items to be selected on hover.
    isScrolling.value = true;
    matchingLi.scrollIntoView(false);
    timeoutId && clearTimeout(timeoutId);
    timeoutId = setTimeout(() => {
      isScrolling.value = false;
    }, 500);
  }
}
</script>

<style lang="scss" scoped>
.spinner.small {
  font-size: 0.875rem;
  color: var(--color-black);
  position: absolute;
  inset-inline-end: 50%;
  transform: translateX(100%);
  z-index: 2;
}

[role="combobox"] {
  --utrecht-focus-outline-offset: 0;
}

ul {
  position: absolute;
  border-radius: var(--radius-default);
  display: grid;
  gap: var(--spacing-default);
  align-self: flex-end;
  transform: translateY(100%);
  z-index: 1;
  inset-block-end: -1px;
}

li {
  max-width: 100%;
  overflow-x: hidden;
}

li.active {
  background-color: var(--color-secondary);
}

li > p {
  font-size: 0.875rem;

  &:first-child {
    font-weight: bold;
  }
}
</style>
