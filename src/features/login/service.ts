import { ServiceResult } from "@/services";
import type { User } from "@/stores/user";
import { meUrl } from "./config";

const anonymousUser = Object.freeze({
  isLoggedIn: false,
});

async function fetchUser(url: string): Promise<User> {
  const response = await fetch(url, {
    credentials: "include",
  });

  if (response.status === 401) return anonymousUser;

  if (!response.ok) {
    throw new Error("unexpected status code: " + response.status);
  }

  const json = await response.json();

  const isLoggedIn = !!json?.isLoggedIn;
  const email = json?.email;
  const isRedacteur = !!json?.isRedacteur;
  const isKcm = !!json?.isKcm;

  if (!isLoggedIn || typeof email !== "string" || !email) return anonymousUser;

  const organisatieIds = Array.isArray(json?.organisatieIds)
    ? json.organisatieIds
    : [];

  return {
    isLoggedIn,
    email,
    isRedacteur,
    organisatieIds,
    isKcm,
  };
}

export const useCurrentUser = () => ServiceResult.fromFetcher(meUrl, fetchUser);
