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

import { searchBedrijvenInHandelsRegisterByKvkNummer } from "@/services/kvk";
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

    if (!klant.bsn && !klant.vestigingsnummer && klant.kvkNummer) {
      //als we zaken zoeken in een zgw zaaksysteeem (behalve de esuite), dan hebben we daar de rsin voor nodig.
      //voor een nietnatuurlijk persoon zoeken we adhv het kvkNummer.
      //dit hoeft dus niet als er een vestigingsnummer is.

      const bedrijf = await searchBedrijvenInHandelsRegisterByKvkNummer(
        klant.kvkNummer,
      ).then(enforceOneOrZero);

      if (!bedrijf) return klant;

      klant.rsin = bedrijf.rsin;
    }

    await enrichKlantWithContactDetails(klant, systemen, defaultSysteem);

    return klant;
  }

  //Klant is not available in the default klantregister

  const kvkNummer = internalKlant?.kvkNummer;
  const vestigingsnummer = internalKlant?.vestigingsnummer;
  const bsn = internalKlant?.bsn;

  return await fetchKlantFromNonDefaultSystems(
    systemen,
    defaultSysteem,
    kvkNummer,
    vestigingsnummer,
    bsn,
  );
};

export const fetchKlantByNonDefaultSysteem = async (
  identifier: KlantIdentificator,
  systeem: Systeem,
): Promise<Klant | null> => {
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
    const identifier = mapKlantToKlantIdentifier(
      nonDefaultSysteem.registryVersion,
      klant,
    );
    if (!identifier) return klant;

    const fallbackKlant = await fetchKlantByNonDefaultSysteem(
      identifier,
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
  bsn: string | undefined,
  //id: string,
): Promise<Klant | null> {
  for (const nonDefaultSysteem of systemen.filter(
    (s) => s.identifier !== defaultSysteem.identifier,
  )) {
    const identifier = mapKlantToKlantIdentifier(
      nonDefaultSysteem.registryVersion,
      {
        kvkNummer: kvkNummer,
        vestigingsnummer: vestigingsnummer,
        bsn: bsn,

        //required fields
        _typeOfKlant: "klant",
        // id: id,
        klantnummer: "",
        telefoonnummers: [],
        emailadressen: [],
        url: "",
      },
    );

    if (!identifier) return null;

    const fallbackKlant = await fetchKlantByNonDefaultSysteem(
      identifier,
      nonDefaultSysteem,
    );

    return fallbackKlant;
  }
  return null;
}
