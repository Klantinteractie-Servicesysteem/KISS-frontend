<template>
  <div class="home">
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
          <label for="searchInput"
            ><span>Zoek een werkinstructie of nieuwsbericht</span>
            <input
              type="search"
              name="search"
              id="searchInput"
              placeholder="Zoek een werkinstructie of nieuwsbericht"
              @search="handleSearch"
              v-model="state.searchField"
          /></label>
          <button title="Zoeken">
            <span>Zoeken</span>
          </button>
        </form>
      </li>
      <li>
        <form
          class="skills-form"
          method="get"
          v-if="skills.state === 'success'"
        >
          <multi-select
            name="skillIds"
            label="Filter op categorie"
            :options="skills.data.entries"
            v-model="userStore.preferences.skills"
          />
          <menu class="delete-skills-menu">
            <li
              v-for="{ id, name, className } in selectedSkills"
              :key="'skills_cb_' + id"
            >
              <button
                type="button"
                :class="`remove-filter icon-after circle-xmark ${className}`"
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
  </div>
</template>

<script setup lang="ts">
import { Heading as UtrechtHeading } from "@utrecht/component-library-vue";
import { computed } from "vue";
import { useSkills, WerkBerichten } from "@/features/werkbericht";
import MultiSelect from "@/components/MultiSelect.vue";
import { useUserStore } from "@/stores/user";
import { ensureState } from "@/stores/create-store";
import { type Berichttype, berichtTypes } from "@/features/werkbericht/types";

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

const skills = useSkills();

const selectedSkills = computed(() => {
  if (skills.state !== "success") return undefined;
  return skills.data.entries
    .map(([id, name]) => ({
      id,
      name,
      className: `category-${name.split(" ").join("-")}`,
    }))
    .filter((x) => userStore.preferences.skills.includes(x.id));
});

const selectedSkillIds = computed(() =>
  selectedSkills.value?.map(({ id }) => id)
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
  align-content: flex-start;
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
  align-items: flex-start;
  justify-content: space-between;
}

label[for="searchInput"] {
  flex: 1;
}

.search-bar {
  width: 100%;
}

.forms > :first-child {
  width: min(100%, var(--section-width));
}

.forms > :last-child {
  width: min(100%, 20rem);
  margin-inline-start: auto;
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
  gap: var(--spacing-default);
  align-items: flex-end;
  justify-items: flex-end;
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
</style>
