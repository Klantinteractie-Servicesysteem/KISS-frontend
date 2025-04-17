import type { OnderwerpObjectPostModel } from "@/services/openklant2";
import type { ContactmomentViewModel, KlantIdentificator } from "./types";
import { fetchZaakIdentificatieByUrlOrId } from "@/services/openzaak";
import {
  fetchObjectContactmomenten,
  type ContactmomentViewModelOk1,
} from "@/services/contactmomenten/service";
import type { Klant } from "@/services/openklant/types";
import { registryVersions } from "@/services/environment/fetch-systemen";

export const mapKlantToKlantIdentifier = (
  registryVersion: string,
  klant: Klant,
): KlantIdentificator => {
  return registryVersion === registryVersions.ok1
    ? {
        bsn: klant.bsn,
        vestigingsnummer: klant.vestigingsnummer,
        kvkNummer: klant.nietNatuurlijkPersoonIdentifier ?? klant.kvkNummer,
        //esuite doet niks met rsin
      }
    : {
        bsn: klant.bsn,
        vestigingsnummer: klant.vestigingsnummer,
        kvkNummer: klant.kvkNummer,
        rsin: klant.rsin,
      };
};

export const enrichOnderwerpObjectenWithZaaknummers = (
  systeemId: string,
  objecten: OnderwerpObjectPostModel[],
) =>
  Promise.all(
    objecten.map(({ onderwerpobjectidentificator: { objectId } }) =>
      fetchZaakIdentificatieByUrlOrId(systeemId, objectId),
    ),
  );

export const enrichContactmomentWithZaaknummer = async (
  systeemId: string,
  { objectcontactmomenten, ...contactmoment }: ContactmomentViewModelOk1,
): Promise<ContactmomentViewModel> => {
  if (!objectcontactmomenten) {
    objectcontactmomenten = await fetchObjectContactmomenten({
      systeemIdentifier: systeemId,
      contactmomentUrl: contactmoment.url,
    }).then(({ page }) => page);
  }
  return {
    ...contactmoment,
    zaaknummers: await Promise.all(
      objectcontactmomenten.map(({ object }) =>
        fetchZaakIdentificatieByUrlOrId(systeemId, object),
      ),
    ),
  };
};
