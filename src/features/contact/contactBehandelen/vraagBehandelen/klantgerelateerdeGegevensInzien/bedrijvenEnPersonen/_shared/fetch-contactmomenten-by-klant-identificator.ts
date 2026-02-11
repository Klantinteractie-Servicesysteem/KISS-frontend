import { fetchContactmomentenByKlantUrlOk1 } from "@/services/contactmomenten/service";
import {
  type Systeem,
  registryVersions,
} from "@/services/environment/fetch-systemen";
import { fetchKlantByKlantIdentificatorOk1 } from "@/services/openklant1";
import {
  fetchBetrokkenen,
  enrichBetrokkeneWithKlantContact,
  KlantContactExpand,
  fetchKlantByKlantIdentificatorOk2,
} from "@/services/openklant2";
import type { ContactmomentViewModel } from "../../../../../types";
import {
  enrichContactmomentWithZaaknummer,
  enrichOnderwerpObjectenWithZaaknummers,
} from "../../../../../shared";
import {
  enrichContactmomentWithDetails,
  mapKlantContactToContactmomentViewModel,
} from "../../service";
import type { KlantIdentificator } from "@/services/openklant/types";
import type { ContactmomentDetails } from "../../../../../contactAfronden/write-contactmoment-details";

export async function fetchContactmomentenByKlantIdentificator(
  klantIndentificator: KlantIdentificator,
  systemen: Systeem[],
): Promise<Array<ContactmomentViewModel & Partial<ContactmomentDetails>>> {
  const promises = systemen.map(async (systeem) => {
    if (systeem.registryVersion === registryVersions.ok1) {
      const klant = await fetchKlantByKlantIdentificatorOk1(
        systeem.identifier,
        klantIndentificator,
      );
      if (!klant?.url) return [];

      const { page } = await fetchContactmomentenByKlantUrlOk1({
        systeemIdentifier: systeem.identifier,
        klantUrl: klant.url,
      });

      return Promise.all(
        page.map((item) =>
          enrichContactmomentWithZaaknummer(systeem.identifier, item).then(
            enrichContactmomentWithDetails,
          ),
        ),
      );
    }

    const klant = await fetchKlantByKlantIdentificatorOk2(
      systeem.identifier,
      klantIndentificator,
    );

    if (!klant?.url) return [];

    const paginated = await fetchBetrokkenen({
      systeemId: systeem.identifier,
      wasPartij__url: klant.url,
      pageSize: "100",
    });

    const enrichedPage = await enrichBetrokkeneWithKlantContact(
      systeem.identifier,
      paginated.page,
      [KlantContactExpand.gingOverOnderwerpobjecten],
    );

    return Promise.all(
      enrichedPage.map(({ klantContact }) =>
        enrichOnderwerpObjectenWithZaaknummers(
          systeem.identifier,
          klantContact._expand.gingOverOnderwerpobjecten || [],
        )
          .then((zaaknummers) =>
            mapKlantContactToContactmomentViewModel(
              systeem.identifier,
              klantContact,
              zaaknummers,
            ),
          )
          .then(enrichContactmomentWithDetails),
      ),
    );
  });

  return Promise.all(promises)
    .then((all) => all.flat())
    .then((all) =>
      all.sort(
        (a, b) =>
          new Date(b.registratiedatum).valueOf() -
          new Date(a.registratiedatum).valueOf(),
      ),
    );
}
