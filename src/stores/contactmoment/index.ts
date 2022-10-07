import type { Klant } from "@/features/klant/types";
import type {
  Medewerker,
  Website,
  Kennisartikel,
  Nieuwsbericht,
  Werkinstructie,
} from "@/features/search/types";
import type { Zaak } from "@/features/zaaksysteem/types";
import { getFormattedUtcDate } from "@/services";
import { defineStore } from "pinia";
import { createSession, type Session } from "../switchable-store";
export * from "./types";

export type ContactmomentZaak = { zaak: Zaak; shouldStore: boolean };
export type ContactmomentContactVerzoek = {
  url: string;
  medewerker: string;
  isInProgress: boolean;
  isSubmitted: boolean;
};

export interface Vraag {
  zaken: ContactmomentZaak[];
  notitie: string;
  contactverzoek: ContactmomentContactVerzoek;
  startdatum: string;
  kanaal: string;
  resultaat: string;
  klanten: { klant: Klant; shouldStore: boolean }[];
  medewerkers: { medewerker: Medewerker; shouldStore: boolean }[];
  websites: { website: Website; shouldStore: boolean }[];
  kennisartikelen: { kennisartikel: Kennisartikel; shouldStore: boolean }[];
  nieuwsberichten: { nieuwsbericht: Nieuwsbericht; shouldStore: boolean }[];
  werkinstructies: { werkinstructie: Werkinstructie; shouldStore: boolean }[];
  primaireVraag: { url: string; title: string } | undefined;
  afwijkendOnderwerp: string;
}

function initVraag(): Vraag {
  return {
    zaken: [],
    notitie: "",
    contactverzoek: {
      url: "",
      medewerker: "",
      isInProgress: false,
      isSubmitted: false,
    },
    startdatum: getFormattedUtcDate(),
    kanaal: "",
    resultaat: "",
    klanten: [],
    medewerkers: [],
    websites: [],
    kennisartikelen: [],
    nieuwsberichten: [],
    werkinstructies: [],
    primaireVraag: undefined,
    afwijkendOnderwerp: "",
  };
}

export interface ContactmomentState {
  vragen: Vraag[];
  huidigeVraag: Vraag;
  session: Session;
  route: string;
}

function initContactmoment(): ContactmomentState {
  const vraag = initVraag();
  return {
    vragen: [vraag],
    huidigeVraag: vraag,
    session: createSession(),
    route: "",
  };
}

interface ContactmomentenState {
  contactmomenten: ContactmomentState[];
  huidigContactmoment: ContactmomentState | undefined;
  contactmomentLoopt: boolean;
}

export const useContactmomentStore = defineStore("contactmoment", {
  state: () => {
    return {
      contactmomentLoopt: false,
      contactmomenten: [],
      huidigContactmoment: undefined,
    } as ContactmomentenState;
  },
  getters: {
    klantVoorHuidigeVraag(state): Klant | undefined {
      return state.huidigContactmoment?.huidigeVraag.klanten
        ?.filter((x) => x.shouldStore)
        ?.map((x) => x.klant)?.[0];
    },
    wouldLoseProgress(): boolean {
      return !!this.huidigContactmoment?.huidigeVraag.contactverzoek
        .isInProgress;
    },
  },
  actions: {
    start() {
      const newMoment = initContactmoment();
      this.contactmomenten.unshift(newMoment);
      this.switchContactmoment(newMoment);
      this.contactmomentLoopt = true;
    },
    switchContactmoment(contactmoment: ContactmomentState) {
      if (!this.contactmomenten.includes(contactmoment)) return;
      if (this.huidigContactmoment) {
        this.huidigContactmoment.huidigeVraag.contactverzoek.isInProgress =
          false;
      }
      this.huidigContactmoment = contactmoment;
      contactmoment.session.enable();
    },
    startNieuweVraag() {
      const nieuweVraag = initVraag();
      const { huidigContactmoment } = this;
      if (!huidigContactmoment) return;

      if (huidigContactmoment.huidigeVraag.klanten) {
        nieuweVraag.klanten = huidigContactmoment.huidigeVraag.klanten.map(
          (klantKoppeling) => ({
            ...klantKoppeling,
          })
        );
      }
      huidigContactmoment.vragen.push(nieuweVraag);
      this.switchVraag(nieuweVraag);
    },
    switchVraag(vraag: Vraag) {
      const { huidigContactmoment } = this;
      if (!huidigContactmoment) return;
      if (!huidigContactmoment.vragen.includes(vraag)) return;

      huidigContactmoment.huidigeVraag.contactverzoek.isInProgress = false;
      huidigContactmoment.huidigeVraag = vraag;
    },
    stop() {
      if (!this.huidigContactmoment) return;
      const currentIndex = this.contactmomenten.indexOf(
        this.huidigContactmoment
      );
      if (currentIndex == -1) return;
      this.contactmomenten.splice(currentIndex, 1);

      if (this.contactmomenten.length) {
        this.switchContactmoment(this.contactmomenten[0]);
        return;
      }

      this.huidigContactmoment = undefined;
      this.contactmomentLoopt = false;
      // start with an empty session. this is equivalent to resetting all state.
      createSession().enable();
    },
    upsertZaak(zaak: Zaak, vraag: Vraag, shouldStore = true) {
      const existingZaak = vraag.zaken.find(
        (contacmomentZaak) => contacmomentZaak.zaak.id === zaak.id
      );

      if (existingZaak) {
        existingZaak.zaak = zaak;
        existingZaak.shouldStore = shouldStore;
        return;
      }

      //als de zaak nog niet gekoppeld was aan het contact moment dan voegen we hem eerst toe
      vraag.zaken.push({
        zaak,
        shouldStore,
      });
    },
    isZaakLinkedToContactmoment(id: string, vraag: Vraag) {
      return vraag.zaken.some(
        ({ zaak, shouldStore }) => shouldStore && zaak.id === id
      );
    },

    setKlant(klant: Klant) {
      const { huidigContactmoment } = this;
      if (!huidigContactmoment) return;
      const { huidigeVraag } = huidigContactmoment;

      const match = huidigeVraag.klanten.find((x) => x.klant.id === klant.id);

      huidigeVraag.klanten.forEach((x) => {
        x.shouldStore = false;
      });

      if (match) {
        match.klant = klant;
        match.shouldStore = true;
        return;
      }

      huidigeVraag.klanten.push({
        shouldStore: true,
        klant,
      });
    },

    addMedewerker(medewerker: any, url: string) {
      const { huidigContactmoment } = this;
      if (!huidigContactmoment) return;
      const { huidigeVraag } = huidigContactmoment;

      const newMedewerkerIndex = huidigeVraag.medewerkers.findIndex(
        (m) => m.medewerker.id === medewerker.id
      );

      if (newMedewerkerIndex === -1) {
        huidigeVraag.medewerkers.push({
          medewerker: {
            id: medewerker.id,
            voornaam: medewerker.contact.voornaam,
            voorvoegselAchternaam: medewerker.contact.voorvoegselAchternaam,
            achternaam: medewerker.contact.achternaam,
            emailadres: medewerker.contact.emails
              ? medewerker.contact.emails[0].email
              : "",
            url,
          },
          shouldStore: true,
        });
      }
    },

    addKennisartikel(kennisartikel: any) {
      const { huidigContactmoment } = this;
      if (!huidigContactmoment) return;
      const { huidigeVraag } = huidigContactmoment;

      const newKennisartikelIndex = huidigeVraag.kennisartikelen.findIndex(
        (k) => k.kennisartikel.url === kennisartikel.url
      );

      if (newKennisartikelIndex === -1) {
        huidigeVraag.kennisartikelen.push({
          kennisartikel: {
            title:
              kennisartikel.vertalingen[0]?.productTitelDecentraal ??
              "Onbekende titel",
            url: kennisartikel.url,
          },
          shouldStore: true,
        });
      }
    },

    addWebsite(website: Website) {
      const { huidigContactmoment } = this;
      if (!huidigContactmoment) return;
      const { huidigeVraag } = huidigContactmoment;

      const newWebsiteIndex = huidigeVraag.websites.findIndex(
        (w) => w.website.url === website.url
      );

      if (newWebsiteIndex === -1) {
        huidigeVraag.websites.push({ website, shouldStore: true });
      }
    },

    toggleNieuwsbericht(nieuwsbericht: Nieuwsbericht) {
      const { huidigContactmoment } = this;
      if (!huidigContactmoment) return;
      const { huidigeVraag } = huidigContactmoment;

      const foundBerichtIndex = huidigeVraag.nieuwsberichten.findIndex(
        (n) => n.nieuwsbericht.url === nieuwsbericht.url
      );

      if (foundBerichtIndex !== -1) {
        huidigeVraag.nieuwsberichten.splice(foundBerichtIndex, 1);
        return;
      }

      huidigeVraag.nieuwsberichten.push({
        nieuwsbericht,
        shouldStore: true,
      });
    },

    toggleWerkinstructie(werkinstructie: Werkinstructie) {
      const { huidigContactmoment } = this;
      if (!huidigContactmoment) return;
      const { huidigeVraag } = huidigContactmoment;

      const foundWerkinstructieIndex = huidigeVraag.werkinstructies.findIndex(
        (w) => w.werkinstructie.url === werkinstructie.url
      );

      if (foundWerkinstructieIndex !== -1) {
        huidigeVraag.werkinstructies.splice(foundWerkinstructieIndex, 1);
        return;
      }

      huidigeVraag.werkinstructies.push({
        werkinstructie,
        shouldStore: true,
      });
    },
  },
});
