import { ServiceResult } from "@/services";
import type { Ref } from "vue";
import { fetchKlantById } from "@/services/openklant2";

export const useKlantById = (id: Ref<string>) =>
  ServiceResult.fromFetcher(() => id.value || "", fetchKlantById);
