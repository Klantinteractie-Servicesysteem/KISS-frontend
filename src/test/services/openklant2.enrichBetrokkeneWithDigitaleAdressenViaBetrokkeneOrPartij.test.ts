import { describe, it, expect, vi, beforeEach } from "vitest";
import { enrichBetrokkeneWithDigitaleAdressenViaBetrokkeneOrPartij } from "@/services/openklant2/service";
import {
  DigitaalAdresTypes,
  type BetrokkeneMetKlantContact,
} from "@/services/openklant2/types";

const jsonResponse = (body: unknown) =>
  new Response(JSON.stringify(body), {
    status: 200,
    headers: { "Content-Type": "application/json" },
  });

const makeBetrokkene = (
  overrides: Partial<BetrokkeneMetKlantContact> = {},
): BetrokkeneMetKlantContact =>
  ({
    uuid: "betrokkene-1",
    digitaleAdressen: [],
    contactnaam: {
      achternaam: "",
      voorletters: "",
      voornaam: "",
      voorvoegselAchternaam: "",
    },
    klantContact: {} as BetrokkeneMetKlantContact["klantContact"],
    ...overrides,
  }) as BetrokkeneMetKlantContact;

describe("enrichBetrokkeneWithDigitaleAdressenViaBetrokkeneOrPartij", () => {
  let fetchMock: ReturnType<typeof vi.fn>;

  beforeEach(() => {
    fetchMock = vi.fn();
    vi.stubGlobal("fetch", fetchMock);
  });

  it("uses the betrokkene's own digitale adres and does not fetch a partij", async () => {
    fetchMock.mockImplementation((url: string) => {
      if (url.includes("/digitaleadressen/eigen-adres")) {
        return Promise.resolve(
          jsonResponse({
            adres: "eigen@example.com",
            soortDigitaalAdres: DigitaalAdresTypes.email,
          }),
        );
      }
      throw new Error(`Unexpected fetch: ${url}`);
    });

    const betrokkene = makeBetrokkene({
      digitaleAdressen: [
        { uuid: "eigen-adres", url: "https://example.com/eigen-adres" },
      ],
      wasPartij: { uuid: "partij-1", url: "https://example.com/partij-1" },
    });

    const [result] = await enrichBetrokkeneWithDigitaleAdressenViaBetrokkeneOrPartij("systeem-1", [
      betrokkene,
    ]);

    expect(result.expandedDigitaleAdressen).toEqual([
      { adres: "eigen@example.com", soortDigitaalAdres: DigitaalAdresTypes.email },
    ]);
    expect(fetchMock).toHaveBeenCalledTimes(1);
  });

  it("falls back to the partij's voorkeursDigitaalAdres when the betrokkene has none", async () => {
    fetchMock.mockImplementation((url: string) => {
      if (url.includes("/partijen/partij-1")) {
        return Promise.resolve(
          jsonResponse({
            uuid: "partij-1",
            voorkeursDigitaalAdres: {
              uuid: "voorkeur-adres",
              url: "https://example.com/voorkeur-adres",
            },
          }),
        );
      }
      if (url.includes("/digitaleadressen/voorkeur-adres")) {
        return Promise.resolve(
          jsonResponse({
            adres: "partij@example.com",
            soortDigitaalAdres: DigitaalAdresTypes.email,
          }),
        );
      }
      throw new Error(`Unexpected fetch: ${url}`);
    });

    const betrokkene = makeBetrokkene({
      digitaleAdressen: [],
      wasPartij: { uuid: "partij-1", url: "https://example.com/partij-1" },
    });

    const [result] = await enrichBetrokkeneWithDigitaleAdressenViaBetrokkeneOrPartij("systeem-1", [
      betrokkene,
    ]);

    expect(result.expandedDigitaleAdressen).toEqual([
      {
        adres: "partij@example.com",
        soortDigitaalAdres: DigitaalAdresTypes.email,
      },
    ]);
  });

  it("resolves to an empty array when the partij has no voorkeursDigitaalAdres", async () => {
    fetchMock.mockImplementation((url: string) => {
      if (url.includes("/partijen/partij-1")) {
        return Promise.resolve(
          jsonResponse({ uuid: "partij-1", voorkeursDigitaalAdres: null }),
        );
      }
      throw new Error(`Unexpected fetch: ${url}`);
    });

    const betrokkene = makeBetrokkene({
      digitaleAdressen: [],
      wasPartij: { uuid: "partij-1", url: "https://example.com/partij-1" },
    });

    const [result] = await enrichBetrokkeneWithDigitaleAdressenViaBetrokkeneOrPartij("systeem-1", [
      betrokkene,
    ]);

    expect(result.expandedDigitaleAdressen).toEqual([]);
  });

  it("resolves to an empty array and fetches nothing when there is no wasPartij", async () => {
    fetchMock.mockImplementation((url: string) => {
      throw new Error(`Unexpected fetch: ${url}`);
    });

    const betrokkene = makeBetrokkene({ digitaleAdressen: [] });

    const [result] = await enrichBetrokkeneWithDigitaleAdressenViaBetrokkeneOrPartij("systeem-1", [
      betrokkene,
    ]);

    expect(result.expandedDigitaleAdressen).toEqual([]);
    expect(fetchMock).not.toHaveBeenCalled();
  });

  it("only fetches a shared partij once for multiple betrokkenen", async () => {
    fetchMock.mockImplementation((url: string) => {
      if (url.includes("/partijen/partij-1")) {
        return Promise.resolve(
          jsonResponse({
            uuid: "partij-1",
            voorkeursDigitaalAdres: {
              uuid: "voorkeur-adres",
              url: "https://example.com/voorkeur-adres",
            },
          }),
        );
      }
      if (url.includes("/digitaleadressen/voorkeur-adres")) {
        return Promise.resolve(
          jsonResponse({
            adres: "partij@example.com",
            soortDigitaalAdres: DigitaalAdresTypes.email,
          }),
        );
      }
      throw new Error(`Unexpected fetch: ${url}`);
    });

    const wasPartij = { uuid: "partij-1", url: "https://example.com/partij-1" };
    const betrokkene1 = makeBetrokkene({
      uuid: "betrokkene-1",
      digitaleAdressen: [],
      wasPartij,
    });
    const betrokkene2 = makeBetrokkene({
      uuid: "betrokkene-2",
      digitaleAdressen: [],
      wasPartij,
    });

    const [result1, result2] = await enrichBetrokkeneWithDigitaleAdressenViaBetrokkeneOrPartij(
      "systeem-1",
      [betrokkene1, betrokkene2],
    );

    expect(result1.expandedDigitaleAdressen).toEqual([
      { adres: "partij@example.com", soortDigitaalAdres: DigitaalAdresTypes.email },
    ]);
    expect(result2.expandedDigitaleAdressen).toEqual([
      { adres: "partij@example.com", soortDigitaalAdres: DigitaalAdresTypes.email },
    ]);
    expect(
      fetchMock.mock.calls.filter(([url]) =>
        (url as string).includes("/partijen/partij-1"),
      ),
    ).toHaveLength(1);
  });
});
