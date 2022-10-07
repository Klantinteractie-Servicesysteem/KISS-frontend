<template>
  <prompt-modal
    :dialog="dialog"
    message="Let op, je hebt het contactverzoek niet afgerond. Als je deze vraag verlaat, wordt het contactverzoek niet verstuurd."
  />
  <menu class="vragen-menu" v-if="vragen">
    <li>
      <button
        class="icon-after plus new-question"
        type="button"
        title="Nieuwe vraag"
        @click="startNieuweVraag"
      ></button>
    </li>
    <li v-for="(vraag, idx) in vragen" :key="idx">
      <button
        type="button"
        :disabled="vraag.isCurrent"
        :title="vraag.isCurrent ? 'Huidige vraag' : `Ga naar vraag ${idx + 1}`"
        @click="vraag.switchVraag"
      >
        {{ idx + 1 }}
      </button>
    </li>
  </menu>
</template>
<script lang="ts" setup>
import { useContactmomentStore } from "@/stores/contactmoment";
import { useConfirmDialog } from "@vueuse/core";
import PromptModal from "@/components/PromptModal.vue";
import { computed } from "@vue/reactivity";

const contactmomentStore = useContactmomentStore();
const dialog = useConfirmDialog();

const vragen = computed(() =>
  contactmomentStore.huidigContactmoment?.vragen.map((vraag) => {
    return {
      isCurrent: contactmomentStore.huidigContactmoment?.huidigeVraag === vraag,
      async switchVraag() {
        if (contactmomentStore.wouldLoseProgress) {
          await waitForConfirmation();
        }
        contactmomentStore.switchVraag(vraag);
      },
    };
  })
);

async function startNieuweVraag() {
  if (contactmomentStore.wouldLoseProgress) {
    await waitForConfirmation();
  }
  contactmomentStore.startNieuweVraag();
  setTimeout(() => {
    document.getElementById("cm-notitieblok")?.focus();
  });
}

async function waitForConfirmation() {
  const { isCanceled } = await dialog.reveal();
  if (isCanceled) {
    throw new Error("canceled");
  }
}
</script>

<style lang="scss" scoped>
.vragen-menu {
  display: flex;
  flex-wrap: wrap;
  gap: var(--spacing-small);
  padding: var(--spacing-small);

  button {
    font-weight: bold;
    display: flex;
    align-items: center;
    justify-content: center;
    inline-size: var(--spacing-large);
    block-size: var(--spacing-large);
    border-radius: 50%;

    &:hover {
      cursor: pointer;
    }

    &:disabled {
      background: var(--color-white);
      color: var(--color-headings);
      cursor: not-allowed;
    }
  }
}
</style>
