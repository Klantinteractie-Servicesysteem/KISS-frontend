import { ServiceResult } from "@/services";
import {
  mapBedrijfsIdentifier,
  useKlantByIdentifier as useKlantByIdentifierOk1,
} from "@/services/openklant1/service";
import {
  findKlantByIdentifierOpenKlant2,
  type KlantBedrijfIdentifier,
} from "@/services/openklant2";
import {
  registryVersions,
  fetchSystemen,
} from "@/services/environment/fetch-systemen";

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

    const systemen = await fetchSystemen();
    const defaultSysteem = systemen.find(({ isDefault }) => isDefault);

    if (!defaultSysteem) {
      throw new Error("Geen default register gevonden");
    }

    if (defaultSysteem.registryVersion === registryVersions.ok2) {
      return findKlantByIdentifierOpenKlant2(defaultSysteem.identifier, id);
    } else {
      const mappedId = mapBedrijfsIdentifier(id);
      return useKlantByIdentifierOk1(defaultSysteem.identifier, () => mappedId);
    }
  };

  return ServiceResult.fromFetcher(getCacheKey(), findKlant, {
    getUniqueId: getCacheKey,
  });
};
