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
        <div class="notitie-tabs">
          <tab-list v-model="state.currentNotitieTab">
            <tab-list-item :label="NotitieTabs.Regulier">
              <template #tab="{ label }">
                <span :title="label" class="icon-after note" />
              </template>
              <utrecht-heading id="notitieblok" :level="2"
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
            </tab-list-item>
            <tab-list-item :label="NotitieTabs.Contactverzoek">
              <template #tab="{ label }">
                <span :title="label" class="icon-after phone-flip" />
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
      </section>
    </template>
  </aside>
</template>

<script lang="ts" setup>
import { ContactverzoekFormulier } from "@/features/contactverzoek";
import { Heading as UtrechtHeading } from "@utrecht/component-library-vue";
import ContactmomentVragenMenu from "@/features/contactmoment/ContactmomentVragenMenu.vue";
import { useContactmomentStore } from "@/stores/contactmoment";
import { ensureState } from "@/stores/create-store";
import { watch } from "vue";
import { useRoute } from "vue-router";
import {
  ContactmomentStarter,
  CurrentContactmomentInfo,
  ContactmomentSwitcher,
} from "@/features/contactmoment";
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
</script>

<style lang="scss" scoped>
aside {
  background-color: var(--sidebar-color-1);
  padding-inline: 2px;
  display: flex;
  flex-direction: column;

  textarea.utrecht-textarea {
    margin-block-start: var(--spacing-default);
    padding: 0;
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
    flex: 1;
    color: var(--utrecht-form-label-color);
  }

  :deep([role="tab"]) {
    margin: 0;
    padding: var(--spacing-default);
    display: flex;
    align-items: center;
    justify-content: center;

    &[aria-selected="true"] {
      color: var(--color-headings);
    }
  }
}

.starter {
  color: var(--color-white);
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

  --utrecht-button-min-inline-size: 17rem;
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

  > h2 {
    margin-block-start: var(--spacing-small);
    margin-inline: var(--spacing-default);
    color: inherit;
  }
}
</style>
