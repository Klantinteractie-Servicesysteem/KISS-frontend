import { describe, expect, test } from "vitest";
import {
  parseDutchDate,
  parseBsn,
  TELEFOON_PATTERN,
  EMAIL_PATTERN,
} from "@/helpers/validation";

describe("parseDutchDate", () => {
  test("should allow dd/MM/yyyy", async () => {
    const parsed = parseDutchDate("20/10/2000");
    expect(parsed).toBeInstanceOf(Date);
    const geboortedatum = parsed as Date;
    expect(geboortedatum.getDate()).toBe(20);
    expect(geboortedatum.getMonth()).toBe(9);
    expect(geboortedatum.getFullYear()).toBe(2000);
  });
  test("should allow d/M/yyyy", async () => {
    const parsed = parseDutchDate("2/1/2000");
    expect(parsed).toBeInstanceOf(Date);
    const geboortedatum = parsed as Date;
    expect(geboortedatum.getDate()).toBe(2);
    expect(geboortedatum.getMonth()).toBe(0);
    expect(geboortedatum.getFullYear()).toBe(2000);
  });

  test("should allow dd-MM-yyyy", async () => {
    const parsed = parseDutchDate("20-10-2000");
    expect(parsed).toBeInstanceOf(Date);
    const geboortedatum = parsed as Date;
    expect(geboortedatum.getDate()).toBe(20);
    expect(geboortedatum.getMonth()).toBe(9);
    expect(geboortedatum.getFullYear()).toBe(2000);
  });

  test("should allow d-M-yyyy", async () => {
    const parsed = parseDutchDate("2-1-2000");
    expect(parsed).toBeInstanceOf(Date);
    const geboortedatum = parsed as Date;
    expect(geboortedatum.getDate()).toBe(2);
    expect(geboortedatum.getMonth()).toBe(0);
    expect(geboortedatum.getFullYear()).toBe(2000);
  });

  test("should allow ddMMyyyy", async () => {
    const parsed = parseDutchDate("20102000");
    expect(parsed).toBeInstanceOf(Date);
    const geboortedatum = parsed as Date;
    expect(geboortedatum.getDate()).toBe(20);
    expect(geboortedatum.getMonth()).toBe(9);
    expect(geboortedatum.getFullYear()).toBe(2000);
  });

  test("should return an error on invalid date", async () => {
    const parsed = parseDutchDate("50-40-2000");
    expect(parsed).toBeInstanceOf(Error);
    const error = parsed as Error;
    expect(error.message).toBe(
      "Voer een valide datum in, bijvoorbeeld 17-09-2022 of 17092022.",
    );
  });

  test("should return an error on a non-date", async () => {
    const parsed = parseDutchDate("aaa bbb");
    expect(parsed).toBeInstanceOf(Error);
    const error = parsed as Error;
    expect(error.message).toBe(
      "Voer een valide datum in, bijvoorbeeld 17-09-2022 of 17092022.",
    );
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

describe("TELEFOON_PATTERN", () => {
  test("validates phone numbers correctly", () => {
    const testCases = [
      { number: "+31612345678", isValid: true },
      { number: "+441134960000", isValid: true },
      { number: "+12065550100", isValid: true },
      { number: "0612345678", isValid: true },
      { number: "09001234567", isValid: true },
      { number: "1400", isValid: true },
      { number: "14012", isValid: true },
      { number: "14079", isValid: true },
      { number: "0695azerty", isValid: false },
      { number: "azerty0545", isValid: false },
      { number: "@4566544++8", isValid: false },
      { number: "onetwothreefour", isValid: false },
      { number: "020 753 0523", isValid: false },
      { number: "+311234", isValid: false },
      { number: "0800852", isValid: false },
      { number: "080085285212", isValid: false },
    ];

    testCases.forEach(({ number, isValid }) =>
      expect(TELEFOON_PATTERN.test(number)).toBe(isValid),
    );
  });
});

describe("EMAIL_PATTERN", () => {
  test("validates email addresses correctly", () => {
    const testCases = [
      { email: "email@here.com", isValid: true },
      { email: "weirder-email@here.and.there.com", isValid: true },
      { email: "example@valid-----hyphens.com", isValid: true },
      { email: "example@valid-with-hyphens.com", isValid: true },
      { email: `example@atm.${"a".repeat(63)}`, isValid: true },
      { email: `example@${"a".repeat(63)}.atm`, isValid: true },
      {
        email: `example@${"a".repeat(63)}.${"b".repeat(10)}.atm`,
        isValid: true,
      },
      { email: `example@atm.${"a".repeat(64)}`, isValid: false },
      {
        email: `example@${"b".repeat(64)}.atm.${"a".repeat(63)}.atm`,
        isValid: false,
      },
      { email: "", isValid: false },
      { email: "abc", isValid: false },
      { email: "abc@", isValid: false },
      { email: "abc@bar", isValid: false },
      { email: "a @x.cz", isValid: false },
      { email: "abc@.com", isValid: false },
      { email: `"test@test"@example.com`, isValid: false },
      { email: "something@@somewhere.com", isValid: false },
      { email: "email@127.0.0.1", isValid: false },
      { email: "example@invalid-.com", isValid: false },
      { email: "example@-invalid.com", isValid: false },
      { email: "example@invalid.com-", isValid: false },
      { email: "example@inv-.alid-.com", isValid: false },
      { email: "example@inv-.-alid.com", isValid: false },
      { email: `test@example.com\n\n<script src="x.js">`, isValid: false },
      { email: "trailingdot@shouldfail.com.", isValid: false },
      { email: `example@${"a".repeat(63)}.us`, isValid: true },
      { email: `example@${"a".repeat(64)}.us`, isValid: false },
      { email: "a@b.com\n", isValid: false },
      { email: "a\n@b.com", isValid: false },
    ];

    testCases.forEach(({ email, isValid }) =>
      expect(EMAIL_PATTERN.test(email)).toBe(isValid),
    );
  });
});
