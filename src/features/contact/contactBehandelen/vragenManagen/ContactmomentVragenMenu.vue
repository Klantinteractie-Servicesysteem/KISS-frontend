<template>
  <prompt-modal
    :dialog="dialog"
    message="Let op, je hebt het contactverzoek niet afgerond. Als je deze vraag verlaat, wordt het contactverzoek niet verstuurd."
  />
  <div>
    <utrecht-heading :level="3">Vragen</utrecht-heading>

    <menu class="vragen-menu" v-if="vragen">
      <li v-for="(vraag, idx) in vragen" :key="idx">
        <utrecht-button
          class="icon-only"
          type="button"
          :disabled="vraag.isCurrent"
          :title="
            vraag.isCurrent ? 'Huidige vraag' : `Ga naar vraag ${idx + 1}`
          "
          @click="vraag.switchVraag"
        >
          {{ idx + 1 }}
        </utrecht-button>
      </li>
      <li>
        <utrecht-button
          class="icon-after plus new-question icon-only"
          type="button"
          title="Nieuwe vraag"
          @click="startNieuweVraag"
        ></utrecht-button>
      </li>
    </menu>
  </div>
</template>
<script lang="ts" setup>
import { useContactmomentStore } from "@/stores/contactmoment/index";
import { useConfirmDialog } from "@vueuse/core";
import PromptModal from "@/components/PromptModal.vue";
import { nextTick, computed } from "vue";
import {
  Button as UtrechtButton,
  Heading as UtrechtHeading,
} from "@utrecht/component-library-vue";

const contactmomentStore = useContactmomentStore();
const dialog = useConfirmDialog();

const vragen = computed(() =>
  contactmomentStore.huidigContactmoment?.vragen.map((vraag) => {
    return {
      isCurrent: contactmomentStore.huidigContactmoment?.huidigeVraag === vraag,
      async switchVraag() {
        contactmomentStore.switchVraag(vraag);
      },
    };
  }),
);

async function startNieuweVraag() {
  contactmomentStore.startNieuweVraag();
  nextTick(() => {
    document.getElementById("cm-notitieblok")?.focus();
  });
}
</script>

<style lang="scss" scoped>
div {
  --utrecht-heading-3-font-size: 1rem;
  --utrecht-heading-3-font-weight: normal;

  display: flex;
  gap: var(--spacing-default);
  margin-block-start: var(--spacing-extrasmall);
  align-items: baseline;
}

/* stylelint-disable custom-property-pattern */
.vragen-menu {
  display: flex;
  flex-wrap: wrap;
  gap: var(--spacing-small);

  button {
    --utrecht-button-inline-size: 1.5rem;
    --_utrecht-button-hover-color: var(--color-white);
    --_utrecht-button-active-color: var(--color-white);
    --_utrecht-button-focus-color: var(--color-white);

    color: var(--color-white);

    &:disabled {
      background: var(--color-white);
      color: var(--color-headings);
      cursor: not-allowed;
    }

    &.icon-only {
      font-weight: normal;
      border-radius: 0;
    }

    &.new-question {
      border: none;
    }
  }
}
</style>
