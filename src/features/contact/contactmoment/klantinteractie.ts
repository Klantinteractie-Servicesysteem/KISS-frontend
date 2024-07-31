import { klantinteractieClient } from "@/features/klant/klantinteractie-client";
import type { Medewerker } from "@/features/search/types";
import {
  useContactmomentStore,
  type Bron,
  type Vraag,
} from "@/stores/contactmoment";
import type { Klantcontact } from "generated/klantinteracties";
import { writeContactmomentDetails } from "./write-contactmoment-details";

export const useKlantinteractie = () => {
  const contactmomentStore = useContactmomentStore();
  return {
    async save() {
      if (!contactmomentStore.huidigContactmoment)
        return {
          errorMessage: "contactmoment niet gestart",
        };

      const { vragen } = contactmomentStore.huidigContactmoment;
      await Promise.all(vragen.map(saveVraag));
      // klaar
      contactmomentStore.stop();
      return {};
    },
  };
};

const saveVraag = async (vraag: Vraag) => {
  const contact =
    await klantinteractieClient.klantenContacten.klantcontactenCreate({
      uuid: "",
      url: "",
      kanaal: vraag.kanaal,
      onderwerp: vraag.specifiekevraag || vraag.vraag?.title || "",
      taal: "nld",
      vertrouwelijk: false,
      inhoud: vraag.notitie,
      plaatsgevondenOp: vraag.startdatum,
      gingOverOnderwerpobjecten: [],
      hadBetrokkenActoren: [],
      omvatteBijlagen: [],
      hadBetrokkenen: [],
      leiddeTotInterneTaken: [],
    });

  const promises: Promise<unknown>[] = [
    writeContactmomentDetails(
      {
        startdatum: vraag.startdatum,
        einddatum: new Date().toISOString(),
        gespreksresultaat: vraag.gespreksresultaat,
        vraag: vraag.vraag?.title,
        specifiekevraag: vraag.specifiekevraag,
        verantwoordelijkeAfdeling: vraag.afdeling?.naam,
      },
      contact.url,
    ),
  ];

  promises.push(
    ...getOnderwerpen(vraag).map(([onderwerp, type, register]) =>
      saveOnderwerp(onderwerp, type, register, contact),
    ),
  );

  return Promise.all(promises);
};

const getOnderwerpen = (vraag: Vraag) => {
  const kennisartikelen = vraag.kennisartikelen
    .filter(({ shouldStore }) => shouldStore)
    .map(
      ({ kennisartikel }) => [kennisartikel, "Kennisartikel", "PDC"] as const,
    );

  const nieuwsberichten = vraag.nieuwsberichten
    .filter(({ shouldStore }) => shouldStore)
    .map(
      ({ nieuwsbericht }) => [nieuwsbericht, "Nieuwsbericht", "KISS"] as const,
    );

  const medewerkers = vraag.medewerkers
    .filter(({ shouldStore }) => shouldStore)
    .map(
      ({ medewerker }) => [medewerker, "Medewerker", "Smoelenboek"] as const,
    );

  const vacs = vraag.vacs
    .filter(({ shouldStore }) => shouldStore)
    .map(({ vac }) => [vac, "VAC", "PDC"] as const);

  const websites = vraag.websites
    .filter(({ shouldStore }) => shouldStore)
    .map(({ website }) => [website, "Website", "Website"] as const);

  const werkinstructies = vraag.werkinstructies
    .filter(({ shouldStore }) => shouldStore)
    .map(
      ({ werkinstructie }) =>
        [werkinstructie, "Werkinstructie", "KISS"] as const,
    );

  return [
    ...kennisartikelen,
    ...nieuwsberichten,
    ...medewerkers,
    ...vacs,
    ...websites,
    ...werkinstructies,
  ];
};

const saveOnderwerp = async (
  onderwerp: Bron | Medewerker,
  type: string,
  register: string,
  contact: Klantcontact,
) => {
  return klantinteractieClient.onderwerpobjecten.onderwerpobjectenCreate({
    uuid: "",
    url: "",
    wasKlantcontact: null,
    klantcontact: {
      uuid: contact.uuid,
      url: contact.url,
    },
    onderwerpobjectidentificator: {
      objectId: onderwerp.url,
      codeObjecttype: type,
      codeRegister: register,
      codeSoortObjectId: "URL",
    },
  });
};
