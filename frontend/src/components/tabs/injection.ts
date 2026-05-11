import type { InjectionKey, Ref } from "vue";
export const tablistInjectionKey = Symbol() as InjectionKey<{
  el: Ref<HTMLElement | undefined>;
  setActive: (name: string) => void;
  isActive: (name: string) => boolean;
  register: (name: string) => void;
  unregister: (name: string) => void;
}>;
