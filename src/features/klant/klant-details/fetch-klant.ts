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

import { searchBedrijvenInHandelsRegisterByRsin } from "@/services/kvk";
import { enforceOneOrZero } from "@/services";

import type { KlantIdentificator } from "@/features/contact/types";
import { mapKlantToKlantIdentifier } from "@/features/contact/shared";
import type { ContactmomentKlant } from "@/stores/contactmoment";

export const fetchKlantByInternalId = async ({
  internalKlant,
  systemen,
  defaultSysteem,
}: {
  internalKlant: ContactmomentKlant;
  systemen: Systeem[];
  defaultSysteem: Systeem;
}): Promise<Klant | null> => {
  //fetch klant from store based on the internal in memory KISS id

  if (internalKlant.id) {
    //fetch klant form default klantregister based on the externalId for the store
    const klant = await fetchKlantById(internalKlant.id, defaultSysteem);
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

    await enrichKlantWithContactDetails(klant, systemen, defaultSysteem);

    return klant;
  }

  //Klant is not available in the default klantregister

  ///////////////////////
  //todo: duplicate from bedrijvenoverzichtrow. clean up
  //If there is no Klant yet in the default Klant registry, or it there is but it doesn't have any contactgegevens...
  //look in any other Klant registry to find any contactgevens for this Bedrijf from the KvK

  const kvkNummer = internalKlant?.kvkNummer;
  const vestigingsnummer = internalKlant?.vestigingsnummer;
  const rsin = internalKlant?.rsin;
  const bsn = internalKlant?.bsn;
  const id = internalKlant?.id ?? "";

  return await fetchKlantFromNonDefaultSystems(
    systemen,
    defaultSysteem,
    kvkNummer,
    vestigingsnummer,
    rsin,
    bsn,
    id,
  );
};

export const fetchKlantByNonDefaultSysteem = async (
  klant: Klant,
  systeem: Systeem,
): Promise<Klant | null> => {
  if (!klant) return null;

  const identifier = mapKlantToKlantIdentifier(systeem.registryVersion, klant);
  if (!identifier) return klant;

  return systeem.registryVersion === registryVersions.ok1
    ? await fetchKlantByKlantIdentificatorOk1(systeem.identifier, identifier)
    : await fetchKlantByKlantIdentificatorOk2(systeem.identifier, identifier);
};

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

export const heeftContactgegevens = (klant: Klant) =>
  klant.emailadressen?.length || klant.telefoonnummers?.length;

export const enrichKlantWithContactDetails = async (
  klant: Klant,
  systemen: Systeem[],
  defaultSysteem: Systeem,
) => {
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
};

export const fetchKlantByKlantIdentificatorOk = async (
  klantIdentificator: KlantIdentificator,
  defaultSysteem: Systeem,
) => {
  if (defaultSysteem.registryVersion === registryVersions.ok2) {
    return await fetchKlantByKlantIdentificatorOk2(
      defaultSysteem.identifier,
      klantIdentificator,
    );
  } else {
    return await fetchKlantByKlantIdentificatorOk1(
      defaultSysteem.identifier,
      klantIdentificator,
    );
  }
};

export async function fetchKlantFromNonDefaultSystems(
  systemen: Systeem[],
  defaultSysteem: Systeem,
  kvkNummer: string | undefined,
  vestigingsnummer: string | undefined,
  rsin: string | undefined,
  bsn: string | undefined,
  id: string,
): Promise<Klant | null> {
  for (const nonDefaultSysteem of systemen.filter(
    (s) => s.identifier !== defaultSysteem.identifier,
  )) {
    const fallbackKlant = await fetchKlantByNonDefaultSysteem(
      {
        kvkNummer: kvkNummer,
        vestigingsnummer: vestigingsnummer,
        rsin: rsin,
        bsn: bsn,

        //required fields
        _typeOfKlant: "klant",
        id: id,
        klantnummer: "",
        telefoonnummers: [],
        emailadressen: [],
        url: "",
      },
      nonDefaultSysteem,
    );

    return fallbackKlant;
  }
  return null;
}
