import { useContactmomentStore, type Vraag } from "@/stores/contactmoment";
import { nanoid } from "nanoid";
import {
  CONTACTVERZOEK_GEMAAKT,
  koppelKlant,
  koppelObject,
  koppelZaakContactmoment,
  mapContactverzoekData,
  saveContactmoment,
  saveContactverzoek,
} from "./service";
import type { Contactmoment } from "./types";
import { writeContactmomentDetails } from "./write-contactmoment-details";
import { useOrganisatieIds } from "@/stores/user";

export const useEsuite = () => {
  const contactmomentStore = useContactmomentStore();
  const organisatieIds = useOrganisatieIds();

  const handleSaveVraagSuccess = async (
    gespreksId: string | undefined,
    otherVragen: Vraag[],
  ): Promise<{ errorMessage?: string }> => {
    if (!gespreksId) {
      gespreksId = nanoid();
    }

    const promises = otherVragen.map((x) => saveVraag(x, gespreksId));
    const otherVrageSaveResults = await Promise.all(promises);
    const firstErrorInOtherVragen = otherVrageSaveResults.find(
      (x) => x.errorMessage,
    );

    if (firstErrorInOtherVragen && firstErrorInOtherVragen.errorMessage) {
      return {
        errorMessage: firstErrorInOtherVragen.errorMessage,
      };
    }

    // klaar
    contactmomentStore.stop();

    return {};
  };

  const saveVraag = async (vraag: Vraag, gespreksId?: string) => {
    const contactmoment: Contactmoment = {
      bronorganisatie: organisatieIds.value[0] || "",
      registratiedatum: new Date().toISOString(), // "2023-06-07UTC15:15:48" "YYYY-MM-DDThh:mm[:ss[.uuuuuu]][+HH:MM|-HH:MM|Z]"getFormattedUtcDate(), // todo check of dit nog het juiste format is. lijkt iso te moeten zijn
      kanaal: vraag.kanaal,
      tekst: vraag.notitie,
      onderwerpLinks: [],
      initiatiefnemer: "klant", //enum "gemeente" of "klant"
      vraag: vraag?.vraag?.title,
      specifiekevraag: vraag.specifiekevraag || undefined,
      gespreksresultaat: vraag.gespreksresultaat,
      verantwoordelijkeAfdeling: vraag.afdeling?.naam,
      startdatum: vraag.startdatum,
      // overige velden zijn waarschijnlijk obsolete. nog even laten staan. misschien nog deels breuikbaar voor bv contactverzoek
      gespreksId,
      vorigContactmoment: undefined,
      voorkeurskanaal: "",
      voorkeurstaal: "",
      medewerker: "",
      einddatum: new Date().toISOString(),
    };

    addKennisartikelenToContactmoment(contactmoment, vraag);
    addWebsitesToContactmoment(contactmoment, vraag);
    addMedewerkersToContactmoment(contactmoment, vraag);
    addNieuwsberichtToContactmoment(contactmoment, vraag);
    addWerkinstructiesToContactmoment(contactmoment, vraag);
    addVacToContactmoment(contactmoment, vraag);

    const klantUrl = vraag.klanten
      .filter((x) => x.shouldStore)
      .map((x) => x.klant.url)
      .find(Boolean);

    const isContactverzoek = vraag.gespreksresultaat === CONTACTVERZOEK_GEMAAKT;
    let cvData;
    if (isContactverzoek) {
      cvData = mapContactverzoekData({
        klantUrl,
        data: vraag.contactverzoek,
      });

      Object.assign(contactmoment, cvData);
    }

    const savedContactmomentResult = await saveContactmoment(contactmoment);

    if (
      savedContactmomentResult.errorMessage ||
      !savedContactmomentResult.data
    ) {
      return savedContactmomentResult;
    }

    const savedContactmoment = savedContactmomentResult.data;

    const promises = [
      writeContactmomentDetails(contactmoment, savedContactmoment.url),
      zakenToevoegenAanContactmoment(vraag, savedContactmoment.url),
    ];

    if (isContactverzoek && cvData) {
      promises.push(
        saveContactverzoek({
          data: cvData,
          contactmomentUrl: savedContactmoment.url,
        }),
      );
    }

    promises.push(koppelKlanten(vraag, savedContactmoment.url));

    await Promise.all(promises);

    return savedContactmomentResult;
  };

  return {
    async save() {
      if (!contactmomentStore.huidigContactmoment)
        return {
          errorMessage: "contactmoment niet gestart",
        };

      const { vragen } = contactmomentStore.huidigContactmoment;
      const saveVraagResult = await saveVraag(vragen[0]);

      if (saveVraagResult.errorMessage) {
        return {
          errorMessage: saveVraagResult.errorMessage,
        };
      }

      return handleSaveVraagSuccess(
        saveVraagResult.data?.gespreksId,
        vragen.slice(1),
      );
    },
  };
};

const addKennisartikelenToContactmoment = (
  contactmoment: Contactmoment,
  vraag: Vraag,
) => {
  if (!vraag.kennisartikelen) return;

  vraag.kennisartikelen.forEach((kennisartikel) => {
    if (!kennisartikel.shouldStore) return;

    contactmoment.onderwerpLinks.push(kennisartikel.kennisartikel.url);
  });
};

const addVacToContactmoment = (contactmoment: Contactmoment, vraag: Vraag) => {
  if (!vraag.vacs) return;

  vraag.vacs.forEach((item) => {
    if (!item.shouldStore) return;

    contactmoment.onderwerpLinks.push(item.vac.url);
  });
};

const addWebsitesToContactmoment = (
  contactmoment: Contactmoment,
  vraag: Vraag,
) => {
  if (!vraag.websites) return;

  vraag.websites.forEach((website) => {
    if (!website.shouldStore) return;

    contactmoment.onderwerpLinks.push(website.website.url);
  });
};

const addMedewerkersToContactmoment = (
  contactmoment: Contactmoment,
  vraag: Vraag,
) => {
  if (!vraag.medewerkers) return;

  vraag.medewerkers.forEach((medewerker) => {
    if (!medewerker.shouldStore || !medewerker.medewerker.url) return;

    contactmoment.onderwerpLinks.push(medewerker.medewerker.url);
  });
};

const addNieuwsberichtToContactmoment = (
  contactmoment: Contactmoment,
  vraag: Vraag,
) => {
  if (!vraag.nieuwsberichten) return;

  vraag.nieuwsberichten.forEach((nieuwsbericht) => {
    if (!nieuwsbericht.shouldStore) return;

    // make absolute if not already
    const absoluteUrl = new URL(
      nieuwsbericht.nieuwsbericht.url,
      window.location.origin,
    );

    contactmoment.onderwerpLinks.push(absoluteUrl.toString());
  });
};

const addWerkinstructiesToContactmoment = (
  contactmoment: Contactmoment,
  vraag: Vraag,
) => {
  if (!vraag.werkinstructies) return;

  vraag.werkinstructies.forEach((werkinstructie) => {
    if (!werkinstructie.shouldStore) return;

    // make absolute if not already
    const absoluteUrl = new URL(
      werkinstructie.werkinstructie.url,
      window.location.origin,
    );

    contactmoment.onderwerpLinks.push(absoluteUrl.toString());
  });
};

const zakenToevoegenAanContactmoment = async (
  vraag: Vraag,
  contactmomentId: string,
) => {
  for (const { zaak, shouldStore } of vraag.zaken) {
    if (shouldStore) {
      try {
        // dit is voorlopige, hopelijk tijdelijke, code om uit te proberen of dit een nuttige manier is om met de instabiliteit van openzaak en openklant om te gaan
        // derhalve bewust nog niet geoptimaliseerd
        try {
          await koppelZaakContactmoment({
            contactmoment: contactmomentId,
            zaak: zaak.self,
          });
        } catch (e) {
          try {
            console.log(
              "koppelZaakContactmoment in openzaak attempt 1 failed",
              e,
            );
            await koppelZaakContactmoment({
              contactmoment: contactmomentId,
              zaak: zaak.self,
            });
          } catch (e) {
            try {
              console.log(
                "koppelZaakContactmoment in openzaak attempt 2 failed",
                e,
              );
              await koppelZaakContactmoment({
                contactmoment: contactmomentId,
                zaak: zaak.self,
              });
            } catch (e) {
              console.log(
                "koppelZaakContactmoment in openzaak attempt 3 failed",
                e,
              );
            }
          }
        }

        // de tweede call gaat vaak mis, maar geeft dan bijna altijd ten onterechte een error response.
        // de data is dan wel correct opgeslagen
        // wellicht een timing issue. voor de zekerheid even wachten

        try {
          setTimeout(
            async () =>
              await koppelObject({
                contactmoment: contactmomentId,
                object: zaak.self,
                objectType: "zaak",
              }),
            1000,
          );
        } catch (e) {
          console.log("koppelZaakContactmoment in openklant", e);
        }
      } catch (e) {
        // zaken toevoegen aan een contactmoment en anedrsom retourneert soms een error terwijl de data meetal wel correct opgelsagen is.
        // toch maar verder gaan dus
        console.error(e);
      }
    }
  }
};

const koppelKlanten = async (vraag: Vraag, contactmomentId: string) => {
  for (const { shouldStore, klant } of vraag.klanten) {
    if (shouldStore && klant.url) {
      await koppelKlant({ contactmomentId, klantId: klant.url });
    }
  }
};
