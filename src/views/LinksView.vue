<template>
  <utrecht-heading :level="1">Links</utrecht-heading>
  <link-list />
</template>
<script lang="ts" setup>
import { Heading as UtrechtHeading } from "@utrecht/component-library-vue";
import LinkList from "@/features/informatie/links/LinkList.vue";
</script>

<style lang="scss" scoped>
utrecht-heading {
  margin-block-end: var(--spacing-default);
}
</style>
