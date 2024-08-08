import { ServiceResult, parseJson, throwIfNotOk } from "@/services";
import type { ZaakDetails } from "./types";
import { computed } from "vue";
import { fetchWithZaaksysteemId } from "./service";

export function useZaaksysteemDeeplink(getZaak: () => ZaakDetails | undefined) {
  const url = "/api/zaaksysteem/deeplinkconfig";
  const getZaaksysteemId = () => getZaak()?.zaaksysteemId || "";

  const getCacheKey = () => {
    const zaaksysteemId = getZaaksysteemId();
    return zaaksysteemId && url + zaaksysteemId;
  };

  const config = ServiceResult.fromFetcher(
    url,
    (u) =>
      fetchWithZaaksysteemId(getZaaksysteemId(), u)
        .then(throwIfNotOk)
        .then(parseJson)
        .then((r) =>
          typeof r?.baseUrl === "string" &&
          typeof r?.idProperty === "string" &&
          r.baseUrl &&
          r.idProperty
            ? (r as {
                baseUrl: string;
                idProperty: string;
              })
            : null,
        ),
    {
      getUniqueId: getCacheKey,
    },
  );

  return computed(() => {
    if (!config.success || !config.data) return null;
    const property = (getZaak() as any)?.[config.data.idProperty];
    if (!property) return null;
    return config.data.baseUrl + property;
  });
}
