<template>
  <a
    v-if="back"
    :href="back.path"
    class="icon-before chevron-left"
    @click.prevent.stop="$router.back()"
  >
    <slot>{{ back.title }}</slot>
  </a>
</template>

<script setup lang="ts">
import { computed } from "vue";
import { useRoute, type RouteLocationNormalizedLoaded } from "vue-router";

const route = useRoute();
const back = computed(() => {
  const { back } = route.meta as {
    back: RouteLocationNormalizedLoaded | undefined;
  };
  return (
    back && {
      path: back.fullPath,
      title:
        typeof back.meta.backTitle === "string" && back.meta.backTitle
          ? `Terug naar ${back.meta.backTitle}`
          : "Terug",
    }
  );
});
</script>

<style lang="scss" scoped>
a {
  text-decoration: none;
  color: inherit;
}
</style>
