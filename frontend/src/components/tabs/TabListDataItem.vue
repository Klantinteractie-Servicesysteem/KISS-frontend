<template>
  <tab-list-item :label="label" :disabled="disabled" class="data-tabpanel">
    <template #tab="{ isActive }">
      <div class="data-tab" :data-active="isActive">
        <span :class="data.error ? 'alert icon-after' : ''">{{ label }}</span>
        <simple-spinner class="small-spinner" v-if="data.loading" />
      </div>
    </template>
    <template #default="{ isActive }">
      <div class="data-tabpanel" :data-active="isActive">
        <service-data-wrapper :data="data">
          <template #success="{ data }">
            <slot name="success" :data="data"></slot>
          </template>
        </service-data-wrapper>
      </div>
    </template>
  </tab-list-item>
</template>

<script setup lang="ts" generic="T">
import type { ServiceData } from "@/services";
import { computed } from "vue";
import ServiceDataWrapper from "@/components/ServiceDataWrapper.vue";
import TabListItem from "./TabListItem.vue";
import SimpleSpinner from "@/components/SimpleSpinner.vue";

const props = defineProps<{
  label: string;
  data: ServiceData<T>;
  disabled?: (data: T) => boolean;
}>();

const disabled = computed(
  () => props.data.success && !!props.disabled?.(props.data.data),
);
</script>

<style lang="scss" scoped>
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
</style>
