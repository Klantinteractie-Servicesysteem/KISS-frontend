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
        <span>{{ contactverzoek.datum }}</span>
        <span>{{ contactverzoek.status }}</span>
        <span>{{ contactverzoek.behandelaar }}</span>
        <span>{{ contactverzoek.afgerond }}</span>
        <span class="chevron icon-after chevron-down"></span>
      </button>

      <div
        class="verzoek-item-content"
        :class="{ 'is-active': activeContactverzoeken[idx] }"
      >
        <dl>
          <dt>Starttijd</dt>
          <dd>{{ contactverzoek.starttijd }}</dd>
        </dl>

        <dl>
          <dt>Aanmaker</dt>
          <dd>{{ contactverzoek.aanmaker }}</dd>
        </dl>

        <dl>
          <dt>Status</dt>
          <dd>{{ contactverzoek.status }}</dd>
        </dl>

        <template v-if="contactverzoek.primaireVraagWeergave">
          <dl>
            <dt>Vraag</dt>
            <dd>{{ contactverzoek.primaireVraagWeergave }}</dd>
          </dl>
        </template>
        <template v-if="contactverzoek.afwijkendOnderwerp">
          <dl>
            <dt>Specificatie</dt>
            <dd>{{ contactverzoek.afwijkendOnderwerp }}</dd>
          </dl>
        </template>

        <dl>
          <dt>Notitie</dt>
          <dd>
            {{ contactverzoek.notitie }}
          </dd>
        </dl>
      </div>
    </div>
  </section>

  <div v-else>Geen contactverzoeken gevonden.</div>
</template>

<script lang="ts" setup>
import { ref } from "vue";
import type { ContactverzoekDetail } from "./types";

const props = defineProps<{
  contactverzoeken: ContactverzoekDetail[];
}>();

const activeContactverzoeken = ref(props.contactverzoeken.map(() => false));

const toggleItemContent = (idx: number) => {
  activeContactverzoeken.value[idx] = !activeContactverzoeken.value[idx];
};
</script>

<style lang="scss" scoped>
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
    outline: auto currentColor;
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

  dt {
    font-weight: bold;
  }

  dl {
    display: flex;
    padding: var(--spacing-default);
  }

  dl > * {
    flex: 1;
  }

  dl > dt {
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
