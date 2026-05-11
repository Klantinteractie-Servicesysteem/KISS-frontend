<template>
  <nav role="tablist" ref="el" />
  <slot></slot>
</template>

<script setup lang="ts">
import { watchEffect, computed, reactive, provide, ref } from "vue";
import { tablistInjectionKey } from "./injection";

const props = defineProps<{ modelValue: string }>();
const emit = defineEmits<{ (e: "update:modelValue", v: string): void }>();

const current = computed({
  get: () => props.modelValue,
  set: (v) => emit("update:modelValue", v),
});

const tabIds = reactive(new Set<string>());

const setActive = (name: string) => (current.value = name);
const isActive = (name: string) => current.value === name;
const register = (name: string) => {
  tabIds.add(name);
};
const unregister = (name: string) => {
  tabIds.delete(name);
};

const el = ref<HTMLElement>();

provide(tablistInjectionKey, {
  el,
  setActive,
  isActive,
  register,
  unregister,
});

watchEffect(() => {
  if (!tabIds.has(current.value)) {
    setActive(tabIds.values().next().value || "");
  }
});
</script>

<style lang="scss" scoped>
[role="tablist"] {
  display: flex;
  font-size: var(--tab-size, 1rem);
  color: var(--tab-color, var(--utrecht-heading-color));
  gap: var(--tab-gap, 0.5em);
  overflow-x: auto;
}

[role="tablist"] > :deep([aria-selected="true"]) {
  font-weight: bold;
}
</style>
