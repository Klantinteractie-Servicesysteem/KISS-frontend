import { fetchContactmomentenByKlantUrlOk1 as fetchContactmomentenByKlantUrlOk1 } from "@/services/contactmomenten/service";
import {
  type Systeem,
  registryVersions,
} from "@/services/environment/fetch-systemen";
import { fetchKlantByIdentifierOpenKlant1 } from "@/services/openklant1";
import {
  findKlantByIdentifier,
  fetchBetrokkenen,
  enrichBetrokkeneWithKlantContact,
  KlantContactExpand,
  mapKlantContactToContactmomentViewModel,
  type ContactmomentViewModel,
} from "@/services/openklant2";
import type { KlantIdentificator } from "../types";
import { getIdentificatorForOk1And2 } from "../shared";

export async function fetchContactmomentenByKlantIdentificator(
  id: KlantIdentificator,
  systemen: Systeem[],
): Promise<ContactmomentViewModel[]> {
  const klantidentificators = getIdentificatorForOk1And2(id);

  const promises = systemen.map((systeem) => {
    if (systeem.registryVersion === registryVersions.ok1) {
      if (!klantidentificators.ok1) return [];
      return fetchKlantByIdentifierOpenKlant1(
        systeem.identifier,
        klantidentificators.ok1,
      ).then((klant) =>
        !klant?.url
          ? []
          : fetchContactmomentenByKlantUrlOk1({
              systeemIdentifier: systeem.identifier,
              klantUrl: klant.url,
            }).then(({ page }) =>
              page.map((cm) => ({
                ...cm,
                systeemId: systeem.identifier,
              })),
            ),
      );
    }
    if (!klantidentificators.ok2) return [];
    return findKlantByIdentifier(
      systeem.identifier,
      klantidentificators.ok2,
    ).then((klant) =>
      !klant?.url
        ? []
        : fetchBetrokkenen({
            systeemId: systeem.identifier,
            wasPartij__url: klant.url,
            pageSize: "100",
          })
            .then(async (paginated) =>
              enrichBetrokkeneWithKlantContact(
                systeem.identifier,
                paginated.page,
                [KlantContactExpand.gingOverOnderwerpobjecten],
              ),
            )
            .then((page) =>
              page.map(({ klantContact }) =>
                mapKlantContactToContactmomentViewModel(
                  systeem.identifier,
                  klantContact,
                ),
              ),
            ),
    );
  });

  return Promise.all(promises).then((all) =>
    all
      .flat()
      .sort(
        (a, b) =>
          new Date(b.registratiedatum).valueOf() -
          new Date(a.registratiedatum).valueOf(),
      ),
  );
}
