<template>
  <prompt-modal
    :dialog="cancelDialog"
    message="Weet je zeker dat je het contactmoment wilt annuleren? Alle gegevens worden verwijderd."
    cancel-message="Nee"
    confirm-message="Ja"
  />

  <prompt-modal
    :dialog="removeVraagDialog"
    message="Weet je zeker dat je deze vraag wilt verwijderen? Alle gegevens in de vraag worden verwijderd."
    cancel-message="Nee"
    confirm-message="Ja"
  />

  <back-link />

  <simple-spinner
    v-if="saving || gespreksresultaten.loading || kanalenKeuzelijst.loading"
  />

  <form v-else class="afhandeling" @submit.prevent="submit">
    <utrecht-heading :level="1" modelValue>Afhandeling</utrecht-heading>

    <application-message
      v-if="errorMessage != ''"
      messageType="error"
      :message="errorMessage"
    />

    <template
      v-else-if="
        contactmomentStore.huidigContactmoment && kanalenKeuzelijst.success
      "
    >
      <article
        v-for="(vraag, idx) in contactmomentStore.huidigContactmoment.vragen"
        :key="idx"
      >
        <utrecht-heading :level="2">
          <div class="vraag-heading">
            Vraag {{ idx + 1 }}

            <utrecht-button
              v-if="contactmomentStore.huidigContactmoment.vragen.length > 1"
              appearance="subtle-button"
              class="icon icon-after trash icon-only"
              :title="`Vraag ${idx + 1} verwijderen`"
              @click="toggleRemoveVraagDialog(idx)"
            />
          </div>
        </utrecht-heading>
        <section v-if="vraag.klanten.length" class="gerelateerde-resources">
          <utrecht-heading :level="3">{{
            vraag.klanten.length > 1 ? "Klanten" : "Klant"
          }}</utrecht-heading>
          <ul>
            <li v-for="record in vraag.klanten" :key="record.klant.id">
              <label>
                <span
                  v-if="
                    record.klant.voornaam ||
                    record.klant.achternaam ||
                    record.klant.bedrijfsnaam
                  "
                  >{{
                    [
                      record.klant.voornaam,
                      record.klant.voorvoegselAchternaam,
                      record.klant.achternaam,
                    ]
                      .filter((x) => x)
                      .join(" ") || record.klant.bedrijfsnaam
                  }}</span
                >
                <span v-else>{{
                  [
                    ...record.klant.emailadressen,
                    ...record.klant.telefoonnummers,
                  ]
                    .filter((x) => x)
                    .join(", ")
                }}</span>
                <input
                  title="Deze klant opslaan bij het contactmoment"
                  type="checkbox"
                  v-model="record.shouldStore"
                />
              </label>
            </li>
          </ul>
        </section>

        <section v-if="vraag.zaken.length" class="gerelateerde-resources">
          <utrecht-heading :level="3">{{
            vraag.zaken.length > 1 ? "Gerelateerde zaken" : "Gerelateerde zaak"
          }}</utrecht-heading>
          <application-message
            class="error-message"
            message="Let op, zaken koppelen aan een contactmoment is stuk tot PC-316 is uitgevoerd"
            message-type="error"
          />
          <ul>
            <li v-for="record in vraag.zaken" :key="record.zaak.id">
              <label>
                <span
                  >{{ record.zaak.identificatie }}
                  <div>
                    (Zaaktype: {{ record.zaak.zaaktypeOmschrijving }})
                  </div></span
                >
                <input
                  title="Deze zaak opslaan bij het contactmoment"
                  type="checkbox"
                  v-model="record.shouldStore"
                />
              </label>
            </li>
          </ul>
        </section>

        <section v-if="vraag.medewerkers.length" class="gerelateerde-resources">
          <utrecht-heading :level="3">{{
            vraag.medewerkers.length > 1
              ? "Gerelateerde medewerkers"
              : "Gerelateerde medewerker"
          }}</utrecht-heading>
          <ul>
            <li v-for="record in vraag.medewerkers" :key="record.medewerker.id">
              <label>
                <span
                  v-if="
                    record.medewerker.voornaam || record.medewerker.achternaam
                  "
                  >{{
                    [
                      record.medewerker.voornaam,
                      record.medewerker.voorvoegselAchternaam,
                      record.medewerker.achternaam,
                    ]
                      .filter((x) => x)
                      .join(" ")
                  }}
                  <span v-if="record.medewerker.emailadres"
                    >({{ record.medewerker.emailadres }})</span
                  >
                </span>
                <input
                  title="Deze medewerker opslaan bij het contactmoment"
                  type="checkbox"
                  v-model="record.shouldStore"
                />
              </label>
            </li>
          </ul>
        </section>

        <section v-if="vraag.websites.length" class="gerelateerde-resources">
          <utrecht-heading :level="3">{{
            vraag.websites.length > 1
              ? "Gerelateerde websites"
              : "Gerelateerde website"
          }}</utrecht-heading>
          <ul>
            <li v-for="record in vraag.websites" :key="record.website.url">
              <label>
                <a
                  :href="record.website.url"
                  rel="noopener noreferrer"
                  target="_blank"
                  >{{ record.website.title }}</a
                >
                <input
                  title="Deze website opslaan bij het contactmoment"
                  type="checkbox"
                  v-model="record.shouldStore"
                />
              </label>
            </li>
          </ul>
        </section>

        <section
          v-if="vraag.kennisartikelen.length"
          class="gerelateerde-resources"
        >
          <utrecht-heading :level="3">{{
            vraag.kennisartikelen.length > 1
              ? "Gerelateerde kennisartikelen"
              : "Gerelateerde kennisartikel"
          }}</utrecht-heading>
          <ul>
            <li
              v-for="record in vraag.kennisartikelen"
              :key="record.kennisartikel.url"
            >
              <label>
                <span>
                  {{ record.kennisartikel.title }}
                </span>
                <input
                  title="Dit kennisartikel opslaan bij het contactmoment"
                  type="checkbox"
                  v-model="record.shouldStore"
                />
              </label>
            </li>
          </ul>
        </section>

        <section
          v-if="vraag.nieuwsberichten.length"
          class="gerelateerde-resources"
        >
          <utrecht-heading :level="3">{{
            vraag.nieuwsberichten.length > 1
              ? "Gerelateerde nieuwsberichten"
              : "Gerelateerd nieuwsbericht"
          }}</utrecht-heading>
          <ul>
            <li
              v-for="record in vraag.nieuwsberichten"
              :key="record.nieuwsbericht.url"
            >
              <label>
                <span>
                  {{ record.nieuwsbericht.title }}
                </span>
                <input
                  title="Dit nieuwsbericht opslaan bij het contactmoment"
                  type="checkbox"
                  v-model="record.shouldStore"
                />
              </label>
            </li>
          </ul>
        </section>

        <section
          v-if="vraag.werkinstructies.length"
          class="gerelateerde-resources"
        >
          <utrecht-heading :level="3">{{
            vraag.werkinstructies.length > 1
              ? "Gerelateerde werkinstructies"
              : "Gerelateerde werkinstructie"
          }}</utrecht-heading>
          <ul>
            <li
              v-for="record in vraag.werkinstructies"
              :key="record.werkinstructie.url"
            >
              <label>
                <span>
                  {{ record.werkinstructie.title }}
                </span>
                <input
                  title="Deze werkinstructie opslaan bij het contactmoment"
                  type="checkbox"
                  v-model="record.shouldStore"
                />
              </label>
            </li>
          </ul>
        </section>

        <section v-if="vraag.vacs.length" class="gerelateerde-resources">
          <utrecht-heading :level="3">{{
            vraag.vacs.length > 1 ? "Gerelateerde VACs" : "Gerelateerde VAC"
          }}</utrecht-heading>
          <ul>
            <li v-for="record in vraag.vacs" :key="record.vac.url">
              <label>
                {{ record.vac.title }}
                <input
                  title="Deze VAC opslaan bij het contactmoment"
                  type="checkbox"
                  v-model="record.shouldStore"
                />
              </label>
            </li>
          </ul>
        </section>

        <section class="details">
          <utrecht-heading :level="3"> Details </utrecht-heading>
          <fieldset class="utrecht-form-fieldset">
            <label
              :for="'hoofdvraag' + idx"
              class="utrecht-form-label required"
            >
              Vraag
            </label>

            <contactmoment-vraag
              :idx="idx"
              :vraag="vraag"
              v-model="vraag.vraag"
            />
            <label
              :class="['utrecht-form-label', { required: !vraag.vraag }]"
              :for="'specifiekevraag' + idx"
            >
              Specifieke vraag
            </label>
            <input
              :required="!vraag.vraag"
              type="text"
              class="utrecht-textbox utrecht-textbox--html-input"
              :id="'specifiekevraag' + idx"
              v-model="vraag.specifiekevraag"
            />

            <label class="utrecht-form-label" :for="'notitie' + idx"
              >Notitie</label
            >
            <textarea
              class="utrecht-textarea"
              :id="'notitie' + idx"
              v-model="vraag.notitie"
            ></textarea>
            <label :for="'kanaal' + idx" class="utrecht-form-label required"
              >Kanaal</label
            >
            <select
              :id="'kanaal' + idx"
              v-model="vraag.kanaal"
              class="utrecht-select utrecht-select--html-select"
              @change="setUserChannel"
              required
            >
              <template v-if="!kanalenKeuzelijst.data.length">
                <option>Telefoon</option>
                <option>E-mail</option>
                <option>Contactformulier</option>
                <option>Twitter</option>
                <option>Facebook</option>
                <option>LinkedIn</option>
                <option>Live chat</option>
                <option>Instagram</option>
                <option>WhatsApp</option>
              </template>
              <option
                v-else
                v-for="{ naam } in kanalenKeuzelijst.data"
                :key="naam"
              >
                {{ naam }}
              </option>
            </select>

            <label
              :for="'gespreksresultaat' + idx"
              class="utrecht-form-label required"
            >
              Afhandeling
            </label>
            <select
              :id="'gespreksresultaat' + idx"
              v-model="vraag.gespreksresultaat"
              class="utrecht-select utrecht-select--html-select"
              required
              v-if="gespreksresultaten.success"
            >
              <option
                v-for="(gespreksresultaat, i) in gespreksresultaten.data"
                :key="`gespreksresultaat_${i}`"
              >
                {{ gespreksresultaat.definitie }}
              </option>
            </select>
          </fieldset>
        </section>

        <section v-if="vraag.gespreksresultaat !== CONTACTVERZOEK_GEMAAKT">
          <utrecht-heading :level="3"> Afgehandeld voor</utrecht-heading>
          <fieldset class="utrecht-form-fieldset">
            <label :for="'afdeling' + idx" class="utrecht-form-label required"
              >Afdeling</label
            >
            <div class="relative">
              <afdelingen-search
                v-model="vraag.afdeling"
                :exact-match="true"
                :id="'afdeling' + idx"
                class="utrecht-textbox utrecht-textbox--html-input"
                :required="true"
                placeholder="Zoek een afdeling"
              />
            </div>
          </fieldset>
        </section>

        <section
          v-if="vraag.gespreksresultaat === CONTACTVERZOEK_GEMAAKT"
          class="contactverzoek-container"
        >
          <utrecht-heading :level="3"> Contactverzoek</utrecht-heading>
          <contactverzoek-formulier v-model="vraag.contactverzoek" />
        </section>
      </article>
      <menu>
        <li>
          <utrecht-button
            modelValue
            type="button"
            appearance="secondary-action-button"
            @click="cancelDialog.reveal"
          >
            Annuleren
          </utrecht-button>
        </li>
        <li>
          <utrecht-button type="submit" appearance="primary-action-button">
            Opslaan
          </utrecht-button>
        </li>
      </menu>
    </template>
  </form>
</template>

<script setup lang="ts">
import { onMounted, ref } from "vue";
import { useRouter } from "vue-router";
import {
  Heading as UtrechtHeading,
  Button as UtrechtButton,
} from "@utrecht/component-library-vue";
import SimpleSpinner from "@/components/SimpleSpinner.vue";
import ApplicationMessage from "@/components/ApplicationMessage.vue";
import { useContactmomentStore, type Vraag } from "@/stores/contactmoment";
import { toast } from "@/stores/toast";
import {
  koppelKlant,
  saveContactmoment,
  koppelObject,
  useGespreksResultaten,
  type Contactmoment,
  koppelZaakContactmoment,
  CONTACTVERZOEK_GEMAAKT,
  saveContactverzoek,
  mapContactverzoekData,
} from "@/features/contact/contactmoment";

import {
  type ContactverzoekData,
} from "../components/types";

import { 
  saveKlantContact,
  saveInternetaak,
  mapContactmomentToInternetaak,
  saveBetrokkene,
  saveDigitaleAdressen,
  enrichInterneTaakWithActoren,
  useOpenKlant2
 } from "../../../services/openklant2/service";

 import type { 
  KlantContactPostmodel
 } from "../../../services/openklant2/types";

import { useOrganisatieIds, useUserStore } from "@/stores/user";
import { useConfirmDialog } from "@vueuse/core";
import PromptModal from "@/components/PromptModal.vue";
import { nanoid } from "nanoid";
import { writeContactmomentDetails } from "@/features/contact/contactmoment/write-contactmoment-details";
import BackLink from "@/components/BackLink.vue";
import AfdelingenSearch from "@/features/contact/components/AfdelingenSearch.vue";
import { fetchAfdelingen } from "@/features/contact/components/afdelingen";
import contactmomentVraag from "@/features/contact/contactmoment/ContactmomentVraag.vue";
import { useKanalenKeuzeLijst } from "@/features/Kanalen/service";
import ContactverzoekFormulier from "../contactverzoek/formulier/ContactverzoekFormulier.vue";

const router = useRouter();
const contactmomentStore = useContactmomentStore();
const saving = ref(false);
const errorMessage = ref("");
const gespreksresultaten = useGespreksResultaten();
const kanalenKeuzelijst = useKanalenKeuzeLijst();

onMounted(() => {
  // nog even laten voor een test: rechtstreeks opvragen van een klant.
  // fetchLoggedIn(
  //   "api/klanten/api/v1/klanten/1561a8f4-0d7d-48df-8bf1-e6cf23afc9e5" //"/api/klanten/klanten/api/v1/klanten/1561a8f4-0d7d-48df-8bf1-e6cf23afc9e5"
  // )
  //   .then((x) => x.json())
  //   .then((x) => console.log(x));

  if (!contactmomentStore.huidigContactmoment) return;
  for (const vraag of contactmomentStore.huidigContactmoment.vragen) {
    if (vraag.contactverzoek.isActive) {
      vraag.gespreksresultaat = CONTACTVERZOEK_GEMAAKT;
    }
    if (!vraag.kanaal) {
      vraag.kanaal = userStore.preferences.kanaal;
    }
  }
});

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
            ...zaak,
          });
        } catch (e) {
          try {
            console.log(
              "koppelZaakContactmoment in openzaak attempt 1 failed",
              e,
            );
            await koppelZaakContactmoment({
              contactmoment: contactmomentId,
              ...zaak,
            });
          } catch (e) {
            try {
              console.log(
                "koppelZaakContactmoment in openzaak attempt 2 failed",
                e,
              );
              await koppelZaakContactmoment({
                contactmoment: contactmomentId,
                ...zaak,
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

const saveBetrokkeneBijKlantContact = async (
  vraag: Vraag, 
  klantcontactId: string, 
  contactverzoekData?: Partial<ContactverzoekData>
): Promise<string[]> => {
  const betrokkenenUuids: string[] = [];

  for (const { shouldStore, klant } of vraag.klanten) {
    if (shouldStore && klant.id) {
      const organisatie = contactverzoekData?.betrokkene?.organisatie || klant.bedrijfsnaam;
      const voornaam = contactverzoekData?.betrokkene?.persoonsnaam?.voornaam || klant.voornaam;
      const voorvoegselAchternaam = contactverzoekData?.betrokkene?.persoonsnaam?.voorvoegselAchternaam || klant.voorvoegselAchternaam;
      const achternaam = contactverzoekData?.betrokkene?.persoonsnaam?.achternaam || klant.achternaam;

      const result = await saveBetrokkene({
        partijId: klant.id,
        klantcontactId: klantcontactId,
        organisatienaam: organisatie,
        voornaam: voornaam,
        voorvoegselAchternaam: voorvoegselAchternaam,
        achternaam: achternaam,
      });
      betrokkenenUuids.push(result.uuid);
    }
  }

  return betrokkenenUuids;
};


const saveVraag = async (vraag: Vraag, gespreksId?: string) => {
  const useKlantInteractiesApi = await useOpenKlant2();

  const isContactverzoek = vraag.gespreksresultaat === CONTACTVERZOEK_GEMAAKT;
  const isAnoniem = !vraag.klanten.some((x) => x.shouldStore && x.klant.id);

  // gedeeld contactmoment voor contactmomentdetails
  const contactmoment: Contactmoment = {
    uuid: "",
    bronorganisatie: organisatieIds.value[0] || "",
    registratiedatum: new Date().toISOString(),
    kanaal: vraag.kanaal,
    tekst: vraag.notitie,
    onderwerpLinks: [],
    initiatiefnemer: "klant",
    vraag: vraag?.vraag?.title,
    specifiekevraag: vraag.specifiekevraag || undefined,
    gespreksresultaat: vraag.gespreksresultaat || "Onbekend",
    verantwoordelijkeAfdeling: vraag.afdeling?.naam || undefined,
    startdatum: vraag.startdatum || new Date().toISOString(),
    einddatum: new Date().toISOString(),
    gespreksId,
    voorkeurskanaal: "",
    voorkeurstaal: "",
    medewerker: "",
    vorigContactmoment: undefined,
    toelichting: ""
  };

  let contactverzoekData: Omit<ContactverzoekData, "contactmoment"> | undefined;
  
  if (isContactverzoek) {
  const klantUrl = vraag.klanten
    .filter((x) => x.shouldStore)
    .map((x) => x.klant.url)
    .find(Boolean);

  contactverzoekData = mapContactverzoekData({
    klantUrl,
    data: vraag.contactverzoek,
  });

  Object.assign(contactmoment, contactverzoekData);
  }

  // Klantcontacten flow
  if (useKlantInteractiesApi) {
    const klantcontact: KlantContactPostmodel = {
      kanaal: vraag.kanaal,
      onderwerp:
        vraag.vraag?.title === "anders"
          ? vraag.specifiekevraag
          : vraag.vraag
            ? vraag.specifiekevraag
              ? `${vraag.vraag.title} (${vraag.specifiekevraag})`
              : vraag.vraag.title
            : vraag.specifiekevraag,
      inhoud: vraag.notitie,
      indicatieContactGelukt: true,
      taal: "nld",
      vertrouwelijk: false,
      plaatsgevondenOp: new Date().toISOString(),
    };

    const savedKlantContactResult = await saveKlantContact(klantcontact);

    if (savedKlantContactResult.errorMessage || !savedKlantContactResult.data) {
      return savedKlantContactResult;
    }

    await writeContactmomentDetails(
      contactmoment,
      savedKlantContactResult.data?.url,
    );

    contactmoment.uuid = savedKlantContactResult.data?.uuid;

    // Drie scenario's:

    // 1. Klantcontact met betrokkenen
    if (!isContactverzoek && !isAnoniem) {
      await saveBetrokkeneBijKlantContact(vraag, savedKlantContactResult.data?.uuid);

    // 2. Contactverzoek met betrokkenen
    } else if (isContactverzoek && !isAnoniem && contactverzoekData) {
      const betrokkenenUuids = await saveBetrokkeneBijKlantContact(vraag, savedKlantContactResult.data?.uuid, contactverzoekData);

      if (betrokkenenUuids.length > 0) {
        const saveAdressenPromises = betrokkenenUuids.map(async (betrokkeneUuid) => {
          if (betrokkeneUuid && contactverzoekData?.betrokkene?.digitaleAdressen?.length) {
            return saveDigitaleAdressen(
              contactverzoekData.betrokkene.digitaleAdressen,
              betrokkeneUuid
            );
          }
        });
        await Promise.all(saveAdressenPromises);
      }

      const interneTaak = mapContactmomentToInternetaak(contactmoment);
      await enrichInterneTaakWithActoren(interneTaak, contactverzoekData?.actor);

      const savedContactverzoekResult = await saveInternetaak(interneTaak);
      return savedContactverzoekResult || savedKlantContactResult;

      // 3. Anoniem contactverzoek 
      } else if (isContactverzoek && isAnoniem && contactverzoekData) {

        const betrokkeneResult = await saveBetrokkene({
        partijId: undefined,
        klantcontactId: savedKlantContactResult.data?.uuid,
        organisatienaam: contactverzoekData?.betrokkene?.organisatie,
        voornaam: contactverzoekData?.betrokkene?.persoonsnaam?.voornaam,
        voorvoegselAchternaam: contactverzoekData?.betrokkene?.persoonsnaam?.voorvoegselAchternaam,
        achternaam: contactverzoekData?.betrokkene?.persoonsnaam?.achternaam,
      });

      const betrokkeneUuid = betrokkeneResult.uuid;

      if (betrokkeneUuid && contactverzoekData?.betrokkene?.digitaleAdressen?.length) {
        await saveDigitaleAdressen(
          contactverzoekData.betrokkene.digitaleAdressen,
          betrokkeneUuid
        );
      }

      const interneTaak = mapContactmomentToInternetaak(contactmoment);
      await enrichInterneTaakWithActoren(interneTaak, contactverzoekData?.actor);

      const savedContactverzoekResult = await saveInternetaak(interneTaak);
      return savedContactverzoekResult || savedKlantContactResult;
    }

    return savedKlantContactResult;
    }
    else
    {

    // Contactmomenten flow
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
  }
};

const navigateToPersonen = () => router.push({ name: "personen" });

async function submit() {
  try {
    saving.value = true;
    errorMessage.value = "";
    if (!contactmomentStore.huidigContactmoment) return;

    const { vragen } = contactmomentStore.huidigContactmoment;

    const saveVraagResult = await saveVraag(vragen[0]);

    if (saveVraagResult.errorMessage) {
      handleSaveVraagError(saveVraagResult.errorMessage);
    } else {
      // Gespreksid zit niet in savevraagresult als we de klantcontacten flow volgen
      const gespreksId =
        saveVraagResult.data && "gespreksId" in saveVraagResult.data
          ? saveVraagResult.data.gespreksId
          : undefined;

      await handleSaveVraagSuccess(gespreksId, vragen.slice(1));
    }
  } catch (error) {
    errorMessage.value =
      "Er is een fout opgetreden bij opslaan van het contactmoment";
  } finally {
    saving.value = false;
  }
}

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

const userStore = useUserStore();
const organisatieIds = useOrganisatieIds();

const handleSaveVraagError = (msg: string) => {
  errorMessage.value = msg;
};

const handleSaveVraagSuccess = async (
  gespreksId: string | undefined,
  otherVragen: Vraag[],
) => {
  if (!gespreksId) {
    gespreksId = nanoid();
  }

  const promises = otherVragen.map((x) => saveVraag(x, gespreksId));
  const otherVrageSaveResults = await Promise.all(promises);
  const firstErrorInOtherVragen = otherVrageSaveResults.find(
    (x) => x.errorMessage,
  );

  if (firstErrorInOtherVragen && firstErrorInOtherVragen.errorMessage) {
    handleSaveVraagError(firstErrorInOtherVragen.errorMessage);
    return;
  }

  // klaar
  contactmomentStore.stop();
  toast({ text: "Het contactmoment is opgeslagen" });
  navigateToPersonen();
};

function setUserChannel(e: Event) {
  if (!(e.target instanceof HTMLSelectElement)) return;
  userStore.setKanaal(e.target.value);
}

const cancelDialog = useConfirmDialog();
cancelDialog.onConfirm(() => {
  contactmomentStore.stop();
  navigateToPersonen();
});

const removeVraagDialog = useConfirmDialog();

const toggleRemoveVraagDialog = async (vraagId: number) => {
  await removeVraagDialog.reveal().then((res) => {
    if (res.isCanceled) return;

    contactmomentStore.removeVraag(vraagId);
  });
};

const trySetOfficieleAfdeling = async (vraag: Vraag) => {
  if (!vraag.vraag?.afdeling) {
    vraag.afdeling = undefined;
    return;
  }
  const artikelAfdelingen = await fetchAfdelingen(vraag.vraag.afdeling, true);
  vraag.afdeling = artikelAfdelingen.page[0];
};

onMounted(() => {
  if (!contactmomentStore.huidigContactmoment) return;
  const promises = contactmomentStore.huidigContactmoment.vragen.map(
    trySetOfficieleAfdeling,
  );
  return Promise.all(promises);
});
</script>

<style scoped lang="scss">
.afhandeling {
  max-width: var(--section-width-large);

  // content stacked
  display: flex;
  flex-direction: column;

  --label-width: 16rem;
  --label-gap: var(--spacing-default);
}

:deep(.notitie) {
  min-height: 10rem;
  width: 100%;
  padding: var(--spacing-default);
  box-sizing: border-box;
  margin-top: var(--spacing-default);
}

.vraag-heading {
  display: flex;
  align-items: center;
  gap: var(--spacing-small);
}

.gerelateerde-resources {
  ul {
    margin-top: var(--spacing-small);
  }

  li {
    padding: var(--spacing-small);
    border: 1px solid var(--color-tertiary);

    &:not(:first-of-type) {
      border-top: none;
    }

    > label {
      display: flex;
      gap: var(--spacing-default);
      justify-content: space-between;
    }
  }

  input[type="checkbox"] {
    margin: var(--spacing-extrasmall);
    scale: 1.5;
  }
}

fieldset {
  display: grid;
  grid-template-columns: var(--label-width) auto;
  gap: var(--label-gap);
}

article {
  padding-block: var(--spacing-large);

  section {
    padding-block: var(--spacing-default);
  }
}

menu {
  display: flex;
  justify-content: flex-end;
  gap: var(--spacing-large);
}

input,
select {
  accent-color: var(--color-primary);
}

.contactverzoek-container {
  :deep(label) {
    display: grid;
    grid-template-columns: var(--label-width) auto;
    gap: var(--label-gap);
  }

  :deep(fieldset) {
    gap: var(--label-gap);
  }
}

.warning {
  padding: var(--spacing-default);
  margin-block-end: var(--spacing-default);
}

:deep(fieldset.radio-group) {
  gap: var(--spacing-default);
  display: flex;

  > legend {
    position: absolute;
  }

  > label {
    display: flex;
    gap: var(--spacing-extrasmall);
    line-height: var(--utrecht-form-fieldset-legend-line-height);

    &:first-of-type {
      margin-inline-start: calc(var(--label-width) + var(--label-gap));
    }
  }
}

.relative {
  position: relative;
}
</style>
