import { fetchLoggedIn, throwIfNotOk, parseJson } from "@/services";
import {
  DigitaleAdressenExpand,
  enrichBetrokkeneWithDigitaleAdressen,
  enrichBetrokkeneWithKlantContact,
  enrichInterneTakenWithActoren,
  fetchBetrokkenen,
  filterOutContactmomenten,
  findKlantByIdentifier,
  KlantContactExpand,
  searchDigitaleAdressen,
  type Betrokkene,
  type BetrokkeneMetKlantContact,
  type ContactmomentViewModel,
  type DigitaalAdresExpandedApiViewModel,
} from "@/services/openklant2";
import {
  enrichContactverzoekObjectWithContactmoment,
  fetchKlantByIdentifierOpenKlant1,
} from "@/services/openklant1";
import type { ContactverzoekOverzichtItem } from "./types";
import type { ContactmomentDetails } from "../../contactmoment";
import { fullName } from "@/helpers/string";
import type { KlantIdentificator } from "../../types";
import {
  registryVersions,
  type Systeem,
} from "@/services/environment/fetch-systemen";
import { fetchInternetakenByKlantIdFromObjecten } from "@/services/internetaak/service";

function searchRecursive(urlStr: string, page = 1): Promise<any[]> {
  const url = new URL(urlStr);
  url.searchParams.set("page", page.toString());

  return fetchLoggedIn(url)
    .then(throwIfNotOk)
    .then(parseJson)
    .then(async (j) => {
      if (!Array.isArray(j?.results)) {
        throw new Error("Expected array: " + JSON.stringify(j));
      }

      if (!j.next) return j.results;
      const nextResults = await searchRecursive(urlStr, page + 1);
      return [...j.results, ...nextResults];
    });
}

async function searchOk2Recursive(
  adres: string,
  page = 1,
): Promise<DigitaalAdresExpandedApiViewModel[]> {
  const paginated = await searchDigitaleAdressen({
    adres,
    page,
    expand: [DigitaleAdressenExpand.verstrektDoorBetrokkene],
  });
  if (!paginated.next) return paginated.page;
  return [...paginated.page, ...(await searchOk2Recursive(adres, page + 1))];
}

export async function search(
  systeemId: string,
  query: string,
  gebruikKlantInteractiesApi: boolean,
): Promise<ContactverzoekOverzichtItem[]> {
  // OK2
  if (gebruikKlantInteractiesApi) {
    const adressen = await searchOk2Recursive(query);

    const betrokkenen = adressen
      .map((x) => x?._expand?.verstrektDoorBetrokkene)
      .filter(Boolean) as Betrokkene[];

    const uniqueBetrokkenen = new Map<string, Betrokkene>(
      betrokkenen.map((betrokkene) => [betrokkene.uuid, betrokkene]),
    );

    return enrichBetrokkeneWithKlantContact(
      systeemId,
      [...uniqueBetrokkenen.values()],
      [
        KlantContactExpand.leiddeTotInterneTaken,
        KlantContactExpand.gingOverOnderwerpobjecten,
      ],
    )
      .then(filterOutContactmomenten)
      .then((page) => enrichBetrokkeneWithDigitaleAdressen(systeemId, page))
      .then((page) => enrichInterneTakenWithActoren(systeemId, page))
      .then(mapKlantcontactToContactverzoekOverzichtItem)
      .then(filterOutGeauthenticeerdeContactverzoeken);
  }
  /// OK1 heeft geen interne taak, dus gaan we naar de objecten registratie
  else {
    const url = new URL("/api/internetaak/api/v2/objects", location.origin);
    url.searchParams.set("ordering", "-record__data__registratiedatum");
    url.searchParams.set("pageSize", "10");
    url.searchParams.set(
      "data_attr",
      `betrokkene__digitaleAdressen__icontains__${query}`,
    );

    return searchRecursive(url.toString())
      .then((x) =>
        Promise.all(
          x.map((obj) =>
            enrichContactverzoekObjectWithContactmoment(obj, systeemId),
          ),
        ),
      )
      .then((x) => x.map(mapObjectToContactverzoekOverzichtItem))
      .then(filterOutGeauthenticeerdeContactverzoeken);
  }
}

function filterOutGeauthenticeerdeContactverzoeken(
  value: ContactverzoekOverzichtItem[],
) {
  return value.filter((x) => !x.betrokkene?.isGeauthenticeerd);
}

function mapKlantcontactToContactverzoekOverzichtItem(
  betrokkeneMetKlantcontact: BetrokkeneMetKlantContact[],
): ContactverzoekOverzichtItem[] {
  return betrokkeneMetKlantcontact.map(
    ({ klantContact, contactnaam, expandedDigitaleAdressen, wasPartij }) => {
      const internetaak = klantContact._expand?.leiddeTotInterneTaken?.[0];
      if (!internetaak) {
        throw new Error("");
      }

      return {
        url: internetaak.url,
        onderwerp: klantContact.onderwerp,
        toelichtingBijContactmoment: klantContact.inhoud,
        status: internetaak.status,
        registratiedatum: klantContact.plaatsgevondenOp,
        vraag: klantContact.onderwerp,
        aangemaaktDoor: klantContact.hadBetrokkenActoren?.[0]?.naam || "",
        behandelaar: internetaak?.actor?.naam,
        toelichtingVoorCollega: internetaak.toelichting,
        betrokkene: {
          persoonsnaam: contactnaam,
          digitaleAdressen: expandedDigitaleAdressen || [],
          isGeauthenticeerd: !!wasPartij,
        },
        objecten:
          klantContact?._expand?.gingOverOnderwerpobjecten?.map((x) => ({
            object: x.onderwerpobjectidentificator.objectId,
            objectType: x.onderwerpobjectidentificator.codeObjecttype,
            contactmoment: x.klantcontact.uuid,
          })) || [],
      } satisfies ContactverzoekOverzichtItem;
    },
  );
}

function mapObjectToContactverzoekOverzichtItem({
  contactverzoekObject,
  contactmoment,
  details,
}: {
  contactverzoekObject: any;
  contactmoment: ContactmomentViewModel | null;
  details: ContactmomentDetails | null;
}): ContactverzoekOverzichtItem {
  const getVraag = (cd: ContactmomentDetails | null) => {
    const { vraag, specifiekeVraag } = cd || {};
    if (!vraag) return specifiekeVraag;
    if (!specifiekeVraag) return vraag;
    return `${vraag} (${specifiekeVraag})`;
  };
  const vraag = getVraag(details) || "";
  const record = contactverzoekObject.record;
  const data = record.data;

  return {
    url: contactverzoekObject.url,
    onderwerp: vraag,
    toelichtingBijContactmoment: contactmoment?.tekst || "",
    status: data.status || "onbekend",
    registratiedatum: data.registratiedatum,
    vraag,
    toelichtingVoorCollega: data.toelichting || "",
    behandelaar: data.actor?.naam || "",
    betrokkene: {
      isGeauthenticeerd: !!data.betrokkene?.klant,
      persoonsnaam: data.betrokkene?.persoonsnaam || {},
      digitaleAdressen: data.betrokkene?.digitaleAdressen || [],
    },
    aangemaaktDoor: fullName(contactmoment?.medewerkerIdentificatie),
    objecten: contactmoment?.objectcontactmomenten || [],
  } satisfies ContactverzoekOverzichtItem;
}

export async function fetchContactverzoekenByKlantIdentificator(
  id: KlantIdentificator,
  systemen: Systeem[],
): Promise<ContactverzoekOverzichtItem[]> {
  const { bsn, vestigingsnummer, rsin, kvkNummer } = id;

  let klantIdentifier1, klantIdentifier2;

  switch (true) {
    case !!bsn:
      klantIdentifier1 = { bsn };
      klantIdentifier2 = { bsn };
      break;
    case !!vestigingsnummer:
      klantIdentifier1 = { vestigingsnummer };
      klantIdentifier2 = { vestigingsnummer };
      break;
    case !!kvkNummer:
      // esuite wil een kvkNummer als niet-natuurlijk-persoon-Id
      klantIdentifier1 = { nietNatuurlijkPersoonIdentifier: kvkNummer };
      klantIdentifier2 = { kvkNummer, rsin };
      break;
    case !!rsin:
      // dan maar proberen met de rsin?
      klantIdentifier1 = { nietNatuurlijkPersoonIdentifier: rsin };
      klantIdentifier2 = { rsin };
      break;

    default:
      return [];
  }

  const promises = systemen.map((systeem) => {
    if (systeem.registryVersion === registryVersions.ok1) {
      return fetchKlantByIdentifierOpenKlant1(
        systeem.identifier,
        klantIdentifier1,
      )
        .then((klant) =>
          !klant?.url
            ? []
            : fetchInternetakenByKlantIdFromObjecten({
                systeemId: systeem.identifier,
                klantUrl: klant.url,
              }).then(({ page }) => page),
        )
        .then(async (page) => {
          const result = [];
          for (const obj of page) {
            result.push(
              await enrichContactverzoekObjectWithContactmoment(
                obj,
                systeem.identifier,
              ).then(mapObjectToContactverzoekOverzichtItem),
            );
          }
          return result;
        });
    }
    return findKlantByIdentifier(systeem.identifier, klantIdentifier2).then(
      (klant) =>
        !klant?.id
          ? []
          : fetchBetrokkenen({
              systeemId: systeem.identifier,
              pageSize: "100",
              wasPartij__url: klant.id,
            }).then(({ page }) =>
              enrichBetrokkeneWithKlantContact(systeem.identifier, page, [
                KlantContactExpand.leiddeTotInterneTaken,
                KlantContactExpand.gingOverOnderwerpobjecten,
              ])
                .then(filterOutContactmomenten)
                .then((page) =>
                  enrichBetrokkeneWithDigitaleAdressen(
                    systeem.identifier,
                    page,
                  ),
                )
                .then((page) =>
                  enrichInterneTakenWithActoren(systeem.identifier, page),
                )
                .then(mapKlantcontactToContactverzoekOverzichtItem),
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
