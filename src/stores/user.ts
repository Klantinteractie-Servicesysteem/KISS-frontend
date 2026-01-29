import { defineStore } from "pinia";
import { useStorage } from "@vueuse/core";
import type { Ref } from "vue";
import { computed } from "vue";

export type Permission =
  | "linksbeheer"
  | "kanalenbeheer"
  | "kanalenread"
  | "skillsbeheer"
  | "skillsread"
  | "gespreksresultatenbeheer"
  | "contactformulierenafdelingenbeheer"
  | "contactformulierengroepenbeheer"
  | "contactformulierenread"
  | "berichtenread"
  | "berichtenbeheer"
  | "vacsbeheer";

export const BEHEER_PERMISSIONS: Permission[] = [
  "linksbeheer",
  "kanalenbeheer",
  "kanalenread",
  "skillsbeheer",
  "gespreksresultatenbeheer",
  "contactformulierenafdelingenbeheer",
  "contactformulierengroepenbeheer",
  "berichtenbeheer",
  "vacsbeheer",
];

export type User = {
  isLoggedIn: boolean;
  isRedacteur: boolean;
  isKcm: boolean;
  isKennisbank: boolean;
  email: string;
  organisatieIds: string[];
  isSessionExpired: boolean;
  permissions: Permission[];
};

export const useUserStore = defineStore("user", {
  state: () => {
    const newState = {
      promise: Promise.resolve(),
      preferences: useStorage("preferences", {
        kanaal: "",
        skills: [],
      }) as Ref<{
        kanaal: string;
        skills: number[];
      }>,
      user: {
        isLoggedIn: false,
        isSessionExpired: false,
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
    setSessionExpired() {
      this.user.isSessionExpired = this.user.isLoggedIn ? true : false; //can only be false After the user has been logged in
      this.user.isLoggedIn = false;
    },
    setUser(user: User) {
      this.user = user;
    },
    setPromise(promise: Promise<User>) {
      this.promise = promise.then();
    },
  },
  getters: {
    hasPermission:
      (state) => (requiredPermissions: Permission | Permission[]) => {
        const permissions = Array.isArray(requiredPermissions)
          ? requiredPermissions
          : [requiredPermissions];
        return permissions.some((p) => state.user.permissions.includes(p));
      },
  },
});

export const useOrganisatieIds = () => {
  const userStore = useUserStore();
  return computed(() =>
    userStore.user.isLoggedIn ? userStore.user.organisatieIds : [],
  );
};
