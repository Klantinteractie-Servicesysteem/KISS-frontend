<template>
  <simple-spinner v-if="links.loading" />
  <application-message
    v-if="links.error"
    :message-type="'error'"
    message="Er ging iets mis. Probeer het later nog eens."
  />
  <nav v-if="links.success">
    <dl v-if="links.data.length">
      <template v-for="groep in links.data" :key="groep.categorie">
        <dt>{{ groep.categorie }}</dt>
        <dd v-for="item in groep.items" :key="item.id">
          <a :href="item.url" rel="noopener noreferrer" target="_blank">
            {{ item.titel }}
          </a>
        </dd>
      </template>
    </dl>
    <p v-else>Geen links gevonden.</p>
  </nav>
</template>

<script lang="ts" setup>
import { useLinks } from "./service";
import SimpleSpinner from "@/components/SimpleSpinner.vue";
import ApplicationMessage from "@/components/ApplicationMessage.vue";

const links = useLinks();
</script>

<style lang="scss" scoped>
dt {
  font-weight: 600;
  margin-block-end: var(--spacing-small);

  &:not(:first-of-type) {
    margin-block-start: var(--spacing-default);
  }
}

a:not(:visited) {
  color: #2e71d7;
}
</style>
