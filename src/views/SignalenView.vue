<template>
  <utrecht-heading :level="1" class="zaak-title"> Signalen </utrecht-heading>

  <utrecht-heading :level="2" class="zaak-title"> Aanmaken </utrecht-heading
  ><br />

  <input
    type="text"
    v-model="signalText"
    class="utrecht-textbox utrecht-textbox--html-input"
  /><br /><br />
  <button
    type="button"
    class="utrecht-button utrecht-button--primary-action"
    @click="submit"
  >
    Opslaan
  </button>
  <br /><br />
  <div v-if="data">
    {{ data.id }}
  </div>

  <br /><br /><br /><br />
  <utrecht-heading :level="2" class="zaak-title"> Opvragen </utrecht-heading
  ><br />
  <input
    type="text"
    v-model="signalId"
    class="utrecht-textbox utrecht-textbox--html-input"
  />
  <br /><br />
  <button
    type="button"
    class="utrecht-button utrecht-button--primary-action"
    @click="zoek"
  >
    Zoek</button
  ><br /><br />
  <div v-if="signaldata">
    {{ signaldata.text }}

    <pre>{{ signaldata }}</pre>
  </div>
</template>

<script setup lang="ts">
import { Heading as UtrechtHeading } from "@utrecht/component-library-vue";
import { ref } from "vue";

const data = ref(null);
const signalText = ref("");
const submit = async () => {
  const response = await fetch("/api/signalen", {
    method: "POST",
    headers: {
      "Content-Type": "application/json",
    },

    body: JSON.stringify({ text: signalText.value }),
  });

  data.value = await response.json();
};

const signaldata = ref(null);
const signalId = ref("");
const zoek = async () => {
  const response = await fetch(`/api/signalen/${signalId.value}`, {
    method: "GET",
    headers: {
      "Content-Type": "application/json",
    },
  });

  signaldata.value = await response.json();
};
</script>

<style scoped lang="scss">
section > *:not(:last-child) {
  margin-block-end: var(--spacing-large);
}
</style>
