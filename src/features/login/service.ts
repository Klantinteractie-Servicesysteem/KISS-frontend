import type { User } from "@/stores/user";

export async function fetchUser(url: string): Promise<User> {
  const response = await fetch(url, {
    credentials: "include",
  });

  if (response.status === 401)
    return {
      isLoggedIn: false,
      email: "",
      isRedacteur: false,
      isKcm: false,
      isSessionExpired: false,
      organisatieIds: [],
    };

  if (!response.ok) {
    throw new Error("unexpected status code: " + response.status);
  }

  const json = await response.json();

  const isLoggedIn = !!json?.isLoggedIn;
  const email = json?.email;
  const isRedacteur = !!json?.isRedacteur;
  const isKcm = !!json?.isKcm;
  const isSessionExpired = false;

  if (isLoggedIn && (typeof email !== "string" || !email))
    throw new Error("user has no emailadress");

  const organisatieIds = Array.isArray(json?.organisatieIds)
    ? json.organisatieIds
    : [];

  return {
    isLoggedIn,
    email,
    isRedacteur,
    organisatieIds,
    isKcm,
    isSessionExpired,
  };
}
