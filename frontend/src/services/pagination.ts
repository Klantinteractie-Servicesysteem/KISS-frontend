import type { PaginatedResult } from "./zgw";

export interface Paginated<T> {
  pageSize?: number;
  pageNumber: number;
  totalPages: number;
  totalRecords?: number;
  page: T[];
}

export function defaultPagination<T>(page: T[]): Paginated<T> {
  return {
    page,
    pageNumber: 1,
    totalPages: 1,
    totalRecords: page.length,
    pageSize: page.length,
  };
}

export function enforceOneOrZero<T>(
  paginated:
    | Paginated<NonNullable<T>>
    | PaginatedResult<NonNullable<T>>
    | NonNullable<T>[],
): T | null {
  const page = Array.isArray(paginated) ? paginated : paginated.page;
  if (page.length === 0) return null;
  if (page.length === 1) return page[0];
  throw new Error("expected a single result, instead found " + page.length);
}
