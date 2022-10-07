<template>
  <utrecht-button
    model-value
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
import { UtrechtButton } from "@utrecht/web-component-library-vue";
import { useContactmomentStore } from "@/stores/contactmoment";
import { useRouter } from "vue-router";
import { useAttrs } from "vue";
import { useConfirmDialog } from "@vueuse/core";
import PromptModal from "@/components/PromptModal.vue";

const beforeStopDialog = useConfirmDialog();

const attrs = useAttrs();

const router = useRouter();
const contactmomentStore = useContactmomentStore();

const onStartContactMoment = async () => {
  if (attrs.disabled) return;
  if (contactmomentStore.wouldLoseProgress) {
    await waitForConfirmation();
  }
  contactmomentStore.start();
  router.push("/klanten");
  setTimeout(() => {
    document.getElementById("cm-notitieblok")?.focus();
  });
};

async function waitForConfirmation() {
  const { isCanceled } = await beforeStopDialog.reveal();
  if (isCanceled) {
    throw new Error("canceled");
  }
}
</script>

<style scoped lang="scss">
utrecht-button {
  --utrecht-button-min-inline-size: 15rem;
  --utrecht-button-background-color: var(--color-accent);
}
</style>
