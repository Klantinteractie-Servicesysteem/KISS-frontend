import { ServiceResult } from "@/services";
import { searchPersonen, type PersoonQuery } from "@/services/brp";

export function useSearchPersonen(getQuery: () => PersoonQuery | undefined) {
  const getCacheKey = () => {
    const query = getQuery();
    return query ? "brp-zoek" + JSON.stringify(query) : "";
  };

  const fetcher = () => {
    const query = getQuery();
    if (!query) {
      throw new Error(
        "Dit scenario kan niet voorkomen, wordt al afgehandeld door getCacheKey",
      );
    }
    return searchPersonen(query);
  };

  return ServiceResult.fromFetcher("", fetcher, {
    getUniqueId: getCacheKey,
  });
}
