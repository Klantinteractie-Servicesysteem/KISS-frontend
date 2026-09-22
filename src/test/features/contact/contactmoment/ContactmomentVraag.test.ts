import { describe, expect, it } from "vitest";
import { mount } from "@vue/test-utils";
import { nextTick } from "vue";
import ContactmomentVraag from "@/features/contact/contactmoment/ContactmomentVraag.vue";
import type { Vraag, Bron } from "@/stores/contactmoment";
import type { Kennisartikel } from "@/features/search/types";

function makeVraag(kennisartikel: Kennisartikel): Vraag {
  return {
    websites: [],
    kennisartikelen: [{ kennisartikel: kennisartikel as Bron, shouldStore: true }],
    nieuwsberichten: [],
    werkinstructies: [],
    vacs: [],
    vraag: kennisartikel as Bron,
  } as unknown as Vraag;
}

function optionTitles(wrapper: ReturnType<typeof mount>) {
  return wrapper.findAll("option").map((o) => o.text());
}

function selectedOptionTitle(wrapper: ReturnType<typeof mount>) {
  const selected = wrapper
    .findAll("option")
    .find((o) => (o.element as HTMLOptionElement).selected);
  return selected?.text();
}

describe("ContactmomentVraag", () => {
  it("display only the plain title, single-section article", async () => {
    const kennisartikel: Kennisartikel = {
      title: "Bomen kappen",
      url: "https://example.com/bomen-kappen",
      sections: [],
      sectionIndex: 0,
    };

    const wrapper = mount(ContactmomentVraag, {
      props: { modelValue: undefined, idx: 0, vraag: makeVraag(kennisartikel) },
    });
    await nextTick();

    expect(optionTitles(wrapper)).toEqual(["Bomen kappen", "Anders"]);
  });

  it("display postfixed option per section, including Inleiding, multi-section article", async () => {
    const kennisartikel: Kennisartikel = {
      title: "Bomen kappen",
      url: "https://example.com/bomen-kappen",
      sections: ["Inleiding", "Aanvraag", "Voorwaarden"],
      sectionIndex: 2,
    };

    const wrapper = mount(ContactmomentVraag, {
      props: { modelValue: undefined, idx: 0, vraag: makeVraag(kennisartikel) },
    });
    await nextTick();

    expect(optionTitles(wrapper)).toEqual([
      "Bomen kappen - Inleiding",
      "Bomen kappen - Aanvraag",
      "Bomen kappen - Voorwaarden",
      "Anders",
    ]);

    // check matching article's current sectionIndex (2 = Voorwaarden)
    expect(selectedOptionTitle(wrapper)).toBe("Bomen kappen - Voorwaarden");
  });

  it("preselects the Inleiding postfix when sectionIndex points at Inleiding in a multi-section article", async () => {
    const kennisartikel: Kennisartikel = {
      title: "Bomen kappen",
      url: "https://example.com/bomen-kappen",
      sections: ["Inleiding", "Aanvraag"],
      sectionIndex: 0,
    };

    const wrapper = mount(ContactmomentVraag, {
      props: { modelValue: undefined, idx: 0, vraag: makeVraag(kennisartikel) },
    });
    await nextTick();

    expect(selectedOptionTitle(wrapper)).toBe("Bomen kappen - Inleiding");
  });
});
