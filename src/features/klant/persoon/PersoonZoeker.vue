<template>
  <section class="actions">
    <form @submit.prevent="zoekOpGeboortedatum">
      <label class="utrecht-form-label">
        Achternaam
        <input
          v-validate="store.achternaam"
          required
          class="utrecht-textbox utrecht-textbox--html-input"
        />
      </label>
      <label class="utrecht-form-label">
        Geboortedatum
        <input
          v-validate="store.geboortedatum"
          class="utrecht-textbox utrecht-textbox--html-input"
        />
      </label>
      <utrecht-button type="submit" appearance="primary-action-button">
        Zoeken
      </utrecht-button>
    </form>
    <form @submit.prevent="zoekOpPostcode">
      <label class="utrecht-form-label">
        Postcode
        <input
          v-validate="store.postcode"
          required
          class="utrecht-textbox utrecht-textbox--html-input"
        />
      </label>
      <label class="utrecht-form-label">
        Huisnummer
        <input
          v-validate="store.huisnummer"
          required
          inputmode="numeric"
          class="utrecht-textbox utrecht-textbox--html-input"
        />
      </label>
      <label class="utrecht-form-label">
        Huisletter
        <input
          v-validate="store.huisletter"
          class="utrecht-textbox utrecht-textbox--html-input"
        />
      </label>
      <label class="utrecht-form-label">
        Toevoeging
        <input
          v-validate="store.toevoeging"
          class="utrecht-textbox utrecht-textbox--html-input"
        />
      </label>
      <utrecht-button type="submit" appearance="primary-action-button">
        Zoeken
      </utrecht-button>
    </form>
    <form @submit.prevent="zoekOpBsn">
      <label class="utrecht-form-label">
        Burgerservicenummer
        <input
          v-validate="store.bsn"
          required
          class="utrecht-textbox utrecht-textbox--html-input"
        />
      </label>
      <utrecht-button type="submit" appearance="primary-action-button">
        Zoeken
      </utrecht-button>
    </form>
  </section>

  <section class="search-section" v-if="store.persoonSearchQuery?.value">
    <simple-spinner v-if="personen.loading" />
    <template v-if="personen.success">
      <personen-overzicht :records="personen.data">
        <template #caption>
          <SearchResultsCaption :results="personen.data" />
        </template>
      </personen-overzicht>
    </template>
    <application-message
      v-if="personen.error"
      messageType="error"
      :message="
        personen.error instanceof FriendlyError
          ? personen.error.message
          : 'Er is een fout opgetreden'
      "
    />
  </section>
</template>

<script lang="ts" setup>
import { watch, computed } from "vue";
import PersonenOverzicht from "./PersonenOverzicht.vue";
import ApplicationMessage from "@/components/ApplicationMessage.vue";
import SimpleSpinner from "@/components/SimpleSpinner.vue"; //todo: spinner via slot?
import Pagination from "@/nl-design-system/components/Pagination.vue"; //todo: ook via slot?
import { ensureState } from "@/stores/create-store"; //todo: niet in de stores map. die is applicatie specifiek. dit is generieke functionaliteit
import { useRouter } from "vue-router";
import SearchResultsCaption from "@/components/SearchResultsCaption.vue";
import {
  parseBsn,
  parsePostcode,
  parseDutchDate,
  validateWith,
  vValidate,
  parseAchternaam,
  parseToevoeging,
  parseHuisletter,
  parseHuisnummer,
} from "@/helpers/validation";
import {
  useSearchPersonen,
  type PersoonQuery,
  type PersoonSearchField,
} from "./service";
import { Button as UtrechtButton } from "@utrecht/component-library-vue";
import { ensureKlantForBsn } from "../service";
import { useOrganisatieIds } from "@/stores/user";
import { FriendlyError } from "@/services";

const store = ensureState({
  stateId: "klant-zoeker",
  stateFactory() {
    return {
      currentSearch: "",
      achternaam: validateWith(parseAchternaam),
      geboortedatum: validateWith(parseDutchDate),
      postcode: validateWith(parsePostcode),
      huisnummer: validateWith(parseHuisnummer),
      toevoeging: validateWith(parseToevoeging),
      huisletter: validateWith(parseHuisletter),
      bsn: validateWith(parseBsn),
      persoonSearchQuery: undefined as
        | PersoonQuery<PersoonSearchField>
        | undefined,
      page: 1,
    };
  },
});

const zoekOpGeboortedatum = () => {
  if (store.value.geboortedatum.validated && store.value.achternaam.validated) {
    store.value.persoonSearchQuery = {
      field: "geslachtsnaamGeboortedatum",
      value: {
        geslachtsnaam: store.value.achternaam.validated,
        geboortedatum: store.value.geboortedatum.validated,
      },
    };
  }
};

const zoekOpPostcode = () => {
  if (store.value.postcode.validated && store.value.huisnummer.validated) {
    store.value.persoonSearchQuery = {
      field: "postcodeHuisnummer",
      value: {
        postcode: store.value.postcode.validated,
        huisnummer: store.value.huisnummer.validated,
        toevoeging: store.value.toevoeging.validated,
        huisletter: store.value.huisletter.validated,
      },
    };
  }
};

const zoekOpBsn = () => {
  if (store.value.bsn.validated) {
    store.value.persoonSearchQuery = {
      field: "bsn",
      value: store.value.bsn.validated,
    };
  }
};

const personen = useSearchPersonen({
  query: computed(() => store.value.persoonSearchQuery),
});

const singleBsn = computed(() => {
  if (personen.success && personen.data.length === 1) {
    const first = personen.data[0];
    return first?.bsn;
  }
  return undefined;
});

const router = useRouter();
const organisatieIds = useOrganisatieIds();
watch(
  [singleBsn, () => organisatieIds.value[0]],
  async ([bsn, org], [oldBsn]) => {
    if (org && bsn && bsn !== oldBsn) {
      const { id } = await ensureKlantForBsn({ bsn }, org);
      await router.push(`/personen/${id}`);
    }
  },
);
</script>

<style lang="scss" scoped>
.klant-aanmaken {
  display: flex;

  span {
    margin-inline-start: var(--spacing-extrasmall);
  }
}

input[type="search"] {
  width: var(--section-width);
}

.search-bar {
  margin-bottom: var(--spacing-large);
  width: min(100%, 20rem);
}

.pagination {
  margin-inline: auto;
}

.search-section {
  display: grid;
  gap: var(--spacing-small);
}

input[type="radio"] {
  accent-color: var(--color-primary);
  margin: 0;
}

.radio-group {
  display: grid;
  grid-template-columns: repeat(3, auto);
  justify-content: flex-start;
  grid-gap: var(--spacing-extrasmall) var(--spacing-default);

  > legend {
    font-size: 0;
  }

  label {
    display: flex;
    gap: var(--spacing-small);
    align-items: center;
  }
}

.actions {
  display: flex;
  flex-direction: column;
  gap: var(--spacing-default);
  inline-size: min(40rem, 100%);

  form {
    display: flex;
    gap: var(--spacing-default);
    align-items: flex-end;

    > :not(button) {
      flex-basis: 33%;

      &:first-child {
        flex-grow: 1;
      }
    }
  }
}
</style>
