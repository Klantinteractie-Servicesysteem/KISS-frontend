import { fetchKlantByKlantIdentificatorOk2 } from "@/services/openklant2";
import { fetchKlantByKlantIdentificatorOk1 } from "@/services/openklant1";
import {
  registryVersions,
  type Systeem,
} from "@/services/environment/fetch-systemen";
import type { Klant } from "@/services/openklant/types";
import type { KlantIdentificatie } from "@/stores/contactmoment";

export const fetchKlant = async ({
  id,
  systemen,
}: {
  id: KlantIdentificatie;
  systemen: Systeem[];
}): Promise<Klant | null> => {
  const defaultSysteem = systemen.find((x) => x.isDefault);
  const others = systemen.filter((x) => x !== defaultSysteem);

  let klant =
    defaultSysteem && (await fetchContactgegevens(id, defaultSysteem));
  if (klant && heeftContactgegevens(klant)) return klant;

  for (const systeem of others) {
    klant = await fetchContactgegevens(id, systeem);
    if (klant && heeftContactgegevens(klant)) return klant;
  }

  return null;
};

const fetchContactgegevens = (id: KlantIdentificatie, systeem: Systeem) =>
  systeem.registryVersion === registryVersions.ok1
    ? fetchKlantByKlantIdentificatorOk1(systeem.identifier, id)
    : fetchKlantByKlantIdentificatorOk2(systeem.identifier, id);

const heeftContactgegevens = (klant: Klant) =>
  klant.emailadressen?.length || klant.telefoonnummers?.length;
