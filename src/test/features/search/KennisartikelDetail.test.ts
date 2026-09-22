import { describe, expect, it } from "vitest";
import { mount } from "@vue/test-utils";
import KennisartikelDetail from "@/features/search/KennisartikelDetail.vue";

function makeKennisartikelRaw(vertaling: Record<string, string>) {
  return {
    uuid: "abc-123",
    url: "https://example.com/artikel",
    afdelingen: [],
    vertalingen: [{ taal: "nl", ...vertaling }],
  };
}

function mountDetail(vertaling: Record<string, string>) {
  return mount(KennisartikelDetail, {
    props: {
      kennisartikelRaw: makeKennisartikelRaw(vertaling),
      title: "Bomen kappen",
      headingLevel: 2,
    },
    global: {
      stubs: { ContentFeedback: true },
    },
  });
}

function lastSelectedEvent(wrapper: ReturnType<typeof mountDetail>) {
  const emitted = wrapper.emitted("kennisartikel-selected");
  expect(emitted).toBeTruthy();
  return emitted![emitted!.length - 1][0] as {
    title: string;
    sections: string[];
    sectionIndex: number;
  };
}

describe("KennisartikelDetail", () => {
  it("hides tab navigation and does not postfix Vraag for a single-section article", () => {
    const wrapper = mountDetail({ tekst: "<p>Intro tekst</p>" });

    expect(wrapper.find("nav").exists()).toBe(false);
    expect(lastSelectedEvent(wrapper).sections).toEqual([]);
  });

  it("hides tab navigation and does not postfix Vraag for a single-section article that is not Inleiding", () => {
    // an article with only "Aanvraag" filled in and no "Inleiding"
    const wrapper = mountDetail({ procedureBeschrijving: "<p>Aanvraag tekst</p>" });

    expect(wrapper.find("nav").exists()).toBe(false);
    expect(lastSelectedEvent(wrapper).sections).toEqual([]);
  });

  it("shows tab navigation and includes Inleiding in the postfix sections for a multi-section article", () => {
    const wrapper = mountDetail({
      tekst: "<p>Intro tekst</p>",
      procedureBeschrijving: "<p>Aanvraag tekst</p>",
    });

    expect(wrapper.find("nav").exists()).toBe(true);
    expect(lastSelectedEvent(wrapper).sections).toEqual([
      "Inleiding",
      "Aanvraag",
    ]);
  });

  it("keeps postfixing non-Inleiding tabs as before when Inleiding is selected among multiple tabs", async () => {
    const wrapper = mountDetail({
      tekst: "<p>Intro tekst</p>",
      procedureBeschrijving: "<p>Aanvraag tekst</p>",
      vereisten: "<p>Voorwaarden tekst</p>",
    });

    // Inleiding is active by default (sectionIndex 0)
    expect(lastSelectedEvent(wrapper).sectionIndex).toBe(0);
    expect(lastSelectedEvent(wrapper).sections).toEqual([
      "Inleiding",
      "Aanvraag",
      "Voorwaarden",
    ]);

    // switch to the second tab ("Aanvraag")
    await wrapper.findAll("nav a")[0]!.trigger("click");

    expect(lastSelectedEvent(wrapper).sectionIndex).toBe(1);
    expect(lastSelectedEvent(wrapper).sections).toEqual([
      "Inleiding",
      "Aanvraag",
      "Voorwaarden",
    ]);
  });
});
