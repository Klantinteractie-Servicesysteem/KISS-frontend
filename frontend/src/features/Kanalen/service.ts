import { fetchLoggedIn, ServiceResult, throwIfNotOk } from "@/services";

type Kanaal = {
  id: number,
  naam: string
}

async function fetchAll(url: string): Promise<Kanaal[]> {
  return await fetchLoggedIn(url)
    .then(throwIfNotOk)
    .then((r) => r.json());
}

export function useKanalen() {
  const kanalen = ServiceResult.fromFetcher(
    "/api/KanalenBeheerOverzicht",
    fetchAll,
  );

  return kanalen;
}

export function useKanalenKeuzeLijst() {
  return ServiceResult.fromFetcher(
    "/api/KanalenContactmomentKeuzelijst",
    fetchAll,
  );
}

export function verwijderkanaal(id: number) {
  return fetchLoggedIn("/api/KanaalVerwijderen/" + id, {
    method: "DELETE",
  });
}
