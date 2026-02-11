<template>
  <aside>
    <menu class="starter" v-if="route.meta.showSearch">
      <li>
        <contactmoment-starter
          v-if="userStore.user.isLoggedIn && userStore.user.isKcm"
        />
      </li>
      <li v-if="contactmomentStore.contactmomenten.length">
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
          <li><contactmoment-canceller /></li>
          <li><contactmoment-finisher /></li>
        </menu>
      </section>
    </template>
  </aside>
</template>

<script lang="ts" setup>
import ContactverzoekFormulier from "@/features/contact/contactBehandelen/vraagBehandelen/contactverzoek/ContactverzoekFormulier.vue";
import ContactmomentVragenMenu from "@/features/contact/contactBehandelen/vragenManagen/ContactmomentVragenMenu.vue";
import { useContactmomentStore } from "@/stores/contactmoment/index";
import { ensureState } from "@/stores/create-store";
import { watch } from "vue";
import { useRoute } from "vue-router";
import ContactmomentStarter from "@/features/contact/contactenManagen/ContactmomentStarter.vue";
import ContactmomentFinisher from "@/features/contact/contactenManagen/ContactmomentFinisher.vue";
import CurrentContactmomentInfo from "@/features/contact/contactenManagen/CurrentContactmomentInfo.vue";
import ContactmomentSwitcher from "@/features/contact/contactenManagen/ContactmomentSwitcher.vue";

import { TabList, TabListItem } from "@/components/tabs";
import { useUserStore } from "@/stores/user";
import ContactmomentCanceller from "@/features/contact/contactenManagen/ContactmomentCanceller.vue";

enum NotitieTabs {
  Regulier = "Reguliere notitie",
  Contactverzoek = "Contactverzoek maken",
}
const contactmomentStore = useContactmomentStore();
const route = useRoute();
const userStore = useUserStore();

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
  display: flex;
  flex-direction: column;
  background-color: var(--sidebar-color-1);
  border-right: 1px solid var(--sidebar-color-2);

  textarea.utrecht-textarea {
    border: 1px solid var(--sidebar-color-2);
    outline: none;
    flex: 1;
    resize: none;
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

  :deep([role="tablist"]) {
    padding: 0;
    justify-items: stretch;
    display: grid;
    grid-template-columns: 1fr 1fr;
    gap: 0;
    color: inherit;
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

    text-align: center;
    padding-block: var(--spacing-small);

    &[aria-selected="true"] {
      color: var(--color-headings);
    }

    &:not([aria-selected="true"]) {
      background-color: var(--sidebar-color-2);
    }
  }
}

menu {
  --utrecht-button-min-inline-size: 100%;

  padding: var(--spacing-default);
  display: flex;
  column-gap: var(--spacing-default);

  li {
    flex: 1;
  }
}

menu.starter {
  background-color: var(--sidebar-color-2);
  min-height: var(--header-min-height);

  .contactmoment-loopt & {
    min-height: 0;
    padding-block-end: var(--spacing-small);
  }
}

.within-moment {
  overflow: auto;
  display: flex;
  flex-direction: column;
  flex: 1;
  color: var(--color-white);

  > h2 {
    margin-block-start: var(--spacing-small);
    margin-inline: var(--spacing-default);
    color: inherit;
  }

  > .contactmoment-header {
    padding-block: var(--spacing-small);
    padding-inline: var(--spacing-default);
    background-color: var(--sidebar-color-2);

    --utrecht-heading-color: var(--color-white);

    > h3 {
      margin-block: var(--spacing-large) var(--spacing-small);
    }
  }

  :deep(.radio-group) {
    display: flex;
    justify-content: space-between;
  }
}
</style>
