<template>
  <slot></slot>
</template>

<script lang="ts" setup>
import { useValidationMessages } from "./useValidationMessages";

const props = defineProps<{
  value: string;
  validate: (val: string) => string[];
}>();

useValidationMessages(props);
</script>

<style lang="scss">
.error {
  color: var(--color-error);
}
</style>
