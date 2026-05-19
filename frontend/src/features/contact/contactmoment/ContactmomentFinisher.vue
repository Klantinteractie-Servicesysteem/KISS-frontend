<template>
  <utrecht-button
    @click="onStopContactMoment"
    title="Contactmoment afronden"
    appearance="primary-action-button"
  >
    Afronden
  </utrecht-button>
</template>

<script lang="ts" setup>
import { useRouter } from "vue-router";
import { Button as UtrechtButton } from "@utrecht/component-library-vue";

const router = useRouter();
const onStopContactMoment = async () => router.push({ name: "afhandeling" }); // een link zou wellicht toepasselijker zijn, maar de styling adhv het designsystem wordt lastig.
</script>
