import {
  type KlantBedrijfIdentifier,
  ensureOk2Klant,
} from "@/services/openklant2";
import {
  ensureKlantForBedrijfIdentifier as ensureKlantForBedrijfIdentifierOk1,
  mapBedrijfsIdentifier,
} from "@/services/openklant1/service";
import { useOrganisatieIds } from "@/stores/user";
import {
  registryVersions,
  fetchSystemen,
  type Systeem,
} from "@/services/environment/fetch-systemen";

export const ensureKlantForBedrijfIdentifier = async (
  klantbedrijfidentifier: KlantBedrijfIdentifier,
  bedrijfsnaam: string,
) => {
  const systemen = await fetchSystemen();
  const defaultSysteem = systemen.find((systeem) => systeem.isDefault);

  if (!defaultSysteem) {
    throw new Error("Geen default register gevonden");
  }

  if (defaultSysteem.registryVersion === registryVersions.ok2) {
    return await ensureOk2Klant(
      defaultSysteem.identifier,
      klantbedrijfidentifier,
    );
  } else {
    const mappedIdentifier = mapBedrijfsIdentifier(klantbedrijfidentifier);
    const organisatieIds = useOrganisatieIds();
    const organisatieId = organisatieIds.value[0] || "";

    return await ensureKlantForBedrijfIdentifierOk1(
      defaultSysteem.identifier,
      { bedrijfsnaam, identifier: mappedIdentifier },
      organisatieId,
    );
  }
};
