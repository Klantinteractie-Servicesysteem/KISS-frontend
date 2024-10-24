import { ServiceResult } from "@/services";
import type { Ref } from "vue";
import { fetchKlantByIdOk2 } from "@/services/openklant2";
import { fetchKlantByIdOk1 } from "@/services/openklant1";

export const useKlantById = (
  id: Ref<string>,
  gebruikKlantInteractiesApi: Ref<boolean | null>,
) => {
  const getApiSpecifickUrl = () => {
    if (gebruikKlantInteractiesApi.value === null) {
      return "";
    }

    return id.value;
  };

  const fetchKlant = (
    url: string,
    gebruikKlantinteractiesApi: Ref<boolean | null>,
  ) => {
    if (gebruikKlantinteractiesApi.value) {
      return fetchKlantByIdOk2(url);
    } else {
      return fetchKlantByIdOk1(url);
    }
  };

  return ServiceResult.fromFetcher(getApiSpecifickUrl, (u: string) =>
    fetchKlant(u, gebruikKlantInteractiesApi),
  );
};
