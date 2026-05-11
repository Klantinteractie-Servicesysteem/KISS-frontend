export * from "./service-data";
export * from "./service-data-enricher";
export * from "./fetch-logged-in";
export * from "./pagination";
export * from "./zgw";
export * from "./use-loader";

export function parseValidInt(input: unknown): number | undefined {
  if (typeof input === "number") {
    return isFinite(input) ? input : undefined;
  }
  if (typeof input !== "string") return undefined;
  const parsed = Number.parseInt(input, 10);
  return isFinite(parsed) ? parsed : undefined;
}

export function parseValidUrl(input: unknown): URL | undefined {
  if (input instanceof URL) return input;
  if (typeof input !== "string") return undefined;
  try {
    return new URL(input);
  } catch (error) {
    return undefined;
  }
}

export interface LookupList<K, V> {
  fromKeyToValue: (key: K) => V | undefined;
  fromValueToKey: (value: V) => K | undefined;
  entries: [K, V][];
}

export function createLookupList<K, V>(entries: [K, V][]): LookupList<K, V> {
  const fromKeyToValueMap = new Map(entries);
  const FromValueToKeyMap = new Map(entries.map(([k, v]) => [v, k]));
  return {
    fromKeyToValue(key: K) {
      return fromKeyToValueMap.get(key);
    },
    fromValueToKey(val: V) {
      return FromValueToKeyMap.get(val);
    },
    entries,
  };
}

// Define a custom error class to include the response and status code
export class ResponseError extends Error {
  constructor(
    message: string,
    public response: Response,
  ) {
    super(message);
  }
}

export function throwIfNotOk(response: Response): Response & { ok: true } {
  if (!response.ok) {
    throw new ResponseError(response.statusText, response);
  }
  return response as Response & { ok: true };
}

export function parseJson(response: Response) {
  return response.json();
}

export class FriendlyError extends Error {}
