<template>
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
            <li v-for="klant in vraag.klanten" :key="klant.id">
              <label :for="'klant-' + klant.id">
                <span
                  v-if="
                    klant.voornaam || klant.achternaam || klant.bedrijfsnaam
                  "
                  >{{
                    [
                      klant.voornaam,
                      klant.voorvoegselAchternaam,
                      klant.achternaam,
                    ]
                      .filter((x) => x)
                      .join(" ") || klant.bedrijfsnaam
                  }}</span
                >
                <span v-else>{{
                  [...klant.emailadressen, ...klant.telefoonnummers]
                    .filter((x) => x)
                    .join(", ")
                }}</span>
                <input
                  title="Deze klant opslaan bij het contactmoment"
                  type="radio"
                  :value="klant"
                  v-model="vraag.klantToStore"
                />
              </label>
            </li>
            <li>
              <label>
                <span>Geen klant opslaan</span>
                <input
                  title="Geen zaak opslaan bij het contactmoment"
                  type="radio"
                  :value="undefined"
                  v-model="vraag.klantToStore"
                />
              </label>
            </li>
          </ul>
        </section>

        <section v-if="vraag.zaken.length" class="gerelateerde-resources">
          <utrecht-heading :level="3">{{
            vraag.zaken.length > 1 ? "Gerelateerde zaken" : "Gerelateerde zaak"
          }}</utrecht-heading>
          <ul>
            <li v-for="record in vraag.zaken" :key="record.zaak.url">
              <label>
                <span
                  >{{ record.zaak.identificatie }}
                  <div>(Zaaktype: {{ record.zaak.zaaktypeOmschrijving }})</div>
                </span>
                <input
                  title="Deze zaak opslaan bij het contactmoment"
                  type="radio"
                  :name="idx + 'zaak'"
                  :checked="record.shouldStore"
                  @change="
                    (e) =>
                      (e.target as HTMLInputElement).checked &&
                      contactmomentStore.selectZaak(record, vraag)
                  "
                />
              </label>
            </li>
            <li>
              <label>
                <span>Geen </span>
                <input
                  title="Geen zaak opslaan bij het contactmoment"
                  type="radio"
                  :name="idx + 'zaak'"
                  :checked="!vraag.zaken.some(({ shouldStore }) => shouldStore)"
                  @change="
                    (e) =>
                      (e.target as HTMLInputElement).checked &&
                      contactmomentStore.selectZaak(undefined, vraag)
                  "
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
              Specifieke vraag<span class="utrecht-form-field-description"
                >(maximaal 180 tekens)</span
              >
            </label>
            <input
              :required="!vraag.vraag"
              type="text"
              class="utrecht-textbox utrecht-textbox--html-input"
              :id="'specifiekevraag' + idx"
              v-model="vraag.specifiekevraag"
              :maxlength="SPECIFIEKEVRAAG_MAXLENGTH"
            />

            <max-length-text-area
              v-model="vraag.notitie"
              :maxlength="NOTITIE_MAXLENGTH"
              :id="'notitie' + idx"
            />

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
          <contactmoment-canceller />
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
import {
  useContactmomentStore,
  type ContactmomentKlant,
  type ContactmomentState,
  type Vraag,
} from "@/stores/contactmoment";
import { toast } from "@/stores/toast";
import {
  koppelKlant,
  useGespreksResultaten,
  CONTACTVERZOEK_GEMAAKT,
  saveContactverzoek,
  mapContactverzoekData,
  koppelZaakEnContactmoment,
} from "@/features/contact/contactmoment";

import { type ContactverzoekData } from "../components/types";

import {
  saveKlantContact,
  saveInternetaak,
  saveBetrokkene,
  saveDigitaleAdressen,
  ensureActoren,
  postOnderwerpobject,
  ensureOk2Klant,
} from "@/services/openklant2/service";

import type { SaveInterneTaakResponseModel } from "../../../services/openklant2/types";

import { useOrganisatieIds, useUserStore } from "@/stores/user";
import { useConfirmDialog } from "@vueuse/core";
import PromptModal from "@/components/PromptModal.vue";
import { nanoid } from "nanoid";
import {
  writeContactmomentDetails,
  type ContactmomentDetails,
} from "@/features/contact/contactmoment/write-contactmoment-details";
import BackLink from "@/components/BackLink.vue";
import AfdelingenSearch from "@/features/contact/components/AfdelingenSearch.vue";
import { fetchAfdelingen } from "@/features/contact/components/afdelingen";
import contactmomentVraag from "@/features/contact/contactmoment/ContactmomentVraag.vue";
import { useKanalenKeuzeLijst } from "@/features/Kanalen/service";
import ContactverzoekFormulier from "../contactverzoek/formulier/ContactverzoekFormulier.vue";
import {
  fetchSystemen,
  registryVersions,
  type Systeem,
} from "@/services/environment/fetch-systemen";
import {
  ensureKlantForBedrijfIdentifier,
  ensureOk1Klant,
  saveContactmoment,
} from "@/services/openklant1";
import type { Contactmoment, Klant } from "@/services/openklant/types";
import {
  SPECIFIEKEVRAAG_MAXLENGTH,
  NOTITIE_MAXLENGTH,
} from "@/services/openklant/service";
import MaxLengthTextArea from "../components//MaxLengthTextArea.vue";
import ContactmomentCanceller from "./ContactmomentCanceller.vue";

const router = useRouter();
const contactmomentStore = useContactmomentStore();
const saving = ref(false);
const errorMessage = ref("");
const gespreksresultaten = useGespreksResultaten();
const kanalenKeuzelijst = useKanalenKeuzeLijst();

onMounted(async () => {
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
  systeemId: string,
  vraag: Vraag,
  contactmomentId: string,
) => {
  for (const { zaak, shouldStore } of vraag.zaken) {
    if (shouldStore) {
      try {
        await koppelZaakEnContactmoment(systeemId, zaak, contactmomentId);
      } catch (e) {
        // zaken toevoegen aan een contactmoment en andersom retourneert soms een error terwijl de data meetal wel correct opgeslagen is.
        // toch maar verder gaan dus
        console.error(e);
      }
    }
  }
};

const koppelKlanten = async (
  systemId: string,
  klanten: Klant[],
  contactmomentId: string,
) => {
  for (const klant of klanten) {
    await koppelKlant({
      systemId,
      contactmomentId,
      klantUrl: klant.url,
    });
  }
};

const saveBetrokkeneBijContactverzoek = async (
  systemIdentifier: string,
  klanten: Array<ContactmomentKlant & Klant>,
  klantcontactId: string,
  contactverzoekData?: Partial<ContactverzoekData>,
): Promise<string[]> => {
  const betrokkenenUuids: string[] = [];

  if (!klanten.length) {
    // voor een contactverzoek zonder klant moet OOK een betrokkene aangemaakt worden
    const organisatie = contactverzoekData?.betrokkene?.organisatie;
    const voornaam = contactverzoekData?.betrokkene?.persoonsnaam?.voornaam;
    const voorvoegselAchternaam =
      contactverzoekData?.betrokkene?.persoonsnaam?.voorvoegselAchternaam;
    const achternaam = contactverzoekData?.betrokkene?.persoonsnaam?.achternaam;

    const result = await saveBetrokkene(systemIdentifier, {
      klantcontactId: klantcontactId,
      organisatienaam: organisatie,
      voornaam: voornaam,
      voorvoegselAchternaam: voorvoegselAchternaam,
      achternaam: achternaam,
    });
    betrokkenenUuids.push(result.uuid);
  }

  for (const klant of klanten) {
    if (klant.id) {
      const organisatie =
        contactverzoekData?.betrokkene?.organisatie || klant.bedrijfsnaam;
      const voornaam =
        contactverzoekData?.betrokkene?.persoonsnaam?.voornaam ||
        klant.voornaam;
      const voorvoegselAchternaam =
        contactverzoekData?.betrokkene?.persoonsnaam?.voorvoegselAchternaam ||
        klant.voorvoegselAchternaam;
      const achternaam =
        contactverzoekData?.betrokkene?.persoonsnaam?.achternaam ||
        klant.achternaam;

      const result = await saveBetrokkene(systemIdentifier, {
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

const saveBetrokkeneBijContactmoment = async (
  systemIdentifier: string,
  klanten: Klant[],
  klantcontactId: string,
) => {
  for (const klant of klanten) {
    await saveBetrokkene(systemIdentifier, {
      partijId: klant.id,
      klantcontactId: klantcontactId,
    });
  }
};

const saveVraag = async (vraag: Vraag, gespreksId?: string) => {
  // if this contactmoment/contactverzoek is releated to a zaak,
  // then we should store this contactmoment/contactverzoek in the registry..
  // that is linked to the zaaksysteem that contains this particular zaak

  const systemen = await fetchSystemen();

  const zaakSysteemId = vraag.zaken.find((x) => x.shouldStore)?.zaaksysteemId;
  const systeem = systemen.find(
    ({ isDefault, identifier }) =>
      (!zaakSysteemId && isDefault) || identifier === zaakSysteemId,
  );
  if (!systeem) {
    throw new Error("Geen systeem gevonden");
  }
  const systemIdentifier = systeem.identifier;
  const useKlantInteractiesApi =
    systeem.registryVersion === registryVersions.ok2;

  // now we now to which system all data should be posted.
  // the klanten we have selected, come from the default system
  // depending on whether we want to link a zaak,
  // we need to ensure the klanten exist in the target system as well
  // these are the actual klanten we need to use in the rest of the process
  const klanten = await ensureKlanten(systeem, vraag);

  const isContactverzoek = vraag.gespreksresultaat === CONTACTVERZOEK_GEMAAKT;
  const isAnoniem = !klanten.length;
  const isNietAnoniemContactmoment = !isContactverzoek && !isAnoniem;

  // gedeeld contactmoment voor contactmomentdetails
  const contactmoment: Contactmoment = {
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
  };

  let contactverzoekData: Omit<ContactverzoekData, "contactmoment"> | undefined;

  if (isContactverzoek) {
    const klantUrl = klanten.map((x) => x.url).find(Boolean);

    contactverzoekData = mapContactverzoekData({
      klantUrl,
      data: vraag.contactverzoek,
    });

    Object.assign(contactmoment, contactverzoekData);
  }

  const fullUrl = (url: string) =>
    url.startsWith("http") ? url : new URL(url, location.origin).toString();

  const cmDetails: ContactmomentDetails = {
    ...contactmoment,
    bronnen: [
      ...vraag.kennisartikelen
        .filter(({ shouldStore }) => shouldStore)
        .map(({ kennisartikel: { title, url } }) => ({
          url: fullUrl(url),
          titel: title,
          soort: "kennisartikel",
        })),
      ...vraag.werkinstructies
        .filter(({ shouldStore }) => shouldStore)
        .map(({ werkinstructie: { title, url } }) => ({
          url: fullUrl(url),
          titel: title,
          soort: "werkinstructie",
        })),
      ...vraag.vacs
        .filter(({ shouldStore }) => shouldStore)
        .map(({ vac: { title, url } }) => ({
          url: fullUrl(url),
          titel: title,
          soort: "vac",
        })),
      ...vraag.nieuwsberichten
        .filter(({ shouldStore }) => shouldStore)
        .map(({ nieuwsbericht: { title, url } }) => ({
          url: fullUrl(url),
          titel: title,
          soort: "nieuwsbericht",
        })),
      ...vraag.websites
        .filter(({ shouldStore }) => shouldStore)
        .map(({ website: { title, url } }) => ({
          url: fullUrl(url),
          titel: title,
          soort: "website",
        })),
    ],
  };

  // Openklant2 flow
  if (useKlantInteractiesApi) {
    // 1. klantcontact opslaan
    // 2. contactmomentdetails opslaan (wat niet in de standaardapi's mee kan, maar tbv management rapportage wel opgeslagen dient te worden)
    // 3. betrokkenen opslaan in geval van een niet anoniem contactmoment of een contactverzoek
    // 4. contactgegevens (digitaleadressen) opslaan bij een contactverzoek
    // 5. actoren opslaan bij een contactverzoek (aan wie het contactverzoek is toegewezen)
    // 6. internetaak opslaan bij een contactverzoek (dit is in wezen het cotnactverzoek)
    // 7. zaken toevoegen

    let betrokkenenUuids: string[] = [];

    // 1 //////////////////////
    const savedKlantContactResult = await saveKlantContact(
      systemIdentifier,
      vraag,
    );

    if (
      savedKlantContactResult.errorMessage ||
      !savedKlantContactResult.data ||
      !savedKlantContactResult.data?.uuid
    ) {
      return savedKlantContactResult;
    }

    const savedKlantContactId = savedKlantContactResult.data.uuid;

    // 2 //////////////////////
    await writeContactmomentDetails(
      cmDetails,
      savedKlantContactResult.data?.url,
    );

    // 3 //////////////////////
    if (isContactverzoek) {
      betrokkenenUuids = await saveBetrokkeneBijContactverzoek(
        systemIdentifier,
        klanten,
        savedKlantContactId,
        contactverzoekData,
      );
    }

    if (isNietAnoniemContactmoment) {
      await saveBetrokkeneBijContactmoment(
        systemIdentifier,
        klanten,
        savedKlantContactId,
      );
    }

    // 4 ///////////////////////
    if (isContactverzoek) {
      if (betrokkenenUuids.length > 0) {
        const saveAdressenPromises = betrokkenenUuids.map(
          async (betrokkeneUuid) => {
            if (
              betrokkeneUuid &&
              contactverzoekData?.betrokkene?.digitaleAdressen?.length
            ) {
              return saveDigitaleAdressen(
                systemIdentifier,
                contactverzoekData.betrokkene.digitaleAdressen,
                betrokkeneUuid,
              );
            }
          },
        );
        await Promise.all(saveAdressenPromises);
      }
    }

    // 5 /////////////////////////
    let actoren: string[] = [];
    if (isContactverzoek) {
      actoren = await ensureActoren(
        systemIdentifier,
        contactverzoekData?.actor,
      );
    }

    // 6 /////////////////////////
    let savedContactverzoekResult: SaveInterneTaakResponseModel | null = null;
    if (isContactverzoek) {
      savedContactverzoekResult = await saveInternetaak(
        systemIdentifier,
        contactverzoekData?.toelichting ?? "",
        savedKlantContactId,
        actoren,
      );
    }

    // 7 ////////////////////////
    for (const zaakinfo of vraag.zaken ?? []) {
      // onderwerp object van het type zaak
      if (zaakinfo.shouldStore) {
        const onderwerpObject = {
          klantcontact: {
            uuid: savedKlantContactId,
          },
          wasKlantcontact: null,
          onderwerpobjectidentificator: {
            objectId: zaakinfo.zaak.uuid,
            codeObjecttype: "zgw-Zaak",
            codeRegister: "openzaak",
            codeSoortObjectId: "uuid",
          },
        };

        //voeg de zaak toe aan het contactmoment
        await postOnderwerpobject(systemIdentifier, onderwerpObject);

        //voeg het contactmoment toe aan de zaak
        //DIT WERKT NIET
        //OPENZAAK ACCEPTEERT ALLEEN OK1 Contactmomenten, GEEN KLANTCONTACTEN
        //ER ZIJN GEEN PLANNEN OM DIT MOGELIJK TE GAAN MAKEN
        // addContactmomentToZaak(
        //   savedKlantContactResult.data?.url as string,
        //   zaakinfo.zaak?.url,
        //   zaakinfo.zaaksysteemId,
        // );
      }
    }

    return savedContactverzoekResult || savedKlantContactResult;
  } else {
    // Contactmomenten flow
    addKennisartikelenToContactmoment(contactmoment, vraag);
    addWebsitesToContactmoment(contactmoment, vraag);
    addMedewerkersToContactmoment(contactmoment, vraag);
    addNieuwsberichtToContactmoment(contactmoment, vraag);
    addWerkinstructiesToContactmoment(contactmoment, vraag);
    addVacToContactmoment(contactmoment, vraag);

    const klantUrl = klanten.map((x) => x.url).find(Boolean);

    const isContactverzoek = vraag.gespreksresultaat === CONTACTVERZOEK_GEMAAKT;
    let cvData;
    if (isContactverzoek) {
      cvData = mapContactverzoekData({
        klantUrl,
        data: vraag.contactverzoek,
      });
      Object.assign(contactmoment, cvData);
    }

    const savedContactmomentResult = await saveContactmoment(
      systemIdentifier,
      contactmoment,
    );

    if (
      savedContactmomentResult.errorMessage ||
      !savedContactmomentResult.data
    ) {
      return savedContactmomentResult;
    }

    const savedContactmoment = savedContactmomentResult.data;

    const promises = [
      writeContactmomentDetails(cmDetails, savedContactmoment.url),
      zakenToevoegenAanContactmoment(
        systemIdentifier,
        vraag,
        savedContactmoment.url,
      ),
    ];

    if (isContactverzoek && cvData) {
      promises.push(
        saveContactverzoek({
          systemIdentifier,
          data: cvData,
          contactmomentUrl: savedContactmoment.url,
        }),
      );
    }

    promises.push(
      koppelKlanten(systemIdentifier, klanten, savedContactmoment.url),
    );

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
    const validationMessage = validateContactmomentState(
      contactmomentStore.huidigContactmoment,
    );
    if (validationMessage) {
      toast({
        type: "error",
        text: validationMessage,
        timeout: 30_000,
        dismissable: true,
      });
      return;
    }

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
  } catch {
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

  const otherVragenSaveResults = [];
  for (const vraag of otherVragen) {
    otherVragenSaveResults.push(await saveVraag(vraag, gespreksId));
  }
  const firstErrorInOtherVragen = otherVragenSaveResults.find(
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

const ensureKlanten = async (systeem: Systeem, vraag: Vraag) => {
  const result = [];
  const klant = vraag.klantToStore;
  if (klant) {
    let klantInSysteem;
    if (systeem.registryVersion === registryVersions.ok1) {
      const bronorganisatie = organisatieIds.value[0] || "";
      if (klant.bsn) {
        klantInSysteem = await ensureOk1Klant(
          systeem.identifier,
          { bsn: klant.bsn },
          bronorganisatie,
        );
      } else if (klant.vestigingsnummer) {
        klantInSysteem = await ensureKlantForBedrijfIdentifier(
          systeem.identifier,
          {
            identifier: { vestigingsnummer: klant.vestigingsnummer },
            bedrijfsnaam: "",
          },
          bronorganisatie,
        );
      } else if (klant.kvkNummer) {
        klantInSysteem = await ensureKlantForBedrijfIdentifier(
          systeem.identifier,
          {
            identifier: { kvkNummer: klant.kvkNummer },
            bedrijfsnaam: "",
          },
          bronorganisatie,
        );
      }
    } else {
      if (klant.bsn) {
        klantInSysteem = await ensureOk2Klant(systeem.identifier, {
          bsn: klant.bsn,
        });
      } else if (klant.vestigingsnummer && klant.kvkNummer) {
        klantInSysteem = await ensureOk2Klant(systeem.identifier, {
          vestigingsnummer: klant.vestigingsnummer,
          kvkNummer: klant.kvkNummer,
        });
      } else if (klant.kvkNummer) {
        klantInSysteem = await ensureOk2Klant(systeem.identifier, {
          kvkNummer: klant.kvkNummer,
        });
      }
    }

    if (!klantInSysteem) {
      throw new Error(
        "geen valide klantidentificator. verwachtte een bsn, vestigingsnummer, rsin of kvknummer",
      );
    }

    result.push({
      ...klant,
      ...klantInSysteem,
    });
  }
  return result;
};

const validateContactmomentState = (cm: ContactmomentState) => {
  const validateContactverzoekToelichtingLength = () => {
    const TOELICHTING_MAX_LENGTH = 1_000;
    for (const [index, vraag] of cm.vragen.entries()) {
      if (vraag.gespreksresultaat === CONTACTVERZOEK_GEMAAKT) {
        const contactverzoek = mapContactverzoekData({
          data: vraag.contactverzoek,
        });
        const toelichtingLength = contactverzoek.toelichting?.length ?? 0;
        if (toelichtingLength > TOELICHTING_MAX_LENGTH) {
          return (
            `De interne informatie van vraag ${index + 1} bevat ${toelichtingLength} tekens, ` +
            `dit mag maximaal ${TOELICHTING_MAX_LENGTH} tekens zijn. ` +
            `Verwijder ${toelichtingLength - TOELICHTING_MAX_LENGTH} tekens ` +
            `uit de Interne toelichting voor de medewerker, ` +
            `of uit de velden van een contactverzoekformulier.`
          );
        }
      }
    }
  };

  return validateContactverzoekToelichtingLength();
};
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

  input:is([type="checkbox"], [type="radio"]) {
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
  :deep(.utrecht-form-label) {
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
