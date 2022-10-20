import type { Klant } from "@/features/shared/types";
import { combineEnrichers } from "@/services";
import { usePersoonByBsn } from "../brp/service";
import { useKlantByBsn } from "../service";
import type { Persoon } from "../types";

const isKlant = (klantOfPersoon: Klant | Persoon): klantOfPersoon is Klant =>
  klantOfPersoon._typeOfKlant === "klant";

export const useEnrichedPersoon = combineEnrichers(
  useKlantByBsn,
  usePersoonByBsn,
  (either) => either.bsn,
  isKlant
);
