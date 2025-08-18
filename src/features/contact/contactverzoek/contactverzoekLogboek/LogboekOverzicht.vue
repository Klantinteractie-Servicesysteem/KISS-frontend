<template>
  <section>
    <utrecht-heading :level="level ? level + 1 : 3">Logboek</utrecht-heading>
    <ul class="logboek" v-if="logboekActiviteiten?.length">
      <li
        v-for="logboekItem in logboekActiviteiten"
        :key="logboekItem.uuid"
        class="ita-step"
      >
        <utrecht-heading :level="level ? level + 2 : 4">{{
          logboekItem.titel
        }}</utrecht-heading>

        <p>{{ logboekItem.tekst }}</p>
        <article class="highlight" v-if="logboekItem.notitie">
          <utrecht-heading :level="level ? level + 2 : 4"
            >Interne toelichting</utrecht-heading
          >
          <p>{{ logboekItem.notitie }}</p>
        </article>
        <ul class="meta">
          <li><DateTimeOrNvt :date="logboekItem.datum" /></li>
          <li>{{ logboekItem.uitgevoerdDoor }}</li>
          <li>
            {{ logboekItem.kanaal ? "Kanaal: " + logboekItem.kanaal : "" }}
          </li>
        </ul>
      </li>
    </ul>
  </section>
</template>

<script lang="ts" setup>
import { fetchLoggedIn, parseJson, throwIfNotOk } from "@/services";

import { ref, watchEffect } from "vue";
import { Heading as UtrechtHeading } from "@utrecht/component-library-vue";
import DateTimeOrNvt from "@/components/DateTimeOrNvt.vue";
import { fetchActor, fetchKlantcontact } from "@/services/openklant2";
import {
  registryVersions,
  useSystemen,
  type Systeem,
} from "@/services/environment/fetch-systemen";
import { fetchZaakIdentificatieByUrlOrId } from "@/services/openzaak";

const props = defineProps<{
  contactverzoekId: string;
  contactverzoekSysteemId: string;
  level?: 1 | 2 | 3 | 4;
}>();

interface LogboekActiviteit {
  datum: string;
  type: string;
  titel: string;
  kanaal: string | undefined;
  tekst: string | undefined;
  uuid: string;
  contactGelukt: string | undefined;
  uitgevoerdDoor: string | undefined;
  notitie: string | undefined;
}

//const { systemen } = useSystemen();
//const systeem = ref<Systeem | undefined>(undefined);

const logboekActiviteiten = ref<LogboekActiviteit[]>([]);

const activiteitTypes = {
  klantcontact: "klantcontact",
  toegewezen: "toegewezen",
  verwerkt: "verwerkt",
  zaakGekoppeld: "zaak-gekoppeld",
  zaakkoppelingGewijzigd: "zaakkoppeling-gewijzigd",
  interneNotitie: "interne-notitie",
};

const getActionTitle = (type: string) =>
  new Map<string, string>([
    [activiteitTypes.klantcontact, "Klantcontact"],
    [activiteitTypes.toegewezen, "Toegewezen"],
    [activiteitTypes.verwerkt, "Verwerkt"],
    [activiteitTypes.zaakGekoppeld, "Zaak gekoppeld"],
    [activiteitTypes.zaakkoppelingGewijzigd, "Zaakkoppeling gewijzigd"],
    [activiteitTypes.interneNotitie, "Interne notitie"],
  ]).get(type) || "Onbekende actie";

watchEffect(async () => {
  // Let op. kiss ondersteunt meerdere klantcontact registers tegelijk.
  // Primair om e-suite naast openklant te kunnen gebruiken.
  // In dat geval is er geen issue. de e-suite maaakt immers toch geen logboek aan.
  // We hoeven dus alleen te kijken of er ook een ok2 systeem is.
  // Maar als er meerdere ok2 systemen tegelijk in gebruik zijn, dan weten we niet in welke eenvullende gegevens van het contact opgehaald moeten worden.
  // We pakken voorals nog het eerste openklant regsiter dat we in de confioguratie vinden.
  // Er zijn nog geen scenario's in beeld waarin dat niet zal gaan werken.
  // Mochten er tzt meer contcatregisters bij komen, dan moeten we hier iets mee.

  // systeem.value = systemen.value?.find(
  //   (x) => x.registryVersion === registryVersions.ok2,
  // );

  // if (!systeem.value) {
  //   return;
  // }

  logboekActiviteiten.value = [];
  await fetchLoggedIn(
    `/api/logboek/api/v2/objects?data_attr=heeftBetrekkingOp__objectId__exact__${props.contactverzoekId}`,
  )
    .then(throwIfNotOk)
    .then(parseJson)
    .then(async (r) => {
      logboekActiviteiten.value = await mapLogboek(r.results);
    });
});

const mapLogboek = async (logboek: any): Promise<LogboekActiviteit[]> => {
  const logItems = [];

  const activiteiten = logboek[0]?.record?.data?.activiteiten;
  for (let i = 0; i < activiteiten.length; i++) {
    const item = activiteiten[i];

    const activiteit = {
      datum: item.datum,
      type: item.type,
      titel: getActionTitle(item.type),
      uitgevoerdDoor: item.actor?.naam ?? "Onbekend",
    };

    if (item.type === activiteitTypes.klantcontact) {
      if (item.heeftBetrekkingOp?.length != 1) {
        return;
      }

      const contactmoment = await fetchKlantcontact({
        systeemId: props.contactverzoekSysteemId,
        expand: [],
        uuid: item.heeftBetrekkingOp[0].objectId,
      });

      activiteit.id = contactmoment.uuid;
      activiteit.kanaal = contactmoment.kanaal ?? "Onbekend";
      activiteit.tekst = contactmoment.inhoud;
      activiteit.contactGelukt = contactmoment.indicatieContactGelukt;

      activiteit.notitie = item.notitie;
      activiteit.titel = contactmoment.indicatieContactGelukt
        ? "Contact gelukt"
        : "Contact niet gelukt";
    }

    if (activiteitTypes.toegewezen === item.type) {
      if (item.heeftBetrekkingOp.length != 1) {
        return;
      }

      const actorId = item.heeftBetrekkingOp[0].objectId;
      const actor = await fetchActor(props.contactverzoekSysteemId, actorId);
      if (actor != null) {
        activiteit.tekst = `Contactverzoek opgepakt door ${actor.naam ?? "Onbekend"}`;
      }
    }

    if (activiteitTypes.zaakGekoppeld === item.type) {
      if (item.heeftBetrekkingOp.length != 1) {
        return;
      }

      const zaakId = item.heeftBetrekkingOp[0].objectId;

      const zaakIdentificatie = await fetchZaakIdentificatieByUrlOrId(
        props.contactverzoekSysteemId,
        zaakId,
      );
      if (zaakIdentificatie != null) {
        activiteit.tekst = `Zaak ${zaakIdentificatie} gekoppeld aan het contactverzoek`;
      }
    }

    if (activiteitTypes.zaakkoppelingGewijzigd === item.type) {
      if (item.heeftBetrekkingOp.length != 1) {
        return;
      }

      const zaakId = item.heeftBetrekkingOp[0].objectId;

      const zaakIdentificatie = await fetchZaakIdentificatieByUrlOrId(
        props.contactverzoekSysteemId,
        zaakId,
      );
      if (zaakIdentificatie != null) {
        activiteit.tekst = `Zaak ${zaakIdentificatie} gekoppeld aan het contactverzoek`;
      }
    }
    if (activiteitTypes.verwerkt === item.type) {
      activiteit.tekst = "Contactverzoek afgerond";
    }
    if (activiteitTypes.interneNotitie === item.type) {
      activiteit.notitie = item.notitie;
    }

    logItems.push(activiteit);
  }

  return logItems;
};
</script>

<style scoped>
.logboek li > * {
  padding-block: var(--spacing-small);
  padding-inline: var(--spacing-default);
}

.meta li > * {
  padding-block: 0;
  padding-inline: 0;
}

.highlight {
  --highlight-border-width: 4px;

  border-inline-start: var(--highlight-border-width) var(--color-primary) solid;
  padding-inline-start: calc(
    var(--spacing-default) - var(--highlight-border-width)
  );
  background-color: var(--color-secondary);
}

.meta {
  display: flex;
  flex-direction: row;
  gap: var(--spacing-default);
  color: var(--color-grey);
  font-style: italic;
}

.meta li:last-child {
  margin-left: auto;
}

.logboek {
  display: flex;
  gap: 1rem;
  flex-direction: column;
}

.logboek > li {
  background: var(--color-white);
  border: 1px solid var(--color-accent);
}
</style>
