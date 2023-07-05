<template>
  <Suspense>
    <div class="wrapper" ref="wrapperRef">
      <textarea
        :value="modelValue"
        v-bind="$attrs"
        @focus="editableElement?.focus()"
        tabindex="-1"
      ></textarea>
      <CkEditorAsync v-model="modelValue" v-bind="$attrs" />
    </div>
    <template #fallback>
      <SimpleSpinnerVue />
    </template>
  </Suspense>
</template>

<script lang="ts">
export default {
  inheritAttrs: false,
};
</script>

<script setup lang="ts">
import SimpleSpinnerVue from "@/components/SimpleSpinner.vue";
import CkEditorAsync from "./CkEditorAsync.vue";
import { computed, ref } from "vue";
const props = defineProps<{ modelValue?: string }>();
const emit = defineEmits<{ (e: "update:modelValue", v?: string): void }>();
const modelValue = computed({
  get: () => props.modelValue,
  set: (val) => emit("update:modelValue", val),
});

const wrapperRef = ref<HTMLDivElement>();
const editableElement = computed(() => {
  const el = wrapperRef.value?.querySelector("[contenteditable=true]");
  if (el && el instanceof HTMLElement) return el;
  return undefined;
});
</script>

<style lang="scss" scoped>
.wrapper {
  display: grid;
  grid-template-areas: "stacked";

  :deep(> *) {
    grid-area: stacked;
  }

  > textarea {
    opacity: 0;
  }
}

:deep(.ck-editor ol),
:deep(.ck-editor ul) {
  padding-left: var(--spacing-default);
}

:deep(.ck-editor p:not(:last-child)) {
  margin-block-end: 1em;
}
</style>
