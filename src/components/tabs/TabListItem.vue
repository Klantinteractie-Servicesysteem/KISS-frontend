<template>
  <teleport :to="tablist.el.value" v-if="tablist?.el.value">
    <component
      role="tab"
      :aria-selected="isActive ? 'true' : 'false'"
      :aria-controls="panelId"
      :inert="isDisabled"
      :href="href"
      :id="tabId"
      :is="is"
      @click.prevent="activate"
    >
      <slot name="tab" :label="label" :is-active="isActive">
        <div class="data-tab">
          <span :class="error ? 'alert icon-after' : ''">{{ label }}</span>
          <simple-spinner class="small-spinner" v-if="loading" />
        </div>
      </slot>
    </component>
  </teleport>
  <section
    :key="panelId"
    :id="panelId"
    role="tabpanel"
    :aria-labelledby="tabId"
    v-show="isActive"
    v-bind="$attrs"
  >
    <slot
      :is-active="isActive"
      :set-loading="setLoading"
      :set-error="setError"
      :set-disabled="setDisabled"
    ></slot>
  </section>
</template>

<script setup lang="ts">
import { nanoid } from "nanoid";
import { inject, computed, watchEffect, ref } from "vue";
import { tablistInjectionKey } from "./injection";
import SimpleSpinner from "../SimpleSpinner.vue";
const props = defineProps<{ disabled?: boolean; label: string }>();
const tablist = inject(tablistInjectionKey);
const tabId = nanoid();
const panelId = tabId + "_panel";
const isActive = computed(() => tablist?.isActive(props.label) || false);
const is = computed(() => (!props.disabled && !isActive.value ? "a" : "span"));
const href = computed(() => (is.value === "a" ? "#" + panelId : undefined));
const activate = () => {
  if (!props.disabled) {
    tablist?.setActive(props.label);
  }
};

const error = ref(false);
const loading = ref(false);
const isDisabled = ref(false);

const setError = (v: boolean) => (error.value = v);
const setLoading = (v: boolean) => (loading.value = v);
const setDisabled = (v: boolean) => (isDisabled.value = v);

watchEffect(() => (isDisabled.value = props.disabled || false));

watchEffect(() => {
  if (isDisabled.value) {
    tablist?.unregister(props.label);
  } else {
    tablist?.register(props.label);
  }
});
</script>

<style lang="scss" scoped>
[role="tab"] {
  text-decoration: none;
  color: inherit;
  padding-inline: var(--spacing-large);
  padding-block: var(--spacing-default);

  &[inert] {
    color: var(--color-grey);
  }
}

[role="tab"][aria-selected="true"],
[role="tabpanel"] {
  background-color: var(--tab-bg, var(--color-secondary));
}

[role="tabpanel"] {
  padding: var(--spacing-large);

  :deep(.details-block) {
    padding: 0;
  }

  :deep(tbody > tr) {
    background: var(--color-white);
  }
}

.data-tab {
  position: relative;
  display: flex;
  gap: var(--spacing-default);
}

.small-spinner {
  --spinner-size: 1em;

  margin: 0;
  inset: 0;
  translate: 0 50%;
}
section {
  display: flex;
  flex-direction: column;
  gap: var(--spacing-large);
}
</style>
