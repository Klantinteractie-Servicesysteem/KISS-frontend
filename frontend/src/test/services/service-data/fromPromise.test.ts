import { flushPromises } from "@vue/test-utils";
import { ServiceResult, type ServiceData } from "@/services";

import { describe, expect, expectTypeOf, test } from "vitest";

describe("ServiceResult.fromPromise", () => {
  test("should return a ServiceData instance and put in in a success state after a promise is resolved.", async () => {
    type TestType = {
      data: string;
    };

    const fromPromiseResult = ServiceResult.fromPromise<TestType>(
      Promise.resolve({ data: "value" } as TestType),
    );

    expectTypeOf(fromPromiseResult).toMatchTypeOf<ServiceData<TestType>>();

    await flushPromises();

    expect(fromPromiseResult.state).toMatch("success");
    expect(fromPromiseResult.success).toBeTruthy();
  });

  test("should return a ServiceData instance and put in in a loading state when a promise is pending.", async () => {
    type TestType = {
      data: string;
    };

    const fromPromiseResult = ServiceResult.fromPromise<TestType>(
      Promise.resolve({ data: "value" } as TestType),
    );

    expectTypeOf(fromPromiseResult).toMatchTypeOf<ServiceData<TestType>>();

    expect(fromPromiseResult.state).toMatch("loading");
    expect(fromPromiseResult.success).toBeFalsy();
  });

  test("should return a ServiceData instance and put in in a error state after a promise is resolved.", async () => {
    type TestType = {
      data: string;
    };

    let x;
    const y = ServiceResult.fromPromise<TestType>(
      (x = Promise.reject<TestType>()),
    );

    await flushPromises();

    x.catch(async () => {
      expect(y.state).toMatch("error");
      expect(y.success).toBeFalsy();
    });
  });
});
