import {
  fetchKlantByIdOk2,
  fetchKlantByKlantIdentificatorOk2,
  type KlantBedrijfIdentifier,
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
import { useContactmomentStore } from "@/stores/contactmoment";
import {
  searchBedrijvenInHandelsRegisterByRsin,
  searchBedrijvenInHandelsRegisterByVestiging,
  type Bedrijf,
} from "@/services/kvk";
import { enforceOneOrZero } from "@/services";

// export const fetchKlantFromExternalRegistryByExternalId = async ({
//   externalId,
//   systemen,
//   defaultSysteem,
// }: {
//   externalId: KlantBedrijfIdentifier;
//   systemen: Systeem[];
//   defaultSysteem: Systeem;
// }): Promise<Klant | null> => {
//   //fetch klant form external registrie based on the externalId for the store
//   const klant = await fetchKlantById(externalId, defaultSysteem);
//   if (!klant) return null;
//   if (heeftContactgegevens(klant)) return klant;
//   if (!systemen.length) return klant;

//   if (!klant.bsn) {
//     // For non-natural persons, we have EITHER an RSIN OR a Chamber of Commerce number (kvknummer),
//     // depending on whether the default system is ok1 or ok2.
//     // To translate this to the other systems,
//     // we need BOTH. So we first need to fetch the company again.

//     const bedrijf = await searchBedrijvenInHandelsRegisterByRsin(
//       klant.rsin ||
//         klant.nietNatuurlijkPersoonIdentifier ||
//         klant.kvkNummer ||
//         "",
//     ).then(enforceOneOrZero);

//     if (!bedrijf) return klant;

//     klant.kvkNummer = bedrijf.kvkNummer;
//     klant.rsin = bedrijf.rsin;
//   }

//   for (const nonDefaultSysteem of systemen.filter(
//     (s) => s.identifier !== defaultSysteem.identifier,
//   )) {
//     const fallbackKlant = await fetchKlantByNonDefaultSysteem(
//       klant,
//       nonDefaultSysteem,
//     );

//     //we nemen alleen de contactgegevens over als die niet in de default klant zitten, maar wel in een ander system zijn gevonden
//     //alleen de contactgegevens, geen andere gegevens overnemen, de klant uit het default systeem is leidend!
//     if (fallbackKlant && heeftContactgegevens(fallbackKlant)) {
//       klant.telefoonnummer = fallbackKlant.telefoonnummer;
//       klant.telefoonnummers = fallbackKlant.telefoonnummers;
//       klant.emailadres = fallbackKlant.emailadres;
//       klant.emailadressen = fallbackKlant.emailadressen;
//       return klant;
//     }
//   }

//   return klant;
// };

export const fetchKlantByInternalId = async ({
  internalId,
  systemen,
  defaultSysteem,
}: {
  internalId: string;
  systemen: Systeem[];
  defaultSysteem: Systeem;
}): Promise<Klant | null> => {
  const store = useContactmomentStore();

  //fetch klant from store based on the internal in memory KISS id
  const klantenInHuidigeVraag =
    store.$state.huidigContactmoment?.huidigeVraag.klanten;
  const knownKlant = klantenInHuidigeVraag?.find(
    (x) => x.klant.internalId == internalId,
  );
  const openKlantId = knownKlant?.klant.id;

  if (!openKlantId) {
    return null;
  }

  //fetch klant form external registrie based on the externalId for the store
  const klant = await fetchKlantById(openKlantId, defaultSysteem);
  if (!klant) return null;
  if (heeftContactgegevens(klant)) return klant;
  if (!systemen.length) return klant;

  if (!klant.bsn) {
    // For non-natural persons, we have EITHER an RSIN OR a Chamber of Commerce number (kvknummer),
    // depending on whether the default system is ok1 or ok2.
    // To translate this to the other systems,
    // we need BOTH. So we first need to fetch the company again.

    const bedrijf = await searchBedrijvenInHandelsRegisterByRsin(
      klant.rsin ||
        klant.nietNatuurlijkPersoonIdentifier ||
        klant.kvkNummer ||
        "",
    ).then(enforceOneOrZero);

    if (!bedrijf) return klant;

    klant.kvkNummer = bedrijf.kvkNummer;
    klant.rsin = bedrijf.rsin;
  }

  for (const nonDefaultSysteem of systemen.filter(
    (s) => s.identifier !== defaultSysteem.identifier,
  )) {
    const fallbackKlant = await fetchKlantByNonDefaultSysteem(
      klant,
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
  bedrijf: Klant,
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
function searchBedrijfInHandelsRegister(arg0: {
  kvkNummer: string;
  vestigingsnummer: string;
  rsin: string | undefined;
}) {
  throw new Error("Function not implemented.");
}
