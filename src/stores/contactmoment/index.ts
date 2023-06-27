import type {
  Medewerker,
  Website,
  Kennisartikel,
  Nieuwsbericht,
  Werkinstructie,
} from "@/features/search/types";
import type { ZaakDetails } from "@/features/zaaksysteem/types";
import { defineStore } from "pinia";
import { createSession, type Session } from "../switchable-store";
export * from "./types";

export type ContactmomentZaak = { zaak: ZaakDetails; shouldStore: boolean };

export type ContactmomentContactVerzoek = {
  url: string;
  medewerker: string;
  afdeling: string;
  notitie: string;
  isActive: boolean;
};

export type ContactmomentKlant = {
  id: string;
  voornaam: string;
  voorvoegselAchternaam?: string;
  achternaam: string;
  bedrijfsnaam?: string;
  telefoonnummer?: string;
  emailadres?: string;
  hasContactInformation: boolean;
  url?: string;
};

export interface Vraag {
  zaken: ContactmomentZaak[];
  notitie: string;
  contactverzoek: ContactmomentContactVerzoek;
  startdatum: string;
  kanaal: string;
  resultaat: string;
  klanten: { klant: ContactmomentKlant; shouldStore: boolean }[];
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
      afdeling: "",
      notitie: "",
      isActive: false,
    },
    startdatum: new Date().toISOString(),
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
    klantVoorHuidigeVraag(state): ContactmomentKlant | undefined {
      return state.huidigContactmoment?.huidigeVraag.klanten
        ?.filter((x) => x.shouldStore)
        ?.map((x) => x.klant)?.[0];
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
    upsertZaak(zaak: ZaakDetails, vraag: Vraag, shouldStore = true) {
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

    setKlant(klant: ContactmomentKlant) {
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

    setKlantHasContactgegevens(klantId: string) {
      const { huidigContactmoment } = this;

      if (!huidigContactmoment) return;

      const { huidigeVraag } = huidigContactmoment;

      const targetKlantIndex = huidigeVraag.klanten.findIndex(
        (k) => k.klant.id === klantId
      );

      if (targetKlantIndex === -1) return;

      huidigeVraag.klanten[targetKlantIndex].klant.hasContactInformation = true;
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

    addKennisartikel(kennisartikel: Kennisartikel) {
      const { huidigContactmoment } = this;
      if (!huidigContactmoment) return;
      const { huidigeVraag } = huidigContactmoment;

      const record = huidigeVraag.kennisartikelen.find(
        (k) => k.kennisartikel.url === kennisartikel.url
      );

      if (!record) {
        huidigeVraag.kennisartikelen.push({
          kennisartikel,
          shouldStore: true,
        });
      } else {
        record.kennisartikel = kennisartikel;
      }

      huidigeVraag.primaireVraag = kennisartikel;
    },

    addWebsite(website: Website) {
      const { huidigContactmoment } = this;
      if (!huidigContactmoment) return;
      const { huidigeVraag } = huidigContactmoment;

      const record = huidigeVraag.websites.find(
        (w) => w.website.url === website.url
      );

      if (!record) {
        huidigeVraag.websites.push({ website, shouldStore: true });
      } else {
        record.website = website;
      }

      huidigeVraag.primaireVraag = website;
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

      huidigeVraag.primaireVraag = nieuwsbericht;
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

      huidigeVraag.primaireVraag = werkinstructie;
    },

    updateContactverzoek(contactverzoek: ContactmomentContactVerzoek) {
      const { huidigContactmoment } = this;

      if (!huidigContactmoment) return;

      huidigContactmoment.huidigeVraag.contactverzoek = {
        ...contactverzoek,
        isActive: true,
      };
    },

    removeVraag(vraagIndex: number) {
      const { huidigContactmoment } = this;

      if (!huidigContactmoment) return;

      huidigContactmoment.vragen.splice(vraagIndex, 1);
    },
  },
});
