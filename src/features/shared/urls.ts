const klantRootUrl = `${window.gatewayBaseUri}/api/klanten`;

export function getKlantIdUrl(id?: string) {
  if (!id) return "";
  const url = new URL(`${klantRootUrl}/${id}`);
  url.searchParams.set("extend[]", "all");
  return url.toString();
}
