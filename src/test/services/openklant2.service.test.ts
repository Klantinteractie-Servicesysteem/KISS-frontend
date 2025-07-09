import { extractOnderwerp } from "@/services/openklant2/service";
import { describe, it, expect } from "vitest";
import type { Vraag } from "@/stores/contactmoment";

describe("extractOnderwerp", () => {
  const makeVraag = (overrides = {}): Vraag => ({
      vraag: { title: "Testvraag", url: "https://example.com" },
      specifiekevraag: "Specifieke vraag",
      zaken: [],
      notitie: "",
      contactverzoek: {
          vragenSets: [],
          vragenSetIdMap: new Map(),
      },
      startdatum: "",
      kanaal: "",
      ...overrides,
      gespreksresultaat: "",
      klanten: [],
      medewerkers: [],
      websites: [],
      kennisartikelen: [],
      nieuwsberichten: [],
      werkinstructies: [],
      vacs: []
  });

  it("returns specifiekevraag if title is 'anders' ", () => {
    const specifiekevraag = "a".repeat(210);
    const vraag = makeVraag({ vraag: { title: "anders" }, specifiekevraag });
    const result = extractOnderwerp(vraag);
    expect(result).toBe(specifiekevraag);
  });

  it("returns title and specifiekevraag if both fit", () => {
    const vraag = makeVraag({ vraag: { title: "Vraag" }, specifiekevraag: "Specifiek" });
    const result = extractOnderwerp(vraag);
    expect(result).toBe("Vraag (Specifiek)");
  });

  it("truncates title if title + specifiekevraag is too long", () => {
    const vraag = makeVraag({ vraag: { title: "a".repeat (190) }, specifiekevraag: "Specifiek" });
    const result = extractOnderwerp(vraag);
    expect(result.length).toBeLessThanOrEqual(200);
    expect(result.endsWith(")"));
    expect(result.includes("..."));
  });

  it("returns only title if only title is present and fits", () => {
    const vraag = makeVraag({ specifiekevraag: undefined });
    const result = extractOnderwerp(vraag);
    expect(result).toBe("Testvraag");
  });

  it("truncates title if only title is present and too long", () => {
    const vraag = makeVraag({ vraag: { title: "a".repeat(210) }, specifiekevraag: undefined });
    const result = extractOnderwerp(vraag);
    expect(result.length).toBeLessThanOrEqual(200);
    expect(result.endsWith("..."));
  });

 
  it("returns empty string if no title or specifiekevraag", () => {
    const vraag = makeVraag({ vraag: { title: undefined }, specifiekevraag: undefined });
    const result = extractOnderwerp(vraag);
    expect(result).toBe("");
  });

  it("truncates vraag so that 'Vraag (Specifieke vraag)' fits 200 chars, with ...", () => {
    const specifiekevraag = "b".repeat(50);
    const vraagTitle = "a".repeat(200);
    const vraag = makeVraag({ vraag: { title: vraagTitle }, specifiekevraag });
    const result = extractOnderwerp(vraag); 
    expect(result.length).toBeLessThanOrEqual(200);
    expect(result.endsWith(` (${specifiekevraag})`)).toBe(true);
    expect(result.includes("..."));
  });

  it("if specifiekevraag is 180 chars, vraag is truncated to 14 chars (with ...), so total is 200", () => {
    const specifiekevraag = "b".repeat(180);
    const vraagTitle = "a".repeat(50);
    const vraag = makeVraag({ vraag: { title: vraagTitle }, specifiekevraag });
    const result = extractOnderwerp(vraag);
    // 14 chars + ... + ' (180 chars)' = 200
    const suffix = ` (${specifiekevraag})`;
    expect(result.length).toBe(200);
    const vraagPart = result.slice(0, result.length - suffix.length);
    expect(vraagPart.endsWith("..."));
    expect(vraagPart.length).toBe(17); // 14 + 3 dots
    expect(result.endsWith(suffix)).toBe(true); 
  });

  it("if specifiekevraag is empty and vraag > 200, vraag is truncated to 197 chars + ...", () => {
    const vraagTitle = "a".repeat(250);
    const vraag = makeVraag({ vraag: { title: vraagTitle }, specifiekevraag: "" });
    const result = extractOnderwerp(vraag);
    expect(result.length).toBe(200);
    expect(result.endsWith("..."));
  });
}); 