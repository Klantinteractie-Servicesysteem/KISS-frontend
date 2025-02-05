import { ServiceResult } from "@/services";
import {
  mapBedrijfsIdentifier,
  useKlantByIdentifier as useKlantByIdentifierOk1,
} from "@/services/openklant1/service";
import {
  findKlantByIdentifier,
  type KlantBedrijfIdentifier,
} from "@/services/openklant2";
import { getRegisterDetails as getSysteemDetails } from "@/features/shared/systeemdetails";

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

    const { useKlantInteractiesApi, systeemId } = await getSysteemDetails();

    if (useKlantInteractiesApi) {
      return findKlantByIdentifier(systeemId, id);
    } else {
      const mappedId = mapBedrijfsIdentifier(id);
      return useKlantByIdentifierOk1(systeemId, () => mappedId);
    }
  };

  return ServiceResult.fromFetcher(getCacheKey(), findKlant, {
    getUniqueId: getCacheKey,
  });
};
