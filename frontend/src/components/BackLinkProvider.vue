<template>
  <slot />
</template>

<script setup lang="ts">
import { watch } from "vue";
import { useRouter } from "vue-router";

const router = useRouter();

watch(router.currentRoute, (to, from) => {
  to.meta.back = from;
});
</script>
