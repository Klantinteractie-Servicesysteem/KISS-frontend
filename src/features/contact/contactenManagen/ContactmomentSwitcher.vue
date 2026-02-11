<template>
  <prompt-modal
    :dialog="dialog"
    message="Let op, je hebt het contactverzoek niet afgerond. Als je van contactmoment wisselt, wordt het contactverzoek niet verstuurd."
  />
  <details ref="detailsEl" v-if="contactmomentStore.contactmomenten.length">
    <summary class="utrecht-button utrecht-button--secondary-action">
      Actief ({{ moments.length }})
    </summary>
    <menu>
      <li v-for="(moment, idx) in moments" :key="idx">
        <utrecht-button
          appearance="subtle-button"
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
        </utrecht-button>
      </li>
    </menu>
  </details>
</template>
<script lang="ts" setup>
import PromptModal from "@/components/PromptModal.vue";
import {
  useContactmomentStore,
  type ContactmomentKlant,
  type ContactmomentState,
} from "@/stores/contactmoment/index";
import { Button as UtrechtButton } from "@utrecht/component-library-vue";
import { onClickOutside, useConfirmDialog } from "@vueuse/core";
import { computed, ref } from "vue";
import { useRouter } from "vue-router";

const router = useRouter();
const contactmomentStore = useContactmomentStore();

const closeDetails = () => {
  const el = detailsEl.value;
  if (!(el instanceof HTMLElement)) return;
  el.removeAttribute("open");
};

const moments = computed(() => {
  return contactmomentStore.contactmomenten.map((moment) => {
    return {
      description: getKlantInfo(moment),
      isCurrent: moment === contactmomentStore.huidigContactmoment,
      async enable() {
        if (contactmomentStore.huidigContactmoment) {
          contactmomentStore.huidigContactmoment.route =
            router.currentRoute.value.fullPath;
        }
        contactmomentStore.switchContactmoment(moment);
        if (moment.route) {
          router.push(moment.route);
        }
        closeDetails();
      },
    };
  });
});

const detailsEl = ref();

const dialog = useConfirmDialog();

onClickOutside(detailsEl, closeDetails);

function _getKlantInfo(klant: ContactmomentKlant) {
  const name =
    [klant.voornaam, klant.voorvoegselAchternaam, klant.achternaam]
      .filter(Boolean)
      .join(" ") || klant.bedrijfsnaam;

  const email = klant.emailadressen.find(Boolean);
  const phone = klant.telefoonnummers.find(Boolean);

  const contact = email || phone;

  return {
    name,
    contact,
  };
}

function getKlantInfo(contactmoment: ContactmomentState) {
  const klanten = contactmoment.vragen
    .flatMap(({ klanten }) => klanten)
    .filter(({ shouldStore }) => shouldStore)
    .map(({ klant }) => klant);

  const infos = klanten.map(_getKlantInfo);

  // it just rturns the first one.
  // due to circumstances that's almost always the right one
  // however not always. this should be replaced with
  // a proper mechanism to get the info of the 'current' klant
  return infos.find((info) => info.name || info.contact);
}
</script>

<style lang="scss" scoped>
summary {
  display: flex;
  justify-content: center;

  --utrecht-button-secondary-action-color: var(--color-white);
  --utrecht-button-secondary-action-border-color: var(--color-white);
}

details {
  position: relative;
}

menu {
  position: absolute;
  z-index: 1;
  inset-inline-end: 0;
  inline-size: 15rem;
  margin-block-start: var(--spacing-small);
  padding-inline: var(--spacing-default);
  padding-block-end: var(--spacing-default);
  background: var(--color-white);
  border-radius: var(--radius-default);
  box-shadow: var(--shadow-default);

  li {
    border-block-end: 1px solid var(--color-black);
    padding-block: var(--spacing-default);
  }

  button {
    --utrecht-button-border-radius: 0;
    --utrecht-button-min-inline-size: 100%;
    --utrecht-button-hover-background-color: var(--color-secondary);

    display: block;
    border: none;
    border-inline-start: 4px solid transparent;
    padding-inline-start: var(--spacing-small);
    text-align: inherit;

    &:disabled:hover {
      cursor: not-allowed;
    }

    &.is-current {
      border-color: var(--color-accent);
    }
  }

  .current-moment {
    font-style: italic;
  }

  .name {
    font-weight: 400;
  }

  .contact,
  .current-moment {
    font-size: 0.875em;
  }
}
</style>
