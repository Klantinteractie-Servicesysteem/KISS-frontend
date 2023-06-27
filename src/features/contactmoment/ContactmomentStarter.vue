<template>
  <utrecht-button
    class="start-button"
    type="button"
    @click="onStartContactMoment"
    v-bind="$attrs"
  >
    Nieuw contactmoment
  </utrecht-button>

  <prompt-modal
    :dialog="beforeStopDialog"
    message="Let op, je hebt het contactverzoek niet afgerond. Als je een nieuw contactmoment start, wordt het contactverzoek niet verstuurd."
  />
</template>

<script lang="ts">
export default {
  inheritAttrs: false,
};
</script>

<script setup lang="ts">
import { Button as UtrechtButton } from "@utrecht/component-library-vue";
import { useContactmomentStore } from "@/stores/contactmoment";
import { useRouter } from "vue-router";
import { nextTick, useAttrs } from "vue";
import { useConfirmDialog } from "@vueuse/core";
import PromptModal from "@/components/PromptModal.vue";

const beforeStopDialog = useConfirmDialog();

const attrs = useAttrs();

const router = useRouter();
const contactmomentStore = useContactmomentStore();

const onStartContactMoment = async () => {
  if (attrs.disabled) return;

  if (contactmomentStore.huidigContactmoment) {
    contactmomentStore.huidigContactmoment.route =
      router.currentRoute.value.fullPath;
  }
  contactmomentStore.start();
  router.push("/personen");
  nextTick(() => {
    document.getElementById("cm-notitieblok")?.focus();
  });
};
</script>

<style scoped lang="scss">
.start-button {
  --utrecht-button-min-inline-size: 15rem;
  --utrecht-button-background-color: var(--color-accent);
  --utrecht-button-color: var(--color-white);
  --utrecht-button-hover-background-color: var(--color-accent-hover);
  --utrecht-button-hover-color: black;
}
</style>
