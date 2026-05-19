import { flushPromises, mount } from "@vue/test-utils";
import { ServiceResult, type ServiceData } from "@/services";

import { describe, expect, expectTypeOf, test } from "vitest";

describe("ServiceResult.fromFetcher", () => {
  test("should return a ServiceData instance and put in in a success state after a promise is resolved.", async () => {
    type TestType = {
      data: string;
    };

    //de te testen code is gewrapped in een leeg vue component en wordt aangeroepen vanuit de mounted lifecycle hook
    //de reden hiervoor is dat fromFetcher gebruik maakt van een SWRV caching lib, die allaen correct functioneert vanuit de context van een vue component
    mount(
      {},
      {
        async mounted() {
          const fromPromiseResult = ServiceResult.fromFetcher<TestType>(
            "dummyurl",
            () => Promise.resolve({ data: "value" } as TestType)
          );

          expectTypeOf(fromPromiseResult).toMatchTypeOf<
            ServiceData<TestType>
          >();

          await flushPromises();

          expect(fromPromiseResult.state).toMatch("success");
          expect(fromPromiseResult.success).toBeTruthy();
        },
      }
    );
  });

  //nb. testen met een rejected of pending promise kan niet stabiel icm useSWRV
});
