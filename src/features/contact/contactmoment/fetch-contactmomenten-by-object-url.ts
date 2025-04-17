import {
  type Systeem,
  registryVersions,
} from "@/services/environment/fetch-systemen";
import { fetchKlantcontacten, KlantContactExpand } from "@/services/openklant2";
import { mapKlantContactToContactmomentViewModel } from "./service";
import {
  enrichContactmomentWithZaaknummer,
  enrichOnderwerpObjectenWithZaaknummers,
} from "../shared";
import { fetchContactmomentenByObjectUrlOk1 } from "@/services/contactmomenten/service";

export function fetchContactmomentenByObjectUrl(systeem: Systeem, url: string) {
  if (systeem.registryVersion === registryVersions.ok2) {
    // OK2
    const id = url.split("/").at(-1);
    if (!id) return Promise.reject("missing id");

    return fetchKlantcontacten({
      systeemIdentifier: systeem.identifier,
      onderwerpobject__onderwerpobjectidentificatorObjectId: id,
      expand: [KlantContactExpand.gingOverOnderwerpobjecten],
    }).then(async (paginated) => ({
      ...paginated,
      page: await Promise.all(
        paginated.page.map(async (contact) =>
          enrichOnderwerpObjectenWithZaaknummers(
            systeem.identifier,
            contact._expand.gingOverOnderwerpobjecten || [],
          ).then((zaaknummers) =>
            mapKlantContactToContactmomentViewModel(
              systeem.identifier,
              contact,
              zaaknummers,
            ),
          ),
        ),
      ),
    }));
  }

  // OK1

  return fetchContactmomentenByObjectUrlOk1({
    systeemIdentifier: systeem.identifier,
    objectUrl: url,
  }).then(async (paginated) => ({
    ...paginated,
    page: await Promise.all(
      paginated.page.map((item) =>
        enrichContactmomentWithZaaknummer(systeem.identifier, item),
      ),
    ),
  }));
}
