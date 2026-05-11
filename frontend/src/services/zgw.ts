export interface PaginatedResult<T> {
  next: string | null;
  previous: string | null;
  count: number;
  page: T[];
}

//Dit is aangepast in de context van OpenZaak.
//Het is nog onduidelijk of een generieke pagination functie bruikbaar blijft.
//De data komt namelijk niet langer een enkele gateway.
//Mogelijk refactoren naar api specifieke paginerings functies.
//oude situatie hebben we ter referentie nog even uitgecommentarieerd laten staan

export async function parsePagination<T>(
  json: unknown,
  map: (jObj: unknown) => T
): Promise<PaginatedResult<Awaited<T>>> {
  const { count, next, previous, results } = json as {
    [key: string]: unknown;
  };
  if (
    !Array.isArray(results) ||
    typeof count !== "number" ||
    (typeof next != "string" && next != null) ||
    (typeof previous != "string" && previous != null)
  )
    throw new Error(
      "unexpected format. expected pagination: " + JSON.stringify(json)
    );

  // const { results, limit, total, page, pages } = json as {
  //   [key: string]: unknown;
  // };
  // if (
  //   !Array.isArray(results) ||
  //   typeof limit !== "number" ||
  //   typeof total !== "number" ||
  //   typeof page !== "number" ||
  //   typeof pages !== "number"
  // )
  //   throw new Error(
  //     "unexpected in gateway json. expected pagination: " + JSON.stringify(json)
  //   );

  // just in case the mapper is async, we wrap the result in a Promise
  const promises = results.map((x) => Promise.resolve(map(x)));

  return {
    page: await Promise.all(promises),
    next: next ?? null,
    previous: previous ?? null,
    count,
    //pageNumber: page,
    //pageSize: limit,
    //totalPages: pages,
    //totalRecords: total,
  };
}

//date format helper: 2005-12-30UTC01:02:03
export const getFormattedUtcDate = () => {
  const formatDateTimeElement = (x: number) => ("0" + x).slice(-2);

  const now = new Date();

  return `${now.getFullYear()}-${formatDateTimeElement(
    now.getMonth() + 1
  )}-${formatDateTimeElement(now.getDate())}UTC${formatDateTimeElement(
    now.getUTCHours()
  )}:${formatDateTimeElement(now.getUTCMinutes())}:${formatDateTimeElement(
    now.getSeconds()
  )}`;
};
