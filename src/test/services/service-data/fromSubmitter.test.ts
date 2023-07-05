import { flushPromises, mount } from "@vue/test-utils";
import { ServiceResult, type ServiceData } from "@/services";
import { describe, expect, expectTypeOf, test } from "vitest";

describe("ServiceResult.fromSubmitter", () => {
  test("should return a ServiceData instance and put in in a success state after a promise is resolved.", async () => {
    type TestType = {
      data: string;
    };

    const fromSubmitterResult = ServiceResult.fromSubmitter<TestType, TestType>(
      () => Promise.resolve({ data: "out" } as TestType)
    );

    fromSubmitterResult.submit({ data: "in" });

    expectTypeOf(fromSubmitterResult).toMatchTypeOf<ServiceData<TestType>>();

    await flushPromises();

    expect(fromSubmitterResult.state).toMatch("success");
    expect(fromSubmitterResult.success).toBeTruthy();
    if (fromSubmitterResult.success) {
      expect((fromSubmitterResult.data as TestType).data).toMatch("out");
    }
  });

  test("should return a ServiceData instance and put in in a loading state when a promise is pending.", async () => {
    type TestType = {
      data: string;
    };

    const fromPromiseResult = ServiceResult.fromSubmitter<TestType, TestType>(
      () => Promise.resolve({ data: "value" } as TestType)
    );

    fromPromiseResult.submit({ data: "in" });

    expectTypeOf(fromPromiseResult).toMatchTypeOf<ServiceData<TestType>>();

    expect(fromPromiseResult.state).toMatch("loading");
    expect(fromPromiseResult.success).toBeFalsy();
  });

  test("should return a ServiceData instance and put in in a error state after a promise is resolved.", async () => {
    type TestType = {
      data: string;
    };

    //de te testen code is gewrapped in een leeg vue component en wordt aangeroepen vanuit de mounted lifecycle hook
    //testen van rejected promises werkt dan beter
    mount(
      {},
      {
        async mounted() {
          const fromPromiseResult = ServiceResult.fromSubmitter<
            TestType,
            TestType
          >(() => Promise.reject<TestType>());

          expect(fromPromiseResult.submit({ data: "in" })).rejects.toBeFalsy();

          await flushPromises();

          expect(fromPromiseResult.state).toMatch("error");
          expect(fromPromiseResult.success).toBeFalsy();
        },
      }
    );
  });
});
