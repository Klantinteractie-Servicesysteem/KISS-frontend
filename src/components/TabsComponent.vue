<template>
  <div>
    <nav role="tablist">
      <component
        v-for="{
          label,
          tag,
          href,
          isActive,
          panelId,
          tabId,
          enable,
        } in entries"
        :key="tabId"
        role="tab"
        :aria-selected="isActive ? 'true' : 'false'"
        :aria-controls="panelId"
        :href="href"
        :id="tabId"
        :is="tag"
        @click.prevent="enable"
      >
        <slot name="tab" :tabName="label">
          {{ label }}
        </slot>
      </component>
    </nav>
    <section
      v-for="{ name, isActive, panelId, tabId } in entries"
      :key="panelId"
      :id="panelId"
      role="tabpanel"
      :aria-labelledby="tabId"
      v-show="isActive"
    >
      <slot :name="name"></slot>
    </section>
  </div>
</template>

<script lang="ts" setup>
import { nanoid } from "nanoid";
import { watch, computed, useSlots } from "vue";

const props = defineProps({
  modelValue: {
    type: String,
    required: true,
  },
});

const instanceId = nanoid();
const slots = useSlots();
const emit = defineEmits(["update:modelValue"]);

const slotKeys = Object.keys(slots);
const entries = computed(() =>
  slotKeys
    .filter((name) => name.toLowerCase() !== "tab")
    .map((name, idx) => {
      const id = instanceId + idx;
      const isActive = props.modelValue === name;
      const panelId = id + "_panel";

      return {
        isActive,
        name,
        label: name,
        href: isActive ? undefined : "#" + panelId,
        tag: isActive ? "span" : "a",
        tabId: id + "_tab",
        panelId,
        enable() {
          emit("update:modelValue", name);
        },
      };
    })
);
watch(
  () => props.modelValue,
  (val) => {
    if (!slotKeys.length) return;
    if (!val || !slotKeys.includes(val)) {
      emit("update:modelValue", slotKeys[0]);
    }
  },
  { immediate: true }
);
</script>

<style lang="scss" scoped>
[role="tablist"] {
  display: flex;
  font-size: var(--tab-size, 1rem);
  font-weight: bold;
  color: var(--tab-color, var(--utrecht-heading-color));
  gap: var(--tab-gap, 0.5em);
}

[role="tab"] {
  text-decoration: none;
  color: inherit;

  --tab-spacing: var(--spacing-default);

  padding-inline: var(--spacing-large);
  padding-block: var(--spacing-default);
}

[role="tabpanel"],
[role="tab"][aria-selected="true"] {
  background-color: var(--tab-bg, var(--color-secondary));
}

[role="tabpanel"] {
  flex: 1;
}
</style>
