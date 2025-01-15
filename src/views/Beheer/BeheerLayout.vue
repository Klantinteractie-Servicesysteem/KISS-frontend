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
    <router-link to="/Beheer/Kanalen">Kanalen</router-link>
    <router-link to="/Beheer/formulieren-contactverzoek-afdeling">
      Contactverzoekformulieren afdelingen
    </router-link>
    <router-link to="/Beheer/formulieren-contactverzoek-groep">
      Contactverzoekformulieren groepen
    </router-link>

    <router-link v-if="useVacs" to="/Beheer/vacs">Vacs</router-link>
  </nav>

  <main>
    <router-view />
  </main>
</template>

<script setup lang="ts">
import { watchEffect } from "vue";
import { useRouter } from "vue-router";
import { useCurrentUser } from "@/features/login";
import { useLoader } from "@/services/use-loader";
import { fetchLoggedIn } from "@/services";

const user = useCurrentUser();
const router = useRouter();

watchEffect(() => {
  if (user.success && user.data.isLoggedIn && !user.data.isRedacteur) {
    router.push("/");
  }
});

const { data: useVacs } = useLoader(() =>
  fetchLoggedIn("/api/environment/use-vacs")
    .then((r) => r.json())
    .then(({ useVacs }) => useVacs),
);
</script>

<style lang="scss" scoped>
nav {
  display: grid;
  grid-template-columns: repeat(3, 1fr);
  gap: var(--spacing-default);
  margin-bottom: var(--spacing-large);
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
