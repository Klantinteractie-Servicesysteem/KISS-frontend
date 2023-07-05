import { ensureState } from "@/stores/create-store";
import { mount } from "@vue/test-utils";
import { describe, expect, test } from "vitest";

describe("create-store", () => {
  test("should hold the model provided by a statefactory", () => {
    mount(
      {},
      {
        async mounted() {
          const store = ensureState({
            stateId: "test",
            stateFactory() {
              return {
                x: "test",
              };
            },
          });

          expect(store.value.x).toMatch("test");
        },
      }
    );
  });

  test("should return to the initial state after a reset", () => {
    mount(
      {},
      {
        async mounted() {
          const store = ensureState({
            stateId: "testreset",
            stateFactory() {
              return {
                x: "initial",
              };
            },
          });

          expect(store.value.x).toMatch("initial");

          store.value.x = "test";

          expect(store.value.x).toMatch("test");

          store.reset();

          expect(store.value.x).toMatch("initial");
        },
      }
    );
  });
});
