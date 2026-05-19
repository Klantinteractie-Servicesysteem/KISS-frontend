import { formatIsoDate } from "@/helpers/date";
import type {} from "@/helpers/validation";
import { fetchLoggedIn, throwIfNotOk, FriendlyError } from "@/services";
import type { Persoon, PersoonQuery } from "./types";

const zoekUrl = "/api/haalcentraal/brp/personen";

const minimalFields = [
  "burgerservicenummer",
  "geboorte.datum",
  "geslacht",
  "adressering.adresregel1",
  "adressering.adresregel2",
  "adressering.adresregel3",
  "adressering.land.omschrijving",
  "naam.voornamen",
  "naam.voorvoegsel",
  "naam.geslachtsnaam",
];

type Compare<T> = (a: T, b: T) => number;

function combineCompare<T>(...comparers: Compare<T>[]): Compare<T> {
  return (a, b) => {
    let result = 0;
    for (const comparer of comparers) {
      result = comparer(a, b);
      if (result !== 0) return result;
    }
    return result;
  };
}

function sortBy<T>(
  ...properties: Array<(x: T) => string | undefined>
): Compare<T> {
  return combineCompare(
    ...properties.map((getProp) => (a: T, b: T) => {
      const propA = getProp(a);
      const propB = getProp(b);
      if (!propA && !propB) return 0;
      if (!propB) return -1;
      if (!propA) return 1;
      return propA.localeCompare(propB);
    }),
  );
}

const compareNaam = sortBy<Persoon>(
  (x) => x.achternaam,
  (x) => x.voorvoegselAchternaam,
  (x) => x.voornaam,
);

const compareAdres = sortBy<Persoon>(
  (x) => x.adresregel1,
  (x) => x.adresregel2,
  (x) => x.adresregel3,
);

const compareAdresThenNaam = combineCompare(compareAdres, compareNaam);

function mapPersoon(json: any): Persoon {
  const {
    adressering,
    naam,
    geboorte,
    burgerservicenummer,
    geslacht,
    geheimhoudingPersoonsgegevens,
  } = json ?? {};
  const { plaats, land, datum } = geboorte ?? {};

  const { adresregel1, adresregel2, adresregel3 } = adressering ?? {};
  const { geslachtsnaam, voornamen, voorvoegsel } = naam ?? {};
  const geboortedatum = datum?.datum && new Date(datum.datum);

  return {
    _typeOfKlant: "persoon",
    bsn: burgerservicenummer,
    geboortedatum,
    geslacht: geslacht?.omschrijving,
    voornaam: voornamen,
    voorvoegselAchternaam: voorvoegsel,
    achternaam: geslachtsnaam,
    geboorteplaats: plaats?.omschrijving,
    geboorteland: land?.omschrijving,
    adresregel1,
    adresregel2,
    adresregel3,
    geheimhoudingPersoonsgegevens,
  };
}

export const searchPersonen = (query: PersoonQuery) => {
  let request, sorter: Compare<Persoon>;

  if ("bsn" in query) {
    request = {
      burgerservicenummer: [query.bsn],
      type: "RaadpleegMetBurgerservicenummer",
      fields: [...minimalFields, "geboorte.land", "geboorte.plaats"],
    };
    sorter = compareNaam;
  } else if ("geslachtsnaamGeboortedatum" in query) {
    const { geboortedatum, geslachtsnaam } = query.geslachtsnaamGeboortedatum;
    request = {
      geboortedatum: formatIsoDate(geboortedatum),
      geslachtsnaam: geslachtsnaam?.endsWith("*")
        ? geslachtsnaam
        : geslachtsnaam + "*",
      type: "ZoekMetGeslachtsnaamEnGeboortedatum",
      fields: [...minimalFields],
    };
    sorter = compareNaam;
  } else {
    const {
      postcode: { numbers, letters },
      huisnummer,
      toevoeging,
      huisletter,
      achternaam,
    } = query.postcodeHuisnummerAchternaam;

    request = {
      postcode: numbers + letters,
      huisnummer,
      huisnummertoevoeging: toevoeging || "",
      huisletter: huisletter || "",
      type: "ZoekMetPostcodeEnHuisnummer",
      fields: [...minimalFields],
      geslachtsnaam: achternaam ? achternaam + "*" : undefined,
    };

    sorter = compareAdresThenNaam;
  }

  return fetchLoggedIn(zoekUrl, {
    method: "POST",
    headers: {
      "Content-Type": "application/json",
    },
    body: JSON.stringify(request),
  })
    .then(async (r) => {
      const contentType = r.headers.get("content-type");
      if (
        r.status === 400 &&
        contentType?.match(/^application\/problem\+json.*/)?.[0]
      ) {
        const json = await r.json();
        if (json?.code && json?.detail) {
          throw new FriendlyError(json.detail);
        }
      }
      throwIfNotOk(r);
      return r.json();
    })
    .then(({ personen }: { personen: unknown[] }) =>
      personen.map(mapPersoon).sort(sorter),
    );
};
