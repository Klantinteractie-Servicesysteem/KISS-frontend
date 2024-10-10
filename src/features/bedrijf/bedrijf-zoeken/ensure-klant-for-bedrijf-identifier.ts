import { findKlantByIdentifier, createKlant, type KlantBedrijfIdentifier } from "@/services/openklant2";
import { ensureKlantForBedrijfIdentifier as ensureKlantForBedrijfsIdentifier } from "@/services/openklant1/service";
import { useOpenKlant2 } from "@/services/openklant2/service";
import { mapBedrijfsIdentifier } from "@/services/openklant1/service";
import { useOrganisatieIds } from "@/stores/user"; 

export const ensureKlantForBedrijfIdentifier = async (
  identifier: KlantBedrijfIdentifier,
  bedrijfsnaam: string,
) => {

  const isOpenKlant2 = await useOpenKlant2();

  if (isOpenKlant2) {
    // Gebruik openklant2 implementatie
    const klant = await findKlantByIdentifier(identifier);
    return klant ?? (await createKlant(identifier));
  } else {
    // Gebruik openklant1 implementatie
    const mappedIdentifier = mapBedrijfsIdentifier(identifier);
    const organisatieIds = useOrganisatieIds();
    const organisatieId = organisatieIds.value[0] || "";

    return await ensureKlantForBedrijfsIdentifier({
      bedrijfsnaam,
      identifier: mappedIdentifier,
    }, organisatieId);
  }
};
