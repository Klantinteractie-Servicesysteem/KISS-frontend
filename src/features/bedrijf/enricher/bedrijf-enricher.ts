import { combineEnrichers } from "@/services";
import {
  useBedrijfHandelsregisterByVestigingsnummer,
  useBedrijfKlantByVestigingsnummer,
} from "../service";
import type { BedrijfHandelsregister, BedrijfKlant } from "../types";

const isBedrijfKlant = (
  klantOfHandelsRegister: BedrijfKlant | BedrijfHandelsregister
): klantOfHandelsRegister is BedrijfKlant => {
  return klantOfHandelsRegister._bedrijfType === "klant";
};

export const useEnrichedBedrijf = combineEnrichers(
  useBedrijfKlantByVestigingsnummer,
  useBedrijfHandelsregisterByVestigingsnummer,
  (either) => either.vestigingsnummer,
  isBedrijfKlant
);
