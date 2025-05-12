<template>
  <section class="container-medium">
    <p>Zoek op één van de onderstaande combinaties.</p>
    <form @submit.prevent="zoekOpBedrijfsnaam" class="zoekerForm">
      <label class="utrecht-form-label">
        Bedrijfsnaam
        <input
          v-validate="store.bedrijfsnaam"
          required
          class="utrecht-textbox utrecht-textbox--html-input"
        />
      </label>
      <utrecht-button type="submit" appearance="primary-action-button">
        Zoeken
      </utrecht-button>
    </form>

    <form @submit.prevent="zoekOpKvkOfVestigingsnummer" class="zoekerForm">
      <label class="utrecht-form-label">
        KVK-nummer of vestigingsnummer
        <input
          v-validate="store.kvkOfVestigingsnummer"
          required
          class="utrecht-textbox utrecht-textbox--html-input"
        />
      </label>

      <utrecht-button type="submit" appearance="primary-action-button">
        Zoeken
      </utrecht-button>
    </form>

    <form @submit.prevent="zoekOpPostcodeHuisnummer" class="zoekerForm">
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
          class="utrecht-textbox utrecht-textbox--html-input"
        />
      </label>
      <utrecht-button type="submit" appearance="primary-action-button">
        Zoeken
      </utrecht-button>
    </form>
  </section>

  <section class="search-section" v-if="store.query">
    <simple-spinner v-if="bedrijven.loading" />
    <template v-if="bedrijven.success">
      <bedrijven-overzicht
        :records="bedrijven.data.page"
        :navigate-on-single-result="navigateOnSingleResult"
      >
        <template #caption v-if="'pageNumber' in bedrijven.data">
          <SearchResultsCaption
            :results="bedrijven.data"
            :zoekTermen="store.query"
          />
        </template>
      </bedrijven-overzicht>
      <pagination
        v-if="'pageNumber' in bedrijven.data"
        class="pagination"
        :pagination="bedrijven.data"
        @navigate="navigate"
      />
    </template>
    <application-message
      v-if="bedrijven.error"
      messageType="error"
      :message="
        bedrijven.error instanceof FriendlyError
          ? bedrijven.error.message
          : 'Er is een fout opgetreden'
      "
    />
  </section>
</template>

<script setup lang="ts">
import {
  vValidate,
  parseBedrijfsnaam,
  parseKkvkOfVestigingsnummer,
  validateWith,
  parsePostcode,
  parseHuisnummer,
} from "@/helpers/validation";
import { ensureState } from "@/stores/create-store";
import { ref } from "vue";
import {
  useSearchBedrijven,
  type BedrijvenQuery,
} from "./use-search-bedrijven";

import SimpleSpinner from "@/components/SimpleSpinner.vue";
import Pagination from "@/nl-design-system/components/Pagination.vue";
import ApplicationMessage from "@/components/ApplicationMessage.vue";
import BedrijvenOverzicht from "./BedrijvenOverzicht.vue";
import SearchResultsCaption from "@/components/SearchResultsCaption.vue";
import { FriendlyError } from "@/services";
import { Button as UtrechtButton } from "@utrecht/component-library-vue";

const store = ensureState({
  stateId: "BedrijfZoeker",
  stateFactory() {
    return {
      currentSearch: "",
      bedrijfsnaam: validateWith(parseBedrijfsnaam),
      kvkOfVestigingsnummer: validateWith(parseKkvkOfVestigingsnummer),
      postcode: validateWith(parsePostcode),
      huisnummer: validateWith(parseHuisnummer),
      query: undefined as BedrijvenQuery | undefined,
      page: 1,
    };
  },
});

const navigateOnSingleResult = ref(false);

const zoekOpKvkOfVestigingsnummer = () => {
  if (store.value.kvkOfVestigingsnummer.validated) {
    if (store.value.kvkOfVestigingsnummer.validated.length === 8) {
      store.value.query = {
        kvkNummer: store.value.kvkOfVestigingsnummer.validated,
      };
    }
    if (store.value.kvkOfVestigingsnummer.validated.length === 12) {
      store.value.query = {
        vestigingsnummer: store.value.kvkOfVestigingsnummer.validated,
      };
    }
    store.value.page = 1;
    navigateOnSingleResult.value = true;
  }
};

const zoekOpBedrijfsnaam = () => {
  if (store.value.bedrijfsnaam.validated) {
    store.value.query = {
      handelsnaam: store.value.bedrijfsnaam.validated,
    };
    store.value.page = 1;
    navigateOnSingleResult.value = true;
  }
};

const zoekOpPostcodeHuisnummer = () => {
  if (store.value.postcode.validated && store.value.huisnummer.validated) {
    store.value.query = {
      postcodeHuisnummer: {
        postcode: store.value.postcode.validated,
        huisnummer: store.value.huisnummer.validated,
      },
    };
    store.value.page = 1;
    navigateOnSingleResult.value = true;
  }
};

const bedrijven = useSearchBedrijven(
  () =>
    store.value.query && {
      query: store.value.query,
      page: store.value.page,
    },
);

const navigate = (val: number) => {
  store.value.page = val;
};
</script>

<style scoped lang="scss">
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
</style>
