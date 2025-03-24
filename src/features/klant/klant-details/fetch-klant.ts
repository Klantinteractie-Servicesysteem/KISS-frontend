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
import { useContactmomentStore } from "@/stores/contactmoment";

export const fetchKlant = async ({
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
  const externalklantId = knownKlant?.klant.id;

  if (!externalklantId) {
    throw new Error(`Onbekende klant ${internalId}`);
  }

  //fetch klant form external registrie based on the externalId for the store
  const klant = await fetchKlantById(externalklantId, defaultSysteem);

  if (heeftContactgegevens(klant)) return klant;
  if (!systemen.length) return klant;

  for (const nonDefaultSysteem of systemen.filter(
    (s) => s.identifier !== defaultSysteem.identifier,
  )) {
    const fallbackKlant = await fetchKlantByNonDefaultSysteem(
      klant,
      nonDefaultSysteem,
    );
    if (heeftContactgegevens(fallbackKlant)) return fallbackKlant;
  }

  return klant;
};

const fetchKlantByNonDefaultSysteem = async (
  klant: Klant | null,
  systeem: Systeem,
): Promise<Klant | null> => {
  if (!klant) return null;

  const identifier = mapKlantToKlantIdentifier(systeem.registryVersion, klant);
  if (!identifier) return klant;

  const gevondenKlant =
    systeem.registryVersion === registryVersions.ok1
      ? await fetchKlantByKlantIdentificatorOk1(systeem.identifier, identifier)
      : await fetchKlantByKlantIdentificatorOk2(systeem.identifier, identifier);

  return gevondenKlant ? fetchKlantById(gevondenKlant.id, systeem) : klant;
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

const heeftContactgegevens = (klant: Klant | null) =>
  klant?.emailadressen?.length || klant?.telefoonnummers?.length;
