<template>
  <section class="container-medium">
    <p>Zoek op één van de onderstaande combinaties.</p>
    <form @submit.prevent="zoekOpGeboortedatum" class="zoekerForm">
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
    <form @submit.prevent="zoekOpPostcode" class="zoekerForm-postcode">
      <div class="form-left">
        <div class="row-top">
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
          <label class="utrecht-form-label">
            Achternaam
            <input
              v-validate="store.achternaamPostcode"
              class="utrecht-textbox utrecht-textbox--html-input"
            />
          </label>
        </div>
      </div>

      <div class="form-right">
        <div class="row-bottom">
          <utrecht-button type="submit" appearance="primary-action-button">
            Zoeken
          </utrecht-button>
        </div>
      </div>
    </form>

    <form @submit.prevent="zoekOpBsn" class="zoekerForm">
      <label class="utrecht-form-label">
        Bsn
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

  <section class="search-section" v-if="store.persoonSearchQuery">
    <simple-spinner v-if="personen.loading" />
    <template v-if="personen.success">
      <personen-overzicht
        :records="personen.data"
        :navigate-on-single-result="navigateOnSingleResult"
      >
        <template #caption>
          <SearchResultsCaption
            :results="personen.data"
            :zoekTermen="store.persoonSearchQuery"
          />
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
import { ref } from "vue";
import ApplicationMessage from "@/components/ApplicationMessage.vue";
import SimpleSpinner from "@/components/SimpleSpinner.vue"; //todo: spinner via slot?
import { ensureState } from "@/stores/create-store"; //todo: niet in de stores map. die is applicatie specifiek. dit is generieke functionaliteit
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
import { Button as UtrechtButton } from "@utrecht/component-library-vue";
import { FriendlyError } from "@/services";

import type { PersoonQuery } from "@/services/brp";
import { useSearchPersonen } from "./use-search-personen";
import PersonenOverzicht from "./PersonenOverzicht.vue";

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
      achternaamPostcode: validateWith(parseAchternaam),
      bsn: validateWith(parseBsn),
      persoonSearchQuery: undefined as PersoonQuery | undefined,
      page: 1,
    };
  },
});

const zoekOpGeboortedatum = () => {
  if (store.value.geboortedatum.validated && store.value.achternaam.validated) {
    store.value.persoonSearchQuery = {
      geslachtsnaamGeboortedatum: {
        geslachtsnaam: store.value.achternaam.validated,
        geboortedatum: store.value.geboortedatum.validated,
      },
    };
    navigateOnSingleResult.value = true;
  }
};

const zoekOpPostcode = () => {
  if (store.value.postcode.validated && store.value.huisnummer.validated) {
    store.value.persoonSearchQuery = {
      postcodeHuisnummerAchternaam: {
        postcode: store.value.postcode.validated,
        huisnummer: store.value.huisnummer.validated,
        toevoeging: store.value.toevoeging.validated,
        huisletter: store.value.huisletter.validated,
        achternaam: store.value.achternaamPostcode.validated,
      },
    };
    navigateOnSingleResult.value = true;
  }
};

const zoekOpBsn = () => {
  if (store.value.bsn.validated) {
    store.value.persoonSearchQuery = {
      bsn: store.value.bsn.validated,
    };
    navigateOnSingleResult.value = true;
  }
};

const personen = useSearchPersonen(() => store.value.persoonSearchQuery);

const navigateOnSingleResult = ref(false);
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

.zoekerForm-postcode {
  display: flex;
  gap: var(--spacing-default);
  background-color: var(--color-secondary);
  padding: var(--spacing-default);

  .form-left {
    .row-top {
      display: flex;
      justify-content: flex-start;
      flex-wrap: wrap;
      gap: var(--spacing-default);

      label {
        flex: 1;
      }
    }
  }

  .form-right {
    display: flex;
    flex-direction: column;
    justify-content: flex-end;
    gap: var(--spacing-small);
  }
}
</style>
