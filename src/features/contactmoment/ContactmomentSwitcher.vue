<template>
  <prompt-modal
    :dialog="dialog"
    message="Let op, je hebt het contactverzoek niet afgerond. Als je van contactmoment wisselt, wordt het contactverzoek niet verstuurd."
  />
  <details ref="detailsEl" v-if="contactmomentStore.contactmomenten.length">
    <summary class="utrecht-button">
      {{
        moments.length > 1
          ? `${moments.length} actieve contactmomenten`
          : "1 actief contactmoment"
      }}
    </summary>
    <menu>
      <li v-for="(moment, idx) in moments" :key="idx">
        <button
          :disabled="moment.isCurrent"
          @click="moment.enable"
          :class="{ 'is-current': moment.isCurrent }"
          :title="
            moment.isCurrent
              ? 'Huidig contactmoment'
              : 'Wissel naar dit contactmoment'
          "
        >
          <p v-if="moment.isCurrent" class="current-moment">
            Huidig contactmoment
          </p>
          <p class="name">
            {{ moment.description?.name || "Onbekende klant" }}
          </p>
          <p v-if="moment.description?.contact" class="contact">
            {{ moment.description.contact }}
          </p>
        </button>
      </li>
    </menu>
  </details>
</template>
<script lang="ts" setup>
import PromptModal from "@/components/PromptModal.vue";
import { useContactmomentStore } from "@/stores/contactmoment";
import { onClickOutside, useConfirmDialog } from "@vueuse/core";
import { computed, ref } from "vue";
import { useRouter } from "vue-router";
import { getKlantInfo } from "./helpers";

const router = useRouter();
const contactmomentStore = useContactmomentStore();
const moments = computed(() =>
  contactmomentStore.contactmomenten.map((moment) => {
    return {
      description: getKlantInfo(moment),
      isCurrent: moment === contactmomentStore.huidigContactmoment,
      async enable() {
        await ensureConfirmation();
        if (contactmomentStore.huidigContactmoment) {
          contactmomentStore.huidigContactmoment.route =
            router.currentRoute.value.fullPath;
        }
        contactmomentStore.switchContactmoment(moment);
        if (moment.route) {
          router.push(moment.route);
        }
      },
    };
  })
);

const ensureConfirmation = async () => {
  if (contactmomentStore.wouldLoseProgress) {
    const { isCanceled } = await dialog.reveal();
    if (isCanceled) {
      throw new Error();
    }
  }
};

const detailsEl = ref();

const dialog = useConfirmDialog();

onClickOutside(detailsEl, () => {
  const el = detailsEl.value;
  if (!(el instanceof HTMLElement)) return;
  el.removeAttribute("open");
});
</script>

<style lang="scss" scoped>
summary.utrecht-button {
  --utrecht-button-background-color: none;
  --utrecht-button-border-color: var(--color-white);
  border-style: solid;
  border-width: 2px;
  display: flex;
  justify-content: center;

  &:hover {
    cursor: pointer;
  }
}

details {
  position: relative;
}

menu {
  position: absolute;
  z-index: 1;
  margin-block-start: var(--spacing-small);
  padding-inline: var(--spacing-default);
  padding-block-end: var(--spacing-default);
  background: var(--color-white);
  inline-size: 100%;
  border-radius: var(--radius-default);
  box-shadow: var(--shadow-default);

  li {
    border-block-end: 1px solid black;
    padding-block: var(--spacing-default);
  }

  button {
    border-inline-start: 4px solid transparent;
    padding-inline-start: var(--spacing-small);
    inline-size: 100%;
    text-align: inherit;

    &:hover {
      cursor: pointer;
    }

    &:disabled:hover {
      cursor: not-allowed;
    }

    &.is-current {
      border-color: var(--color-accent);
    }
  }

  .current-moment {
    color: var(--color-accent);
    font-style: italic;
  }

  .name {
    font-weight: bold;
  }

  .contact,
  .current-moment {
    font-size: 0.875em;
  }
}
</style>
