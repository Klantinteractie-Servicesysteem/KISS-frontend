import type { OnderwerpObjectPostModel } from "@/services/openklant2";
import type { ContactmomentViewModel } from "./types";
import { fetchZaakIdentificatieByUrlOrId } from "@/services/openzaak";
import {
  fetchObjectContactmomenten,
  type ContactmomentViewModelOk1,
} from "@/services/contactmomenten/service";

export const enrichOnderwerpObjectenWithZaaknummers = (
  systeemId: string,
  objecten: OnderwerpObjectPostModel[],
) =>
  Promise.all(
    objecten
      .filter(({ onderwerpobjectidentificator }) => {
        // Check if this is a zaak-type object
        return (
          onderwerpobjectidentificator.codeObjecttype === "zgw-Zaak" &&
          onderwerpobjectidentificator.codeRegister === "openzaak"
        );
      })
      .map(({ onderwerpobjectidentificator: { objectId } }) =>
        fetchZaakIdentificatieByUrlOrId(systeemId, objectId),
      ),
  ).then((results) => results.filter(Boolean)); // Filter out any null/undefined results

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
