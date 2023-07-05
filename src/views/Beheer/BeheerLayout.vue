<template>
  <nav>
    <router-link to="/">Home</router-link>
    <router-link to="/Beheer/NieuwsEnWerkinstructies">
      Nieuws en werkinstructies
    </router-link>
    <router-link to="/Beheer/Skills">Skills</router-link>
    <router-link to="/Beheer/Links">Links</router-link>
    <router-link to="/Beheer/gespreksresultaten"
      >Gespreksresultaten</router-link
    >
  </nav>
  <main>
    <router-view />
  </main>
</template>

<script setup lang="ts">
import { useCurrentUser } from "@/features/login";
import { watchEffect } from "vue";
import { useRouter } from "vue-router";

const user = useCurrentUser();
const router = useRouter();

watchEffect(() => {
  if (user.success && user.data.isLoggedIn && !user.data.isRedacteur) {
    router.push("/");
  }
});
</script>

<style lang="scss" scoped>
nav {
  display: flex;
  gap: var(--spacing-default);
}

main {
  max-width: var(--section-width-large);
}

:deep(.listItem) {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding-block: var(--spacing-small);
  border-bottom: 2px solid var(--color-tertiary);
}

:deep(menu) {
  margin-block-start: var(--spacing-default);
}

:deep(.header-wrapper) {
  display: flex;
  align-items: center;
  justify-content: space-between;
}

:deep(a.utrecht-button) {
  text-decoration: none;
}

:deep(form) {
  max-inline-size: 40rem;
}

:deep(fieldset) {
  display: flex;
  flex-direction: column;

  > label {
    display: flex;
    gap: var(--spacing-small);
    align-items: baseline;

    > input {
      margin: 0;
      padding: 0;
      scale: 1.25;
      accent-color: var(--color-primary);
    }
  }
}
</style>
