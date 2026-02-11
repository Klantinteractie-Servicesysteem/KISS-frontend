<template>
  <div class="home">
    <seed-beheer />
    <p
      v-if="
        userStore.user.isLoggedIn &&
        !userStore.user.isKcm &&
        !userStore.user.isRedacteur &&
        !userStore.user.isKennisbank
      "
    >
      Je hebt niet de juist rechten voor het gebruik van deze applicatie. Neem
      contact op met Functioneel Beheer.
    </p>
    <template
      v-else-if="userStore.user.isLoggedIn && !userStore.user.isKennisbank"
    >
      <header>
        <utrecht-heading :level="1">Startscherm</utrecht-heading>
      </header>
      <menu class="forms">
        <li>
          <form
            ref="searchForm"
            enctype="application/x-www-form-urlencoded"
            method="get"
            @submit="handleSubmit"
            class="search-bar"
          >
            <label for="werkbericht-type-input">
              Naar welk type bericht ben je op zoek?
              <select
                name="type"
                id="werkbericht-type-input"
                v-model="state.typeField"
              >
                <option :value="undefined">Alle</option>
                <option
                  v-for="label in berichtTypes"
                  :key="`berichtTypes_${label}`"
                  :value="label"
                >
                  {{ label }}
                </option>
              </select>
            </label>
            <label for="search-input"
              ><span>Zoek een werkinstructie of nieuwsbericht</span>
              <input
                type="search"
                name="search"
                id="search-input"
                placeholder="Zoek een werkinstructie of nieuwsbericht..."
                @search="handleSearch"
                v-model="state.searchField"
            /></label>
            <button title="Zoeken">
              <span>Zoeken</span>
            </button>
          </form>
        </li>
        <li>
          <form class="skills-form" method="get" v-if="skills">
            <multi-select
              name="skillIds"
              label="Filter op categorie"
              :options="skills.entries"
              v-model="userStore.preferences.skills"
            />
            <menu class="delete-skills-menu">
              <li
                v-for="{ id, name } in selectedSkills"
                :key="'skills_cb_' + id"
              >
                <button
                  type="button"
                  class="remove-filter icon-after xmark"
                  @click="
                    userStore.preferences.skills =
                      userStore.preferences.skills.filter((x) => x !== id)
                  "
                >
                  <span>Verwijder filter op </span><span>{{ name }}</span>
                </button>
              </li>
            </menu>
          </form>
        </li>
      </menu>

      <werk-berichten
        v-if="state.currentSearch"
        :level="2"
        page-param-name="werkberichtsearchpage"
        :search="state.currentSearch"
        :skill-ids="selectedSkillIds"
        :type="state.currentType"
        :current-page="state.searchPage"
        @navigate="state.searchPage = $event"
        header="Zoekresultaten"
      />
      <template v-else>
        <werk-berichten
          :level="2"
          header="Nieuws"
          page-param-name="nieuwspage"
          :type="'Nieuws'"
          :skill-ids="selectedSkillIds"
          :current-page="state.nieuwsPage"
          @navigate="state.nieuwsPage = $event"
        />
        <werk-berichten
          :level="2"
          header="Werkinstructies"
          page-param-name="werkinstructiepage"
          :type="'Werkinstructie'"
          :skill-ids="selectedSkillIds"
          :current-page="state.werkinstructiesPage"
          @navigate="state.werkinstructiesPage = $event"
        />
      </template>
    </template>
  </div>
</template>

<script setup lang="ts">
import { Heading as UtrechtHeading } from "@utrecht/component-library-vue";
import { computed } from "vue";
import { fetchSkills, WerkBerichten } from "@/features/informatie/werkbericht";
import MultiSelect from "@/components/MultiSelect.vue";
import { useUserStore } from "@/stores/user";
import { ensureState } from "@/stores/create-store";
import {
  type Berichttype,
  berichtTypes,
} from "@/features/informatie/werkbericht/types";
import SeedBeheer from "@/features/beheer/SeedBeheer.vue";
import { useLoader } from "@/services";

const state = ensureState({
  stateId: "HomeView",
  stateFactory() {
    return {
      searchField: "",
      typeField: undefined as Berichttype | undefined,
      currentSearch: "",
      currentType: undefined as Berichttype | undefined,
      searchPage: 1,
      nieuwsPage: 1,
      werkinstructiesPage: 1,
    };
  },
});

const userStore = useUserStore();

const { data: skills } = useLoader(() => {
  if (userStore.user.isKcm || userStore.user.isRedacteur) {
    return fetchSkills();
  }
});

const selectedSkills = computed(() => {
  return skills.value?.entries
    .map(([id, name]) => ({
      id,
      name,
    }))
    .filter((x) => userStore.preferences.skills.includes(x.id));
});

const selectedSkillIds = computed(() =>
  selectedSkills.value?.map(({ id }) => id),
);

function handleSubmit(e: Event) {
  const { currentTarget } = e;
  if (!(currentTarget instanceof HTMLFormElement)) return;
  e.preventDefault();
  const formData = new FormData(currentTarget);
  const obj = Object.fromEntries(formData);
  state.value.currentSearch = obj?.search?.toString() || "";
  state.value.currentType = berichtTypes.includes(obj.type as Berichttype)
    ? (obj?.type as Berichttype)
    : undefined;
}

function handleSearch(e: Event) {
  const { currentTarget } = e;
  if (!(currentTarget instanceof HTMLInputElement)) return;
  e.preventDefault();
  state.value.currentSearch = currentTarget.value;
}
</script>

<style scoped lang="scss">
.home {
  gap: var(--spacing-default);
  display: flex;
  flex-flow: row wrap;
  justify-content: space-between;
  position: relative;

  > * {
    flex-basis: 100%;
  }
}

.home > section {
  &:not(:only-of-type) {
    max-width: var(--section-width);
  }

  > utrecht-heading:first-child {
    padding-left: var(--text-margin);
    padding-bottom: var(--spacing-small);
    border-bottom: 1px solid var(--color-tertiary);
  }
}

header {
  flex-flow: row wrap;
  display: flex;
  justify-content: space-between;
  align-items: center;
}

#werkbericht-type-input {
  background-color: var(--color-secondary);
  border-inline-end: var(--border-style);
}

.admin-link {
  padding: var(--spacing-small);
  color: var(--color-headings);
}

.forms {
  display: flex;
  flex-wrap: wrap;
  gap: var(--spacing-default);
  justify-content: space-between;
}

.forms > :first-child {
  // so placeholder fits
  flex-basis: 32.5rem;
}

.forms > :last-child {
  flex-basis: 20rem;
}

form {
  display: flex;
}

.remove-filter {
  display: flex;
  gap: var(--spacing-small);
  border: none;

  &:focus-visible {
    outline: var(--utrecht-focus-outline-color, transparent)
      var(--utrecht-focus-outline-style, solid)
      var(--utrecht-focus-outline-width, 0);
    outline-offset: var(--utrecht-focus-outline-offset, 0);
  }

  > :first-child {
    position: absolute;
    left: -999px;
  }
}

.skills-form {
  display: grid;
  gap: var(--spacing-small);
  width: 100%;
}

menu {
  list-style: none;
}

.delete-skills-menu {
  display: flex;
  flex-flow: row wrap;
  gap: var(--spacing-small);
}

.remove-filter::after {
  transition: opacity 200ms;
}

.remove-filter:not(:hover, :focus, :focus-visible, :active)::after {
  opacity: 0;
}
</style>
