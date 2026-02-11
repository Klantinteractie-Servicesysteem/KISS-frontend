import type { Kanaal } from "@/features/types";
import { fetchLoggedIn, ServiceResult, throwIfNotOk } from "@/services";

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

export function verwijderkanaal(id: number) {
  return fetchLoggedIn("/api/KanaalVerwijderen/" + id, {
    method: "DELETE",
  });
}
