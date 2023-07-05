import { describe, expect, test } from "vitest";

import {
  parseGeslachtsnaamGeboortedatum,
  type GeslachtsnaamGeboortedatum,
  parseBsn,
} from "@/helpers/validation";

describe("parseGeslachtsnaamGeboortedatum", () => {
  test("should allow XXX dd/MM/yyyy", async () => {
    const parsed = parseGeslachtsnaamGeboortedatum("bbb 20/10/2000");
    const geslachtsnaamGeboortedatum = parsed as GeslachtsnaamGeboortedatum;
    expect(geslachtsnaamGeboortedatum.geboortedatum.getDate()).toBe(20);
    expect(geslachtsnaamGeboortedatum.geboortedatum.getMonth()).toBe(9);
    expect(geslachtsnaamGeboortedatum.geboortedatum.getFullYear()).toBe(2000);
    expect(geslachtsnaamGeboortedatum.geslachtsnaam).toBe("bbb");
  });

  test("should allow XXX dd/MM/yyyy", async () => {
    const parsed = parseGeslachtsnaamGeboortedatum("20-10-2000 bbb");
    const geslachtsnaamGeboortedatum = parsed as GeslachtsnaamGeboortedatum;
    expect(geslachtsnaamGeboortedatum.geboortedatum.getDate()).toBe(20);
    expect(geslachtsnaamGeboortedatum.geboortedatum.getMonth()).toBe(9);
    expect(geslachtsnaamGeboortedatum.geboortedatum.getFullYear()).toBe(2000);
    expect(geslachtsnaamGeboortedatum.geslachtsnaam).toBe("bbb");
  });

  //todo: date 50-40-2000 is accepted. regex should be improved
  test("should return an error on invalid date", async () => {
    const parsed = parseGeslachtsnaamGeboortedatum("aaa bbb");
    const geslachtsnaamGeboortedatum = parsed as Error;
    expect(geslachtsnaamGeboortedatum.message).toBe(
      "Voer een valide datum in, bijvoorbeeld 17-09-2022 of 17092022."
    );
  });

  test("should return an error on missing name", async () => {
    const parsed = parseGeslachtsnaamGeboortedatum("20-10-2000");
    const geslachtsnaamGeboortedatum = parsed as Error;
    expect(geslachtsnaamGeboortedatum.message).toBe("Vul een achternaam in");
  });
});

describe("parseBsn", () => {
  test("should allow a valid bsn", async () => {
    const parsed = parseBsn("645791258");
    expect(parsed).toBe("645791258");
  });

  test("should return an error on an invalid bsn", async () => {
    const parsed = parseBsn("123456789");
    const parsedError = parsed as Error;
    expect(parsedError.message).toBe("Dit is geen valide BSN.");
  });

  test("should return an error on an invalid bsn", async () => {
    const parsed = parseBsn("123");
    const parsedError = parsed as Error;
    expect(parsedError.message).toBe("Voer een BSN in van negen cijfers.");
  });
});
