import { ServiceResult, type Paginated } from "@/services";
import {
  searchBedrijvenInHandelsRegister,
  type Bedrijf,
  type BedrijfSearchOptions,
} from "@/services/kvk";

export type BedrijvenQuery =
  | BedrijfSearchOptions
  | { email: string }
  | { telefoonnummer: string };

export function useSearchBedrijven(
  getArgs: () =>
    | {
        query: BedrijvenQuery;
        page: number;
      }
    | undefined,
) {
  const getCacheKey = () => {
    const args = getArgs();
    return args ? "searchBedrijven" + JSON.stringify(args) : "";
  };

  const fetcher = () => {
    const args = getArgs();
    if (!args) {
      throw new Error("query wordt hierboven al gecheckt");
    }

    const { page, query } = args;

    if ("email" in query || "telefoonnummer" in query) {
      throw new Error("Invalid query for searchBedrijvenInHandelsRegister");
    }

    // Voer een zoekopdracht uit voor bedrijven via het Handelsregister
    return searchBedrijvenInHandelsRegister(query, page);
  };

  return ServiceResult.fromFetcher<Paginated<Bedrijf>>(getCacheKey, fetcher);
}
