import type { User, UserPermissions } from "@/stores/user";
import { meUrl } from "./config";

function transformPermissions(permissionsArray: string[]): UserPermissions {
  return {
    beheer: permissionsArray.includes("Beheer"),
    skills: permissionsArray.includes("Skills"),
  };
}

export async function fetchUser(): Promise<User> {
  const response = await fetch(meUrl, {
    credentials: "include",
  });

  if (response.status === 401)
    return {
      isLoggedIn: false,
      email: "",
      isRedacteur: false,
      isBeheerder: false,
      isKcm: false,
      isKennisbank: false,
      isSessionExpired: false,
      organisatieIds: [],
      permissions: transformPermissions([]),
    };

  if (!response.ok) {
    throw new Error("unexpected status code: " + response.status);
  }

  const json = await response.json();

  const isLoggedIn = !!json?.isLoggedIn;
  const email = json?.email;
  const isRedacteur = !!json?.isRedacteur;
  const isBeheerder = !!json?.isBeheerder;
  const isKcm = !!json?.isKcm;
  const isKennisbank = !!json?.isKennisbank;
  const isSessionExpired = false;

  if (isLoggedIn && (typeof email !== "string" || !email))
    throw new Error("user has no emailadress");

  const organisatieIds = Array.isArray(json?.organisatieIds)
    ? json.organisatieIds
    : [];

  const permissions = transformPermissions(
    Array.isArray(json?.permissions) ? json.permissions : [],
  );

  return {
    isLoggedIn,
    email,
    isRedacteur,
    isBeheerder,
    organisatieIds,
    isKcm,
    isKennisbank,
    isSessionExpired,
    permissions,
  };
}
