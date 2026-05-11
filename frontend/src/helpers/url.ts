export function toRelativeProxyUrl(
  absoluteUrl: string,
  broxyBasePath: string
): string | null {
  try {
    return `${broxyBasePath}${new URL(absoluteUrl).pathname}`;
  } catch {
    return null;
  }
}
