import { mount } from "@vue/test-utils";
import { describe, expect, test, vi } from "vitest";
import type { ContactmomentKlant } from "@/stores/contactmoment";
import KlantDetails from "@/features/klant/klant-details/KlantDetails.vue";

let storedKlant: ContactmomentKlant | undefined;

vi.mock("@/stores/contactmoment", () => ({
  useContactmomentStore: () => ({
    getKlantByInternalId: () => storedKlant,
  }),
}));

const createKlant = (
  emailadressen: string[],
  telefoonnummers: string[],
): ContactmomentKlant => ({
  id: "1",
  internalId: "10",
  emailadressen,
  telefoonnummers,
  hasContactInformation: !!(emailadressen.length || telefoonnummers.length),
});

const mountWith = (klant: ContactmomentKlant | undefined) => {
  storedKlant = klant;
  return mount(KlantDetails, {
    props: { internalKlantId: "10" },
    global: { stubs: { UtrechtHeading: { template: "<h2><slot /></h2>" } } },
  });
};

describe("KlantDetails", () => {
  test("shows contactgegevens when only an e-mailadres is known", () => {
    const wrapper = mountWith(createKlant(["test@example.com"], []));

    expect(wrapper.find("article").exists()).toBe(true);
    expect(wrapper.text()).toContain("E-mailadressen");
    expect(wrapper.text()).toContain("test@example.com");
    expect(wrapper.text()).not.toContain("Telefoonnummers");
  });

  test("shows contactgegevens when only a telefoonnummer is known", () => {
    const wrapper = mountWith(createKlant([], ["0612345678"]));

    expect(wrapper.find("article").exists()).toBe(true);
    expect(wrapper.text()).toContain("Telefoonnummers");
    expect(wrapper.text()).toContain("0612345678");
    expect(wrapper.text()).not.toContain("E-mailadressen");
  });

  test("shows both e-mailadressen and telefoonnummers when both are known", () => {
    const wrapper = mountWith(
      createKlant(["test@example.com"], ["0612345678", "0201234567"]),
    );

    expect(wrapper.text()).toContain("test@example.com");
    expect(wrapper.findAll("li")).toHaveLength(3);
  });

  test("renders nothing when no contactgegevens are known", () => {
    const wrapper = mountWith(createKlant([], []));

    expect(wrapper.find("article").exists()).toBe(false);
  });

  test("renders nothing when the klant is not found", () => {
    const wrapper = mountWith(undefined);

    expect(wrapper.find("article").exists()).toBe(false);
  });
});
