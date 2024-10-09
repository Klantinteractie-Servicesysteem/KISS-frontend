import { ensureKlantForBsn as ensureKlantForBsn1 } from "@/services/openklant1";
import { findKlantByIdentifier, createKlant as createKlant2 } from "@/services/openklant2";
import { useOpenKlant2 } from "@/services/openklant2/service";
import { useOrganisatieIds } from "@/stores/user"; 

export const ensureKlantForBsn = async (
  parameters: { bsn: string }
) => {

  const isOpenKlant2 = await useOpenKlant2();

  if (isOpenKlant2) {
    // Gebruik openklant2 implementatie
    return (await findKlantByIdentifier(parameters)) ?? (await createKlant2(parameters));
  } else {
    // Gebruik openklant1 implementatie
    const organisatieIds = useOrganisatieIds();
    const bronorganisatie = organisatieIds.value[0] || "";
    
    return await ensureKlantForBsn1(parameters, bronorganisatie);
  }
};
