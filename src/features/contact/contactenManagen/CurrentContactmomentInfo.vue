<template v-if="contactmomentStore.huidigContactmoment">
  <article>
    <prompt-modal
      :dialog="beforeStopDialog"
      message="Let op, je hebt het contactverzoek niet afgerond. Als je dit contactmoment afsluit, wordt het contactverzoek niet verstuurd."
    />
    <header>
      <h2>{{ klantInfo?.name || "Onbekend" }}</h2>
    </header>
  </article>
</template>

<script setup lang="ts">
import {
  useContactmomentStore,
  type ContactmomentState,
} from "@/stores/contactmoment/index";
import { useConfirmDialog } from "@vueuse/core";
import { computed } from "vue";
import PromptModal from "@/components/PromptModal.vue";

const beforeStopDialog = useConfirmDialog();

const contactmomentStore = useContactmomentStore();

const activeKlantInCurrentVraag = (contactmoment: ContactmomentState) => {
  // currently the first klant is considered the
  // current active klant
  // this will have to be replaced by a proper mechanism
  // to keep track of the active contactmoment/vraag/klant
  const activeKlant = contactmoment?.huidigeVraag?.klanten
    ? contactmoment?.huidigeVraag?.klanten
        .filter(({ shouldStore }) => shouldStore === true)
        .find(Boolean)
    : null;

  if (activeKlant) {
    const name =
      [
        activeKlant.klant.voornaam,
        activeKlant.klant.voorvoegselAchternaam,
        activeKlant.klant.achternaam,
      ]
        .filter(Boolean)
        .join(" ") || activeKlant.klant.bedrijfsnaam;

    const email = activeKlant.klant.emailadressen.find(Boolean);
    const phone = activeKlant.klant.telefoonnummers.find(Boolean);

    const contact = email || phone;

    return {
      name,
      contact,
    };
  }
};

const klantInfo = computed(() =>
  contactmomentStore.huidigContactmoment
    ? activeKlantInCurrentVraag(contactmomentStore.huidigContactmoment)
    : undefined,
);
</script>

<style lang="scss" scoped>
article {
  display: flex;
  flex-wrap: wrap;
  align-items: center;
  justify-content: space-between;
  gap: var(--spacing-default);

  --utrecht-button-background-color: var(--color-error);
  --utrecht-button-hover-background-color: var(--color-error-hover);
  --utrecht-button-hover-color: var(--color-black);
  --utrecht-button-min-inline-size: auto;

  h2 {
    font-size: 1rem;
    font-weight: normal;
    margin-block-start: 0;
  }
}
</style>
