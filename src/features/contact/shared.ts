import type { OnderwerpObjectPostModel } from "@/services/openklant2";
import type { ContactmomentViewModel, KlantIdentificator } from "./types";
import { fetchZaakIdentificatieByUrlOrId } from "@/services/openzaak";
import type { ContactmomentViewModelOk1 } from "@/services/contactmomenten/service";

export const getIdentificatorForOk1And2 = ({
  bsn,
  vestigingsnummer,
  rsin,
  kvkNummer,
}: KlantIdentificator) => {
  switch (true) {
    case !!bsn:
      return { ok1: { bsn }, ok2: { bsn } } as const;
    case !!vestigingsnummer:
      return {
        ok1: { vestigingsnummer },
        ok2: { vestigingsnummer },
      } as const;
    case !!kvkNummer:
      // esuite wil een kvkNummer als niet-natuurlijk-persoon-Id
      return {
        ok1: { nietNatuurlijkPersoonIdentifier: kvkNummer },
        ok2: { kvkNummer, rsin },
      };
    case !!rsin:
      // esuite kan niks met een rsin
      return { ok2: { rsin } };

    default:
      return {};
  }
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
): Promise<ContactmomentViewModel> => ({
  ...contactmoment,
  zaaknummers: await Promise.all(
    objectcontactmomenten.map(({ object }) =>
      fetchZaakIdentificatieByUrlOrId(systeemId, object),
    ),
  ),
});
