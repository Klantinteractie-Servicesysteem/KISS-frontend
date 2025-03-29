import { ServiceResult, enforceOneOrZero } from "@/services";
import {
  findBedrijfInHandelsRegister,
  type BedrijfIdentifier,
} from "@/services/kvk";

export const useBedrijfByIdentifier = (
  getId: () => BedrijfIdentifier | undefined,
) => {
  const getCacheKey = () => {
    const id = getId();

    if (!id) return "";

    //door de constructie zoder url is een cachekey nodig, maar die willen we eigenlijk niet gebruiken, ivm onvoorspelbaar gedrag.
    //voor nu opgelost door een unieke key te genereren.,
    return "" + Date.now();
  };

  const fetcher = () => {
    const id = getId();
    if (!id) {
      throw new Error(
        "Dit scenario kan niet voorkomen, wordt al afgehandeld door getCacheKey",
      );
    }

    return findBedrijfInHandelsRegister(id).then(enforceOneOrZero);
  };

  return ServiceResult.fromFetcher("", fetcher, {
    getUniqueId: getCacheKey,
  });
};
