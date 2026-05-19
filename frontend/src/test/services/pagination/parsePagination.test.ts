import { parsePagination } from "@/services";

import { describe, expect, test } from "vitest";

describe("parsePagination", () => {
  test("should return a paginationobject when used with a synchronous mapper", async () => {
    const pageData = [{ data: "a" }, { data: "b" }];
    const nextUrl = "nextUrl";
    const previousUrl = "previousUrl";

    const result = await parsePagination(
      {
        count: 2,
        next: nextUrl,
        previous: previousUrl,
        results: pageData,
      },
      (obj: unknown): unknown => obj
    );

    expect(result.count).toBe(2);
    expect(result.next).toBe(nextUrl);
    expect(result.previous).toBe(previousUrl);
    expect(result.page).toStrictEqual(pageData);
  });

  test("should return a paginationobject when used with a asynchronous mapper", async () => {
    const pageData = [{ data: "a" }, { data: "b" }];
    const nextUrl = "nextUrl";
    const previousUrl = "previousUrl";

    const result = await parsePagination(
      {
        count: 2,
        next: nextUrl,
        previous: previousUrl,
        results: pageData,
      },
      (obj: unknown): unknown => {
        return Promise.resolve(obj);
      }
    );

    expect(result.count).toBe(2);
    expect(result.next).toBe(nextUrl);
    expect(result.previous).toBe(previousUrl);
    expect(result.page).toStrictEqual(pageData);
  });
});
