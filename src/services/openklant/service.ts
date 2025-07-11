import { registryVersions, type Systeem } from "../environment/fetch-systemen";
import { fetchKlantByKlantIdentificatorOk1 } from "../openklant1";
import { fetchKlantByKlantIdentificatorOk2 } from "../openklant2";
import type { Klant, KlantIdentificator } from "./types";

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

export const fetchKlantByNonDefaultSysteem = async (
  identifier: KlantIdentificator,
  systeem: Systeem,
): Promise<Klant | null> => {
  return systeem.registryVersion === registryVersions.ok1
    ? await fetchKlantByKlantIdentificatorOk1(systeem.identifier, identifier)
    : await fetchKlantByKlantIdentificatorOk2(systeem.identifier, identifier);
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
    const identifier = {
      bsn: bsn,
      vestigingsnummer: vestigingsnummer,
      kvkNummer: kvkNummer,
    };

    if (!identifier) return null;

    const fallbackKlant = await fetchKlantByNonDefaultSysteem(
      identifier,
      nonDefaultSysteem,
    );

    return fallbackKlant;
  }
  return null;
}

export const heeftContactgegevens = (klant: Klant) =>
  klant.emailadressen?.length || klant.telefoonnummers?.length;

export const SPECIFIEKEVRAAG_MAXLENGTH = 180;
