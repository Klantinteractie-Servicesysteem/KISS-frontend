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
        <div class="contactmoment-header">
          <current-contactmoment-info />
          <contactmoment-vragen-menu />
        </div>
        <div class="notitie-tabs">
          <tab-list v-model="state.currentNotitieTab">
            <tab-list-item :label="NotitieTabs.Regulier" class="notitie-tab">
              <template #tab="{ label }">
                <span :title="label">Notitieblok</span>
              </template>

              <textarea
                aria-labelledby="notitieblok"
                id="cm-notitieblok"
                class="utrecht-textarea"
                v-model="
                  contactmomentStore.huidigContactmoment.huidigeVraag.notitie
                "
                placeholder="Schrijf een notitie…"
              />
            </tab-list-item>
            <tab-list-item
              :label="NotitieTabs.Contactverzoek"
              class="contactverzoek-tab"
            >
              <template #tab="{ label }">
                <span :title="label">Contactverzoek</span>
              </template>
              <utrecht-heading :level="2">Contactverzoek maken</utrecht-heading>
              <form @submit.prevent>
                <contactverzoek-formulier
                  v-model="
                    contactmomentStore.huidigContactmoment.huidigeVraag
                      .contactverzoek
                  "
                />
              </form>
            </tab-list-item>
          </tab-list>
        </div>

        <menu class="finisher">
          <li>
            <contactmoment-finisher />
          </li>
        </menu>
      </section>
    </template>
  </aside>
</template>

<script lang="ts" setup>
import ContactverzoekFormulier from "@/features/contact/contactverzoek/formulier/ContactverzoekFormulier.vue";
import { Heading as UtrechtHeading } from "@utrecht/component-library-vue";
import ContactmomentVragenMenu from "@/features/contact/contactmoment/ContactmomentVragenMenu.vue";
import { useContactmomentStore } from "@/stores/contactmoment";
import { ensureState } from "@/stores/create-store";
import { watch } from "vue";
import { useRoute } from "vue-router";
import {
  ContactmomentStarter,
  ContactmomentFinisher,
  CurrentContactmomentInfo,
  ContactmomentSwitcher,
} from "@/features/contact/contactmoment";
import { TabList, TabListItem } from "@/components/tabs";

enum NotitieTabs {
  Regulier = "Reguliere notitie",
  Contactverzoek = "Contactverzoek maken",
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
  },
);

watch(
  () => contactmomentStore.huidigContactmoment?.huidigeVraag?.notitie,
  (v) => {
    if (
      contactmomentStore.huidigContactmoment &&
      !contactmomentStore.huidigContactmoment.huidigeVraag.contactverzoek
        .isActive
    ) {
      contactmomentStore.huidigContactmoment.huidigeVraag.contactverzoek.interneToelichting =
        v;
    }
  },
);
</script>

<style lang="scss" scoped>
aside {
  background-color: var(--sidebar-color-1);
  padding-inline: 2px;
  display: flex;
  flex-direction: column;

  textarea.utrecht-textarea {
    padding: 0;
    border: none;
    outline: none;
    flex: 1;
    resize: none;
  }

  [role="tablist"] {
    height: 3rem;
  }

  #cm-notitieblok {
    height: 100%;
  }
}

.notitie-tabs {
  overflow: hidden;
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
    background-color: var(--color-primary);
  }

  :deep([role="tabpanel"]) {
    overflow-y: auto;
    display: flex;
    flex-direction: column;
    flex: 1;
    color: var(--utrecht-form-label-color);
    padding: var(--spacing-default);

    textarea::placeholder {
      font-style: italic;
    }
  }

  :deep([role="tab"]) {
    --utrecht-focus-outline-offset: -4px;

    &[aria-selected="true"] {
      color: var(--color-headings);
    }
  }
}

menu {
  align-self: center;

  --utrecht-button-min-inline-size: 17rem;
}

menu.starter {
  margin-block: 4rem var(--spacing-default);
  display: flex;
  flex-direction: column;
  gap: var(--spacing-default);

  li {
    display: flex;
    flex-direction: column;
    align-items: stretch;
  }
}

menu.finisher {
  margin-block: var(--spacing-large);
}

.within-moment {
  overflow: auto;
  display: flex;
  flex-direction: column;
  flex: 1;
  background-color: var(--color-white);
  color: var(--color-white);
  margin-block-start: var(--spacing-default);

  > h2 {
    margin-block-start: var(--spacing-small);
    margin-inline: var(--spacing-default);
    color: inherit;
  }

  > .contactmoment-header {
    background-color: var(--color-primary);
    padding-block: var(--spacing-default) var(--spacing-default);
    padding-inline: var(--spacing-small);

    --utrecht-heading-color: var(--color-white);

    > h3 {
      margin-block: var(--spacing-large) var(--spacing-small);
    }
  }
}
</style>
