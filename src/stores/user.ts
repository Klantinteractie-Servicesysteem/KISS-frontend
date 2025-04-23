import { defineStore } from "pinia";
import { useStorage } from "@vueuse/core";
import type { Ref } from "vue";
import { computed } from "vue";

export type User =
  | {
      isLoggedIn: false;
    }
  | {
      isLoggedIn: true;
      isRedacteur: boolean;
      isKcm: boolean;
      email: string;
      organisatieIds: string[];
    };

export const useUserStore = defineStore("user", {
  state: () => {
    const newState = {
      preferences: useStorage("preferences", {
        kanaal: "",
        skills: [],
      }) as Ref<{
        kanaal: string;
        skills: number[];
      }>,
      user: {
        isLoggedIn: false,
      } as User,
    };

    // if there is state in localstorage from before this change, it will return a boolean for skills
    if (!Array.isArray(newState.preferences.value.skills)) {
      newState.preferences.value.skills = [];
    }

    return newState;
  },
  actions: {
    setKanaal(kanaal: string) {
      this.preferences.kanaal = kanaal;
    },
    setUser(user: User) {
      this.user = user;
    },
  },
});

export const useOrganisatieIds = () => {
  const userStore = useUserStore();
  return computed(() =>
    userStore.user.isLoggedIn ? userStore.user.organisatieIds : [],
  );
};
