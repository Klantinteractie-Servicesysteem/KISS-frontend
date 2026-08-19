import { fetchLoggedIn, throwIfNotOk, parseJson, useLoader } from "..";

export type PabcStatus = {
  usePabc: boolean;
};

const fetchUsePabc = (): Promise<boolean> =>
  fetchLoggedIn("/api/environment/use-pabc")
    .then(throwIfNotOk)
    .then(parseJson)
    .then(({ usePabc }) => usePabc as boolean);

export const usePabcStatus = () => useLoader(fetchUsePabc);
