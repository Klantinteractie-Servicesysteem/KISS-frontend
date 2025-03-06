import { computed } from "vue";
import { throwIfNotOk, parseJson, fetchLoggedIn, useLoader } from "..";

type ValueOf<T> = T[keyof T];

export const registryVersions = {
  ok1: "OpenKlant1",
  ok2: "OpenKlant2",
} as const;

export type Systeem = {
  isDefault: boolean;
  identifier: string;
  registryVersion: ValueOf<typeof registryVersions>;
  deeplinkUrl?: string;
  deeplinkProperty?: string;
};

export const fetchSystemen = () =>
  fetchLoggedIn("/api/environment/registers")
    .then(throwIfNotOk)
    .then(parseJson)
    .then(({ systemen }) => systemen as Systeem[]);

export const useSystemen = () => {
  const { data: systemen, loading, error } = useLoader(fetchSystemen);
  const defaultSysteem = computed(() =>
    systemen.value?.find(({ isDefault }) => isDefault),
  );

  return {
    systemen,
    loading,
    error,
    defaultSysteem,
  };
};
