<template>
  <section v-if="contactverzoeken.length">
    <div class="header">
      <span>Datum</span>
      <span>Status</span>
      <span>Behandelaar</span>
      <span>Afgerond op</span>
      <span class="chevron"></span>
    </div>

    <div
      v-for="(contactverzoek, idx) in contactverzoeken"
      :key="idx"
      class="verzoek-item"
    >
      <button
        @click="toggleItemContent(idx)"
        class="verzoek-item-header"
        :class="{ 'is-active': activeContactverzoeken[idx] }"
      >
        <dutch-date
          v-if="contactverzoek.record.data.registratiedatum"
          :date="new Date(contactverzoek.record.data.registratiedatum)"
        />
        <span v-else />
        <span>{{ contactverzoek.record.data.status }}</span>
        <span>{{ contactverzoek.record.data.actor.naam }}</span>
        <span>{{ contactverzoek.record.data.datumVerwerkt }}</span>
        <span class="chevron icon-after chevron-down"></span>
      </button>

      <div
        class="verzoek-item-content"
        :class="{ 'is-active': activeContactverzoeken[idx] }"
      >
        <dl>
          <dt>Starttijd</dt>
          <dd>
            <dutch-time
              v-if="contactverzoek.record.data.registratiedatum"
              :date="new Date(contactverzoek.record.data.registratiedatum)"
            />
          </dd>

          <dt>Toelichting voor de collega</dt>
          <dd>
            {{ contactverzoek.record.data.toelichting }}
          </dd>

          <template
            v-if="
              contactverzoek.record.data.betrokkene.persoonsnaam?.achternaam
            "
          >
            <dt>Naam betrokkene</dt>
            <dd>
              {{ fullName(contactverzoek.record.data.betrokkene.persoonsnaam) }}
            </dd>
          </template>

          <template v-if="contactverzoek.record.data.betrokkene.organisatie">
            <dt>Organisatie</dt>
            <dd>{{ contactverzoek.record.data.betrokkene.organisatie }}</dd>
          </template>

          <template
            v-for="(adres, idx) in contactverzoek.record.data.betrokkene
              .digitaleAdressen"
            :key="idx"
          >
            <dt>
              {{
                capitalizeFirstLetter(
                  adres.omschrijving || adres.soortDigitaalAdres || "contact"
                )
              }}
            </dt>
            <dd>
              {{ adres.adres }}
            </dd>
          </template>

          <slot
            name="contactmoment"
            :url="contactverzoek.record.data.contactmoment"
          ></slot>
        </dl>
      </div>
    </div>
  </section>

  <div v-else>Geen contactverzoeken gevonden.</div>
</template>

<script lang="ts" setup>
import { ref } from "vue";
import type { Contactverzoek } from "./service";
import { watch } from "vue";
import DutchDate from "@/components/DutchDate.vue";
import DutchTime from "@/components/DutchTime.vue";
import { fullName } from "@/helpers/string";

const props = defineProps<{
  contactverzoeken: Contactverzoek[];
}>();

const capitalizeFirstLetter = (val: string) =>
  `${val?.[0]?.toLocaleUpperCase() || ""}${val?.substring(1) || ""}`;

const activeContactverzoeken = ref([] as boolean[]);

watch(
  () => props.contactverzoeken.length - activeContactverzoeken.value.length,
  (diff) => {
    for (let index = 0; index < diff; index++) {
      activeContactverzoeken.value.push(false);
    }
  }
);

const toggleItemContent = (idx: number) => {
  activeContactverzoeken.value[idx] = !activeContactverzoeken.value[idx];
};
</script>

<style lang="scss" scoped>
dl {
  --column-width: 25ch;
  --gap: var(--spacing-default);

  padding-inline-start: var(--spacing-default);
  display: grid;
  column-gap: var(--gap);
  row-gap: var(--spacing-default);
  grid-template-columns: var(--column-width) 1fr;
  padding-block: var(--spacing-large);
}

.header {
  display: flex;
  background-color: var(--color-tertiary);
  color: var(--color-white);
}

.header > *,
.verzoek-item-header > * {
  flex: 1;
  max-width: 250px;
  padding: var(--spacing-default);
}

.verzoek-item-header {
  all: unset;
  width: 100%;
  display: flex;
  border-bottom: 1px solid var(--color-tertiary);

  &:focus-visible {
    outline: auto currentcolor;
  }

  &.is-active {
    background-color: var(--color-secondary);

    .chevron::after {
      transform: rotate(180deg);
    }
  }

  &:hover {
    background-color: var(--color-secondary);
  }
}

.verzoek-item-content {
  background-color: var(--color-secondary);

  :deep(dt) dl > dt {
    max-width: 150px;
  }

  &:not(.is-active) {
    display: none;
  }
}

.chevron {
  display: flex;
  max-width: 50px;
  align-items: center;
  margin-inline-start: auto;

  &::after {
    transition: transform 250ms;
  }
}
</style>
