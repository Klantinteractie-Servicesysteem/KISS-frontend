import { ServiceResult } from "@/services";
import {
  mapBedrijfsIdentifier,
  useKlantByIdentifier,
} from "@/services/openklant1/service";
import {
  findKlantByIdentifier,
  type KlantBedrijfIdentifier,
  useOpenKlant2,
} from "@/services/openklant2";

export const useKlantByBedrijfIdentifier = (
  getId: () => KlantBedrijfIdentifier | undefined,
) => {
  const getCacheKey = () => {
    const id = getId();
    if (!id) return "";
    return "klant" + JSON.stringify(id);
  };

  const findKlant = async () => {
    const id = getId();
    if (!id) {
      throw new Error("Geen valide KlantBedrijfIdentifier");
    }

    const isOpenKlant2 = await useOpenKlant2();

    if (isOpenKlant2) {
      return findKlantByIdentifier(id);
    } else {
      const mappedId = mapBedrijfsIdentifier(id);
      return useKlantByIdentifier(() => mappedId);
    }
  };

  return ServiceResult.fromFetcher(getCacheKey(), findKlant, {
    getUniqueId: getCacheKey,
  });
};
