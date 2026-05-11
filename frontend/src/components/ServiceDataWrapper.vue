<template>
  <slot name="init" v-if="data.state === 'init'"></slot>
  <slot v-if="data.loading" name="loading">
    <simple-spinner />
  </slot>
  <slot name="error" v-if="data.error">
    <application-message
      messageType="error"
      message="Er is een fout opgetreden"
    />
  </slot>
  <slot v-if="data.success" name="success" :data="data.data" />
</template>

<script setup lang="ts" generic="T">
import type { ServiceData } from "@/services";
import SimpleSpinner from "./SimpleSpinner.vue";
import ApplicationMessage from "./ApplicationMessage.vue";
defineProps<{
  data: ServiceData<T>;
}>();
</script>
