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
} from "@/services/openklant2";
import type { ContactmomentViewModel, KlantIdentificator } from "../types";
import {
  enrichContactmomentWithZaaknummer,
  enrichOnderwerpObjectenWithZaaknummers,
  getIdentificatorForOk1And2,
} from "../shared";
import { mapKlantContactToContactmomentViewModel } from "./service";

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
              Promise.all(
                page.map((item) =>
                  enrichContactmomentWithZaaknummer(systeem.identifier, item),
                ),
              ),
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
              Promise.all(
                page.map(({ klantContact }) =>
                  enrichOnderwerpObjectenWithZaaknummers(
                    systeem.identifier,
                    klantContact._expand.gingOverOnderwerpobjecten || [],
                  ).then((zaaknummers) =>
                    mapKlantContactToContactmomentViewModel(
                      systeem.identifier,
                      klantContact,
                      zaaknummers,
                    ),
                  ),
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
