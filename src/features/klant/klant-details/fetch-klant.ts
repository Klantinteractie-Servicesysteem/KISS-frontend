import {
  fetchKlantByIdOk2,
  fetchKlantByKlantIdentificatorOk2,
} from "@/services/openklant2";
import {
  fetchKlantByIdOk1,
  fetchKlantByKlantIdentificatorOk1,
} from "@/services/openklant1";
import {
  registryVersions,
  type Systeem,
} from "@/services/environment/fetch-systemen";
import type { Klant } from "@/services/openklant/types";
import { mapKlantToKlantIdentifier } from "@/features/contact/shared";
import { findBedrijfInHandelsRegister, type Bedrijf } from "@/services/kvk";
import { enforceOneOrZero } from "@/services";

export const fetchKlant = async ({
  id,
  systemen,
  defaultSysteem,
}: {
  id: string;
  systemen: Systeem[];
  defaultSysteem: Systeem;
}): Promise<Klant | null> => {
  const klant = await fetchKlantById(id, defaultSysteem);
  if (!klant) return null;
  if (heeftContactgegevens(klant)) return klant;
  if (!systemen.length) return klant;

  // For non-natural persons, we have EITHER an RSIN OR a Chamber of Commerce number (kvknummer),
  // depending on whether the default system is ok1 or ok2.
  // To translate this to the other systems,
  // we need BOTH. So we first need to fetch the company again.
  const bedrijf = await findBedrijfInHandelsRegister({
    kvkNummer: klant.kvkNummer || klant.nietNatuurlijkPersoonIdentifier || "",
    vestigingsnummer: klant.vestigingsnummer || "",
    rsin: klant.rsin || klant.nietNatuurlijkPersoonIdentifier,
  }).then(enforceOneOrZero);

  if (!bedrijf) return klant;

  for (const nonDefaultSysteem of systemen.filter(
    (s) => s.identifier !== defaultSysteem.identifier,
  )) {
    const fallbackKlant = await fetchKlantByNonDefaultSysteem(
      bedrijf,
      nonDefaultSysteem,
    );

    //we nemen alleen de contactgegevens over als die niet in de default klant zitten, maar wel in een ander system zijn gevonden
    //alleen de contactgegevens, geen andere gegevens overnemen, de klant uit het default systeem is leidend!
    if (fallbackKlant && heeftContactgegevens(fallbackKlant)) {
      klant.telefoonnummer = fallbackKlant.telefoonnummer;
      klant.telefoonnummers = fallbackKlant.telefoonnummers;
      klant.emailadres = fallbackKlant.emailadres;
      klant.emailadressen = fallbackKlant.emailadressen;
      return klant;
    }
  }

  return klant;
};

const fetchKlantByNonDefaultSysteem = async (
  bedrijf: Bedrijf,
  systeem: Systeem,
): Promise<Klant | null> =>
  systeem.registryVersion === registryVersions.ok1
    ? await fetchKlantByKlantIdentificatorOk1(systeem.identifier, bedrijf)
    : await fetchKlantByKlantIdentificatorOk2(systeem.identifier, bedrijf);

const fetchKlantById = async (
  id: string,
  systeem: Systeem,
): Promise<Klant | null> => {
  try {
    return await (
      systeem.registryVersion === registryVersions.ok1
        ? fetchKlantByIdOk1
        : fetchKlantByIdOk2
    )(systeem.identifier, id);
  } catch {
    return null;
  }
};

const heeftContactgegevens = (klant: Klant) =>
  klant.emailadressen?.length || klant.telefoonnummers?.length;
