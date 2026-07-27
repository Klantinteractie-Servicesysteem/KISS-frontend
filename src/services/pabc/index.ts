import { fetchLoggedIn, throwIfNotOk, parseJson, useLoader } from "..";

export type PabcStatus = {
  usePabc: boolean;
};

export type AllowedZaaktypen = {
  isFiltered: boolean;
  zaaktypen: string[] | null;
};

const fetchUsePabc = (): Promise<boolean> =>
  fetchLoggedIn("/api/environment/use-pabc")
    .then(throwIfNotOk)
    .then(parseJson)
    .then(({ usePabc }) => usePabc as boolean);

const fetchAllowedZaaktypen = (): Promise<AllowedZaaktypen> =>
  fetchLoggedIn("/api/pabc/allowed-zaaktypen")
    .then(throwIfNotOk)
    .then(parseJson)
    .then((json) => json as AllowedZaaktypen);

export const usePabcStatus = () => useLoader(fetchUsePabc);

export const useAllowedZaaktypen = () => useLoader(fetchAllowedZaaktypen);
