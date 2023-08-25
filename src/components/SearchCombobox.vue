<template>
  <input
    v-bind="$attrs"
    :id="inputId"
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
  />
  <simple-spinner
    v-if="!matchingResult && listItems.loading"
    class="spinner small"
  />
  <ul
    v-if="showList"
    class="utrecht-textbox"
    role="listbox"
    :id="listboxId"
    :aria-labelledby="labelId"
    ref="ulref"
    @mousedown="selectItem()"
  >
    <li
      v-for="(r, i) in workingList"
      :key="i"
      @mouseover="handleHover(i)"
      :class="{ active: i === activeIndex }"
      role="option"
    >
      <article>
        <header>{{ r.value }}</header>
        <p v-if="r.description">{{ r.description }}</p>
      </article>
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
import { useFocus } from "@vueuse/core";
import { ref, watch, type PropType } from "vue";
import { nanoid } from "nanoid";
import { focusNextFormItem } from "@/helpers/html";
import type { ServiceData } from "@/services";
import SimpleSpinner from "./SimpleSpinner.vue";

export type DatalistItem = {
  value: string;
  description?: string;
};

const props = defineProps({
  modelValue: {
    type: String,
    default: undefined,
  },
  id: {
    type: String,
    default: undefined,
  },
  required: {
    type: Boolean,
    default: false,
  },
  listItems: {
    type: Object as PropType<ServiceData<DatalistItem[]>>,
    required: true,
  },
  exactMatch: {
    type: Boolean,
    default: false,
  },
  disabled: {
    type: Boolean,
    default: false,
  },
});

const generatedLabelId = nanoid();
const inputId = computed(() => props.id || generatedLabelId);
const labelId = nanoid();
const listboxId = nanoid();

const minIndex = computed(() => (props.exactMatch ? 0 : -1));
const activeIndex = ref(minIndex.value);

const workingList = ref<DatalistItem[]>([]);

function nextIndex() {
  if (
    showList.value &&
    activeIndex.value < workingList.value.length - 1 &&
    workingList.value.length
  ) {
    activeIndex.value += 1;
  } else {
    activeIndex.value = minIndex.value;
  }
  scrollIntoView();
}

function previousIndex() {
  if (!showList.value || !workingList.value.length) {
    activeIndex.value = minIndex.value;
  } else if (activeIndex.value > minIndex.value) {
    activeIndex.value -= 1;
  } else {
    activeIndex.value = workingList.value.length - 1;
  }
  scrollIntoView();
}

function setMinIndex() {
  activeIndex.value = minIndex.value;
}

function selectItem(focusNext = false) {
  const item = workingList.value[activeIndex.value];
  if (item) {
    emit("update:modelValue", item.value);
  }
  if (focusNext && inputRef.value) {
    focusNextFormItem(inputRef.value);
  } else {
    forceclosed.value = true;
    setTimeout(() => {
      inputRef.value?.focus?.();
    });
  }
}

const emit = defineEmits<{ "update:modelValue": [string] }>();

const inputRef = ref<HTMLInputElement>();
const ulref = ref();

function onInput(e: Event) {
  forceclosed.value = false;
  if (!(e.currentTarget instanceof HTMLInputElement)) return;
  emit("update:modelValue", e.currentTarget.value);
}

const isScrolling = ref(false);

const hasFocus = useFocus(inputRef);
const forceclosed = ref(false);

const showList = computed(
  () =>
    !props.disabled &&
    !forceclosed.value &&
    !!workingList.value.length &&
    hasFocus.focused.value,
);

watch(workingList.value, (r) => {
  activeIndex.value = Math.max(
    minIndex.value,
    Math.min(activeIndex.value, r.length - 1),
  );
});

const matchingResult = computed(() => {
  if (workingList.value.some((x) => x.value === props.modelValue))
    return props.modelValue;
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
    if (r.loading) return;
    if (!r.success) {
      workingList.value = [];
      return;
    }
    activeIndex.value = Math.max(
      minIndex.value,
      Math.min(activeIndex.value, r.data.length - 1),
    );
    workingList.value = r.data;
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
  color: black;
  position: absolute;
  inset-inline-end: 50%;
  transform: translateX(100%);
  z-index: 2;
}

ul {
  position: absolute;
  border-radius: 0;
  display: grid;
  gap: var(--spacing-default);
  align-self: flex-end;
  transform: translateY(100%);
  z-index: 1;
  inset-block-end: 0;
}

li {
  max-width: 100%;
  overflow-x: hidden;
}

li.active {
  background-color: var(--color-secondary);
}

article > p,
article > header {
  font-size: 0.875rem;
}

article > header {
  font-weight: bold;
}
</style>
