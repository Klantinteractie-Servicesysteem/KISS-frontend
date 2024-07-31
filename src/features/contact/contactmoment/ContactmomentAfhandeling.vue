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
                  [record.klant.emailadres, record.klant.telefoonnummer]
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
  useGespreksResultaten,
  CONTACTVERZOEK_GEMAAKT,
} from "@/features/contact/contactmoment";
import { useUserStore } from "@/stores/user";
import { useConfirmDialog } from "@vueuse/core";
import PromptModal from "@/components/PromptModal.vue";
import BackLink from "@/components/BackLink.vue";
import AfdelingenSearch from "@/features/contact/components/AfdelingenSearch.vue";
import { fetchAfdelingen } from "@/features/contact/components/afdelingen";
import contactmomentVraag from "@/features/contact/contactmoment/ContactmomentVraag.vue";
import { useKanalenKeuzeLijst } from "@/features/Kanalen/service";
import ContactverzoekFormulier from "../contactverzoek/formulier/ContactverzoekFormulier.vue";
import { useEsuite } from "./esuite";

const router = useRouter();
const contactmomentStore = useContactmomentStore();
const saving = ref(false);
const errorMessage = ref("");
const gespreksresultaten = useGespreksResultaten();
const kanalenKeuzelijst = useKanalenKeuzeLijst();
const esuite = useEsuite();

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

const navigateToPersonen = () => router.push({ name: "personen" });

async function submit() {
  try {
    saving.value = true;
    errorMessage.value = "";
    if (!contactmomentStore.huidigContactmoment) return;

    const result = await esuite.save();

    if (result.errorMessage) {
      handleSaveVraagError(result.errorMessage);
    } else {
      toast({ text: "Het contactmoment is opgeslagen" });
      navigateToPersonen();
    }
  } catch (error) {
    errorMessage.value =
      "Er is een fout opgetreden bij opslaan van het contactmoment";
  } finally {
    saving.value = false;
  }
}

const userStore = useUserStore();

const handleSaveVraagError = (msg: string) => {
  errorMessage.value = msg;
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
