<template>
  <details ref="detailsEl">
    <summary class="utrecht-button utrecht-button--secondary-action">
      {{ label }}
    </summary>
    <fieldset>
      <legend>{{ label }}</legend>
      <label
        v-for="[optionKey, optionLabel] in options"
        :for="name + '_check_' + optionKey"
        :key="name + '_check_' + optionKey"
      >
        <input
          type="checkbox"
          :name="name"
          :id="name + '_check_' + optionKey"
          :value="optionKey"
          v-model="currentValue"
          @change="emit('update:modelValue', currentValue)"
        />
        {{ optionLabel }}
      </label>
    </fieldset>
  </details>
</template>

<script lang="ts" setup>
import { onClickOutside } from "@vueuse/core";
import { ref, watch } from "vue";

const props = defineProps<{
  name: string;
  label: string;
  options: [number, string][];
  modelValue: number[];
}>();
const emit = defineEmits(["update:modelValue"]);
const currentValue = ref(props.modelValue);
const detailsEl = ref<HTMLElement>();

watch(
  () => props.modelValue,
  (v) => {
    currentValue.value = v;
  },
);

onClickOutside(detailsEl, () => {
  const el = detailsEl.value;
  if (!(el instanceof HTMLElement)) return;
  el.removeAttribute("open");
});
</script>

<style scoped lang="scss">
details > summary.utrecht-button.utrecht-button--secondary-action {
  display: flex;
  justify-content: space-between;
  border-radius: var(--radius-default);
  font-weight: 500;
  padding-block: var(--spacing-small);
  padding-inline: var(--padding-inline);
  width: 100%;
  user-select: none;
  border: var(--border-style);
  color: inherit;

  &:not(:disabled, .utrecht-button--disabled) {
    &:active,
    &:hover,
    &:focus {
      color: inherit;
      border: var(--border-style);
    }
  }

  > * {
    display: inline;
  }
}

details {
  --box-shadow: var(--shadow-default);
  --border-width: 1px;
  --border-style: currentcolor var(--border-width) solid;
  --padding-inline: 1.5rem;

  position: relative;
  min-width: 20rem;
  z-index: 1;
  display: flex;
  gap: var(--spacing-default);
  color: var(--color-black);
}

legend {
  font-size: 0;
  position: absolute;
}

fieldset {
  display: grid;
  position: absolute;
  inline-size: 100%;
  gap: var(--spacing-extrasmall);
  background-color: var(--color-white);
  box-shadow: var(--box-shadow);
  padding-block: var(--spacing-small);
  padding-inline: calc(1rem + var(--border-width)) var(--padding-inline); 
  border-radius: var(--radius-default);
  border: var(--border-style);

  label {
    display: flex;
    align-items: baseline;
    gap: var(--spacing-small);
    line-height: var(
      --utrecht-paragraph-line-height,
      var(--utrecht-document-line-height, normal)
    );
  }

  input {
    accent-color: var(--color-black);
    transform: scale(1.25) translateY(10%);
  }
}
</style>
