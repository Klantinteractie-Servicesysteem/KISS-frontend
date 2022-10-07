<template>
  <aside>
    <menu class="starter" v-if="route.meta.showSearch">
      <li>
        <contactmoment-starter />
      </li>
      <li>
        <contactmoment-switcher />
      </li>
    </menu>
    <template
      v-if="contactmomentStore.contactmomentLoopt && route.meta.showNotitie"
    >
      <section
        class="within-moment"
        v-if="contactmomentStore.huidigContactmoment"
      >
        <current-contactmoment-info />
        <h2>Vragen</h2>
        <contactmoment-vragen-menu />
        <tabs-component v-model="state.currentNotitieTab" class="notitie-tabs">
          <template #tab="{ tabName }">
            <span
              :title="tabName"
              :class="[
                'icon-after',
                tabName === NotitieTabs.Terugbel ? 'phone-flip' : 'note',
              ]"
            ></span>
          </template>
          <template #[NotitieTabs.Regulier]>
            <utrecht-heading id="notitieblok" model-value :level="2"
              >Notitieblok</utrecht-heading
            >
            <textarea
              aria-labelledby="notitieblok"
              id="cm-notitieblok"
              class="utrecht-textarea"
              v-model="
                contactmomentStore.huidigContactmoment.huidigeVraag.notitie
              "
            />
          </template>
          <template #[NotitieTabs.Terugbel]>
            <p v-if="contactverzoekIsSentToMedewerker">
              Contactverzoek verstuurd naar
              {{ contactverzoekIsSentToMedewerker }}
            </p>
            <ContactverzoekFormulier
              v-else
              :huidige-vraag="
                contactmomentStore.huidigContactmoment.huidigeVraag
              "
              :huidige-klant="contactmomentStore.klantVoorHuidigeVraag"
              @start="handleContactverzoekStart"
              @submit="handleContactverzoekSubmit"
            />
          </template>
        </tabs-component>
      </section>
    </template>
  </aside>
</template>

<script lang="ts" setup>
import { ContactverzoekFormulier } from "@/features/contactverzoek";
import TabsComponent from "@/components/TabsComponent.vue";
import { UtrechtHeading } from "@utrecht/web-component-library-vue";
import ContactmomentVragenMenu from "@/features/contactmoment/ContactmomentVragenMenu.vue";
import { useContactmomentStore } from "@/stores/contactmoment";
import { ensureState } from "@/stores/create-store";
import { watch, computed } from "vue";
import { useRoute } from "vue-router";
import {
  ContactmomentStarter,
  CurrentContactmomentInfo,
  ContactmomentSwitcher,
} from "@/features/contactmoment";

enum NotitieTabs {
  Regulier = "Reguliere notitie",
  Terugbel = "Contactverzoek maken",
}

const contactmomentStore = useContactmomentStore();
const route = useRoute();

const state = ensureState({
  stateId: "Sidebar",
  stateFactory() {
    return {
      currentNotitieTab: NotitieTabs.Regulier,
    };
  },
});

watch(
  () => contactmomentStore.huidigContactmoment?.huidigeVraag,
  () => {
    state.reset();
  }
);

const contactverzoekIsSentToMedewerker = computed(
  () =>
    contactmomentStore.huidigContactmoment?.huidigeVraag.contactverzoek
      .medewerker
);

const handleContactverzoekStart = () => {
  if (
    !contactmomentStore.huidigContactmoment ||
    contactmomentStore.huidigContactmoment.huidigeVraag.contactverzoek
      .isSubmitted
  )
    return;
  contactmomentStore.huidigContactmoment.huidigeVraag.contactverzoek.isInProgress =
    true;
};

const handleContactverzoekSubmit = ({
  medewerker,
  url,
}: {
  medewerker: string;
  url: string;
}) => {
  if (!contactmomentStore.huidigContactmoment) return;
  const { contactverzoek } =
    contactmomentStore.huidigContactmoment.huidigeVraag;
  contactverzoek.url = url;
  contactverzoek.medewerker = medewerker;
  contactverzoek.isSubmitted = true;
  contactverzoek.isInProgress = false;
};
</script>

<style lang="scss" scoped>
aside {
  background-color: var(--sidebar-color-1);
  padding-inline: 2px;
  display: flex;
  flex-direction: column;

  textarea.utrecht-textarea {
    margin-block-start: var(--spacing-default);
    padding: 0px;
    border: none;
    outline: none;
    flex: 1;
    resize: none;
  }

  [role="tablist"] {
    height: 3rem;
  }
}

.notitie-tabs {
  margin-block-start: var(--spacing-small);
  flex: 1;
  display: flex;
  flex-direction: column;

  --tab-bg: var(--color-white);

  :deep([role="tablist"]) {
    padding: 0;
    justify-items: stretch;
    display: grid;
    grid-template-columns: 1fr 1fr;
    gap: 0;
    color: inherit;
  }

  :deep([role="tabpanel"]) {
    padding: var(--spacing-default);
    display: flex;
    flex-direction: column;
  }

  :deep([role="tab"]) {
    margin: 0;
    padding: 1rem;
    display: flex;
    align-items: center;
    justify-content: center;

    &[aria-selected="true"] {
      color: var(--color-headings);
    }
  }
}

.starter {
  align-self: center;
  margin-block-start: 4rem;
  margin-block-end: var(--spacing-default);
  display: flex;
  flex-direction: column;
  gap: var(--spacing-default);

  li {
    display: flex;
    flex-direction: column;
    align-items: stretch;
  }

  :deep(utrecht-button) {
    --utrecht-button-min-inline-size: 17rem;
  }
}

.within-moment {
  display: flex;
  flex-direction: column;
  flex: 1;
  background-color: var(--sidebar-color-2);
  border-start-start-radius: var(--radius-large);
  border-start-end-radius: var(--radius-large);
  color: var(--color-white);
  margin-block-start: var(--spacing-default);
  padding-block-start: var(--spacing-default);

  h2 {
    margin-block-start: var(--spacing-small);
    margin-inline: var(--spacing-default);
    color: inherit;
  }
}

:deep(h2) {
  font-size: 1.25rem;
}
</style>
