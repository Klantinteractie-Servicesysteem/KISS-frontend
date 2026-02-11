<template>
  <article>
    <utrecht-heading :level="headingLevel">
      {{ title }}
    </utrecht-heading>
    <div>
      <section>
        <utrecht-heading :level="headingLevel + 1"
          >Algemene contactgegevens</utrecht-heading
        >
        <dl>
          <dt>E-mailadres:</dt>
          <dd>{{ emails }}</dd>
          <dt>Telefoonnummer:</dt>
          <dd>{{ telefoonnummers }}</dd>
        </dl>
      </section>
      <section v-if="availabilities">
        <utrecht-heading :level="headingLevel + 1">Agenda</utrecht-heading>
        <table class="availability">
          <thead>
            <tr>
              <th></th>
              <th v-for="(_, day) in availabilities" :key="day">
                {{ day }}
              </th>
            </tr>
          </thead>
          <tbody>
            <tr>
              <th scope="row">ochtend</th>
              <td
                v-for="(value, day) in availabilities"
                :key="day"
                :class="[value.ochtend ? 'groen' : 'rood']"
              >
                {{ value.ochtend ? "beschikbaar" : "niet beschikbaar" }}
              </td>
            </tr>
            <tr>
              <th scope="row">middag</th>
              <td
                v-for="(value, day) in availabilities"
                :key="day"
                :class="[value.middag ? 'groen' : 'rood']"
              >
                {{ value.middag ? "beschikbaar" : "niet beschikbaar" }}
              </td>
            </tr>
          </tbody>
        </table>
      </section>
    </div>
    <section>
      <utrecht-heading :level="headingLevel + 1"
        >Detailgegevens</utrecht-heading
      >
      <img
        v-if="medewerkerRaw.optionalProfilePicture"
        :src="medewerkerRaw.optionalProfilePicture"
        width="128"
      />
      <dl>
        <dt>Functie:</dt>
        <dd>{{ medewerkerRaw?.function ?? medewerkerRaw?.functie }}</dd>
        <dt>Afdeling:</dt>
        <dd>{{ afdeling }}</dd>
        <dt>Wat kun je en wat doe je:</dt>
        <dd>{{ medewerkerRaw?.skills }}</dd>
        <template v-if="replacement">
          <dt>Vervanger:</dt>
          <dd>{{ replacement }}</dd>
        </template>
      </dl>
    </section>
  </article>

  <content-feedback
    :name="title"
    :url="medewerkerRaw.url"
    :current-section="currentFeedbackSection"
  />
</template>

<script lang="ts" setup>
import { Heading as UtrechtHeading } from "@utrecht/component-library-vue";
import { computed } from "vue";
import { ContentFeedback } from "./feedback/index";
import type { CurrentFeedbackSection } from "./feedback/types";

const props = defineProps<{
  medewerkerRaw: any;
  headingLevel: 2 | 3 | 4;
  title: string;
}>();

const currentFeedbackSection: CurrentFeedbackSection = {
  label: props.title,
  id: props.medewerkerRaw?.user,
};

const availabilities = computed(() => {
  const rawAvailabilities =
    props.medewerkerRaw?.availabilities ??
    props.medewerkerRaw?.calendar?.availabilities ??
    props.medewerkerRaw.beschikbaarheid;

  if (!rawAvailabilities) return null;

  const days = ["maandag", "dinsdag", "woensdag", "donderdag", "vrijdag"];
  return Object.fromEntries(days.map((x) => [x, rawAvailabilities[x] || {}]));
});

const telefoonnummers = computed(() =>
  (props.medewerkerRaw?.contact ?? props.medewerkerRaw)?.telefoonnummers
    ?.map(({ telefoonnummer }: any) => telefoonnummer)
    ?.filter(Boolean)
    ?.join(", "),
);

const emails = computed(
  () =>
    (props.medewerkerRaw?.contact ?? props.medewerkerRaw)?.emails
      ?.map(({ email }: any) => email)
      ?.filter(Boolean)
      ?.join(", ") || props.medewerkerRaw?.user,
);

const replacement = computed(() => {
  const obj =
    props.medewerkerRaw?.replacement ?? props.medewerkerRaw?.vervanging;
  if (typeof obj === "string") return obj;
  return obj?.name || "";
});

const afdeling = computed(() => {
  if (props.medewerkerRaw?.department) return props.medewerkerRaw.department;
  if (Array.isArray(props.medewerkerRaw?.afdelingen))
    return props.medewerkerRaw.afdelingen
      .map(({ afdelingnaam }: any) => afdelingnaam)
      .filter(Boolean)
      .join(", ");
  return "";
});
</script>

<style lang="scss" scoped>
article {
  display: grid;
  gap: var(--spacing-default);
  grid-template-columns: 1fr 1fr;
  column-gap: var(--spacing-large);

  > div {
    display: flex;
    flex-direction: column;
    gap: var(--spacing-default);
  }

  > :first-child {
    grid-column: span 2;
  }

  img {
    margin-bottom: var(--spacing-default);
  }

  section > :first-child {
    margin-block-end: var(--spacing-default);
  }
}

table {
  border-collapse: separate;
  border-spacing: var(--spacing-small);
}

table td {
  font-size: 0;
}

.groen {
  background-color: green;
}

.rood {
  background-color: red;
}

dl {
  display: grid;
  grid-template-columns: 1fr 1fr;
  row-gap: var(--spacing-small);
}

dd,
dt {
  line-height: normal;
}

h2 {
  margin-bottom: 1em;
}
</style>
