<template>
  <section v-if="isOpen">
    <utrecht-heading :level="2">Feedback</utrecht-heading>
    <feedback-form
      @cancelled="feedbackCancelled"
      @saved="feedbackSaved"
      :url="url"
      :name="name"
      :current-section="currentSection"
    ></feedback-form>
  </section>
  <menu v-else>
    <li>
      <utrecht-button
        @click="openFeedbackForm"
        appearance="primary-action-button"
      >
        Feedback
      </utrecht-button>
    </li>
  </menu>
  <application-message
    v-if="isSaved"
    message="Je feedback is verzonden"
    :autoClose="true"
  ></application-message>
</template>

<script lang="ts" setup>
import { ref } from "vue";
import {
  Heading as UtrechtHeading,
  Button as UtrechtButton,
} from "@utrecht/component-library-vue";
import FeedbackForm from "./components/FeedbackForm.vue";
import ApplicationMessage from "@/components/ApplicationMessage.vue";
import type { CurrentFeedbackSection } from "./types";

defineProps<{
  url: unknown | URL;
  name: string;
  currentSection: CurrentFeedbackSection;
}>();

const isOpen = ref(false);
const isSaved = ref(false);

const openFeedbackForm = () => {
  isSaved.value = false;
  isOpen.value = true;
};

const feedbackSaved = () => {
  isOpen.value = false;
  isSaved.value = true;
};

const feedbackCancelled = () => {
  isOpen.value = false;
};
</script>

<style lang="scss" scoped>
section {
  border: 1px solid var(--color-primary);
  border-radius: var(--radius-default);
  padding: var(--spacing-large);
}

utrecht-heading {
  margin-bottom: var(--spacing-large);
}

menu {
  list-style: none;
}
</style>
