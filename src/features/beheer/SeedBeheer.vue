<template>
  <div v-if="seedLoading">
    <simple-spinner />
  </div>

  <application-message v-else-if="canSeed" messageType="warning">
    <p>
      Wilt u KISS vullen met voorbeelddata voor Gespreksresultaten, Skills,
      Nieuws en Werkinstructies en Links?
    </p>

    <button type="button" class="utrecht-button start-button" @click="seedData">
      Voorbeelddata aanmaken
    </button>
  </application-message>
</template>

<script setup lang="ts">
import { computed, ref } from "vue";
import { toast } from "@/stores/toast";
import SimpleSpinner from "@/components/SimpleSpinner.vue";
import ApplicationMessage from "@/components/ApplicationMessage.vue";

import { useLoader } from "@/services/use-loader";
import { fetchLoggedIn } from "@/services";
import { useUserStore } from "@/stores/user";
import { storeToRefs } from "pinia";

const seedLoading = ref(false);

const userStore = useUserStore();
const { user } = storeToRefs(userStore);

const isRedacteur = computed(
  () => user.value.isLoggedIn && user.value.isRedacteur,
);

const seedData = async () => {
  seedLoading.value = true;

  const { ok } = await fetchLoggedIn("/api/seed/start", { method: "POST" });

  if (ok) {
    window.location.reload();
  } else {
    seedLoading.value = false;
    toast({
      text: "Er is een fout opgetreden bij het vullen van KISS met voorbeelddata.",
      type: "error",
    });
  }
};

const { data: canSeed } = useLoader(() => {
  if (isRedacteur.value) {
    return fetchLoggedIn("/api/seed/check").then(({ ok }) => ok);
  }
});
</script>

<style lang="scss" scoped>
article {
  display: flex;
  column-gap: var(--spacing-default);
  align-items: flex-start;
  justify-content: space-between;
}

p {
  margin-block-end: var(--spacing-default);
  flex: 1;
}

.start-button {
  --utrecht-button-background-color: var(--color-white);
  --utrecht-button-color: var(--color-accent-text);
}
</style>
