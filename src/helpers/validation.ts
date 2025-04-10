import type { Directive, FunctionDirective } from "vue";
import { watch } from "vue";

function isValidPhoneNumber(val: string) {
  if (!val) return true; // empty is allowed, handeld by required attribute
  const numberCount = (val.match(/[0-9]/g) ?? []).length;
  const hasOnlyAllowedChars = /^(\+|-| |[0-9])*$/.test(val);
  return numberCount >= 10 && hasOnlyAllowedChars;
}

export function customPhoneValidator(value: string): string[] {
  return isValidPhoneNumber(value)
    ? []
    : [`'${value}' lijkt geen valide telefoonnummer.`];
}
interface Postcode {
  numbers: string;
  digits: string;
}

export interface PostcodeHuisnummer {
  postcode: Postcode;
  huisnummer: string;
  toevoeging?: string;
  huisletter?: string;
}

export interface GeslachtsnaamGeboortedatum {
  geslachtsnaam: string;
  geboortedatum: Date;
}

export function parsePostcode(input: string): Postcode | Error {
  const matches = input.trim().match(/^([1-9]\d{3}) *([A-Za-z]{2})$/);
  if (matches?.length !== 3) {
    return new Error("Voer een valide postcode in, bijvoorbeeld 1234 AZ.");
  }
  return {
    numbers: matches[1],
    digits: matches[2],
  };
}

const dateRegex =
  /((?<day1>[0-9]{2})(?<month1>[0-9]{2})(?<year1>[0-9]{4}))|((?<day2>[0-9]{1,2})[/|-](?<month2>[0-9]{1,2})[/|-](?<year2>[0-9]{4}))/;

const matchDutchDate = (input: string) => {
  const trimmed = input?.trim();
  const matches = trimmed.match(dateRegex);

  const { year1, month1, day1, year2, month2, day2 } = matches?.groups ?? {};
  const year = year1 || year2;
  const month = month1 || month2;
  const day = day1 || day2;

  if (year && month && day) {
    const correctedMonth = +month - 1;
    const intYear = +year;
    const intDay = +day;
    const date = new Date(intYear, correctedMonth, intDay);
    if (
      date.getFullYear() === intYear &&
      date.getMonth() === correctedMonth &&
      date.getDate() === intDay
    )
      return {
        date,
        matchedString: matches?.[0] ?? "",
      };
  }

  return new Error(
    "Voer een valide datum in, bijvoorbeeld 17-09-2022 of 17092022.",
  );
};

export function parseDutchDate(input: string): Date | Error {
  const result = matchDutchDate(input);
  return result instanceof Error ? result : result.date;
}

const multipliers = [9, 8, 7, 6, 5, 4, 3, 2, -1] as const;

function elfProef(numbers: number[]): boolean {
  if (numbers.length !== 9) return false;
  const multipliedSum = numbers.reduce(
    (previousValue, currentValue, currentIndex) =>
      previousValue + currentValue * multipliers[currentIndex],
    0,
  );
  return multipliedSum % 11 === 0;
}

export function parseBedrijfsnaam(input: string): string | Error {
  if (!input) return input;
  const matches = input.trim().match(/^([a-zA-Z0-9À-ž .\-']{2,199})$/);
  return !matches || matches.length < 2
    ? new Error(
        "Vul een geldig (begin van een) bedrijfsnaam in, van minimaal 2 tekens",
      )
    : matches[1];
}

export function parseBsn(input: string): string | Error {
  if (!input) return input;
  const matches = input.trim().match(/^(\d{9})$/);
  if (!matches || matches.length < 2)
    return new Error("Voer een BSN in van negen cijfers.");
  const match = matches[1];
  const numbers = match.split("").map((char) => +char);
  return elfProef(numbers) ? match : new Error("Dit is geen valide BSN.");
}

export function parseKkvkOfVestigingsnummer(input: string): string | Error {
  const matches = input.trim().match(/^(\d{8}|\d{12})$/);
  return !matches || matches.length < 2
    ? new Error(
        "Vul de 8 cijfers van het KvK-nummer in, bijvoorbeeld 12345678, of vul de 12 cijfers van het Vestigingsnummer in, bv. 123456789012",
      )
    : matches[1];
}

export function parseAchternaam(input: string): string | Error {
  if (!input) return input;
  const matches = input.trim().match(/^([a-zA-Z0-9À-ž .\-']{3,199})$/);
  return !matches || matches.length < 2
    ? new Error(
        "Vul een geldig (begin van een) achternaam in, van minimaal 3 tekens",
      )
    : matches[1];
}

export function parseHuisletter(input: string): string | Error {
  if (!input) return input;
  const matches = input.trim().match(/^([a-zA-Z]{1})$/);
  return !matches || matches.length < 2
    ? new Error("Vul een geldige huisletter in")
    : matches[1];
}

export function parseToevoeging(input: string): string | Error {
  if (!input) return input;
  const matches = input.trim().match(/^([a-zA-Z0-9 -]{1,4})$/);
  return !matches || matches.length < 2
    ? new Error("Vul een geldige toevoeging in, van maximaal 4 karakters")
    : matches[1];
}

export function parseHuisnummer(input: string): string | Error {
  if (!input) return input;
  const matches = input.trim().match(/^[1-9][0-9]{0,4}$/);
  return !matches || !matches.length
    ? new Error("Vul een geldig huisnummer in")
    : matches[0];
}

export type Validator<T> = (v: string) => T | Error;
export type ValidatorSetup<T = any> = {
  validator: Validator<T>;
  current: string;
  validated: T | undefined;
};

export function validateWith<T>(validator: Validator<T>): ValidatorSetup<T> {
  return {
    validator,
    current: "",
    validated: undefined,
  };
}

type InputElementWithSetupRemoval = HTMLInputElement & {
  removeValidatorSetup?: () => void;
};

type ValidatorSetupWithElement = ValidatorSetup & {
  el?: InputElementWithSetupRemoval;
};

const setupValidation: FunctionDirective<
  InputElementWithSetupRemoval,
  ValidatorSetupWithElement
> = (el, { value, oldValue }) => {
  el.removeValidatorSetup?.();
  oldValue?.el?.removeValidatorSetup?.();
  const unwatch = watch(
    () => value.current,
    (i) => {
      el.value = i;
      const parsed = value.validator(i);
      if (parsed instanceof Error) {
        value.validated = undefined;
        el.setCustomValidity(parsed.message);
      } else {
        el.setCustomValidity("");
        value.validated = parsed;
      }
    },
    { immediate: true },
  );
  function onInput() {
    value.current = el.value;
  }
  el.addEventListener("input", onInput);
  el.removeValidatorSetup = () => {
    el.removeEventListener("input", onInput);
    unwatch();
    el.removeValidatorSetup = undefined;
  };
  value.el = el;
};

export const vValidate: Directive<HTMLInputElement, ValidatorSetup> = {
  updated: setupValidation,
  mounted: setupValidation,
  unmounted(el) {
    (el as any).removeValidatorSetup?.();
  },
};

// https://github.com/maykinmedia/open-klant/blob/f231f368c48276ffe429fb7e3105b0ce9f0eb444/src/openklant/utils/validators.py#L26
export const TELEFOON_PATTERN =
  /^(0[8-9]00[0-9]{4,7}|0[1-9][0-9]{8}|\+[0-9]{9,20}|1400|140[0-9]{2,3})$/;

// https://github.com/django/django/blob/4.2/django/core/validators.py#L174
export const EMAIL_PATTERN = new RegExp(
  "^" +
    // user_regex (without quoted string)
    "([-!#$%&'*+/=?^_`{}|~0-9A-Z]+(\\.[-!#$%&'*+/=?^_`{}|~0-9A-Z]+)*)" +
    "@" +
    // domain_regex (without literal_regex)
    "((?:[A-Z0-9](?:[A-Z0-9-]{0,61}[A-Z0-9])?\\.)+(?:[A-Z0-9-]{2,63}(?<!-)))" +
    "$",
  "i",
);
