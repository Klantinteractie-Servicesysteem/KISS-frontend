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
import { toast } from "@/stores/toast";
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
  level?: 1 | 2 | 3 | 4;
}>();

const { systemen } = useSystemen();

// const logboekData = ref<{
//   count: 0;
//   next: null;
//   previous: null;
//   results: LogboekActiviteit[];
// } | null>();

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
const logboekActiviteiten = ref<LogboekActiviteit[]>([]);

const systeem = ref<Systeem | undefined>(undefined);

watchEffect(async () => {
  ///todo!!
  //alleen iets doen als  je openklant gebruikt. niets doen im geval van esuite. de esuite maakt geen logboek records aan dus er is toch niets te vinden
  // en niet alle aanvullende calls zullen slagen als je dit toch probeert met de esuite!

  systeem.value = systemen.value?.find(
    (x) => x.registryVersion === registryVersions.ok2,
  );

  console.log(systeem.value, 123);
  if (!systeem.value) {
    return;
  }

  const logboekUrl = `/api/logboek/api/v2/objects?data_attr=heeftBetrekkingOp__objectId__exact__${props.contactverzoekId}`;
  logboekActiviteiten.value = [];
  await fetchLoggedIn(logboekUrl)
    .then(throwIfNotOk)
    .then(parseJson)
    .then(async (r) => {
      logboekActiviteiten.value = await mapLogboek(r.results);
    })
    .catch(() =>
      toast({
        text: "Er is een fout opgetreden bij het ophalen van het contactverzoek logboek. Probeer het later opnieuw.",
        type: "error",
      }),
    );
});

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

const mapLogboek = async (logboek: any) => {
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
      //let op. kiss ondersteunt meerdere klant registers. primair om e-suite naast openklant te kunnen gebruiken
      // in dat geval is dit geen issue. de esuite maaakt immers toch geen logboek aan
      // maar als je meerdere andere openklant achtige systemen grbuikt, waarbij ITA wel gebruikt wordt voor afhandeling
      // dn weten we nu eigenlijk niet uit welk register we de bijbehorende contacmoment gegevesn moeten halen
      // we pakken voorals nog het eerste openklant regsiter dat we in de confioguratie vonden.
      // er zijn nog geen scenario's in beeld waarin dat niet zal gaan werken.
      //mochten er tzt meer contcatregisters bij komen, dan moeten we hier iets mee

      if (item.heeftBetrekkingOp?.length != 1) {
        return;
      }

      const contactmoment = await fetchKlantcontact({
        systeemId: systeem.value.identifier,
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
      const actor = await fetchActor(systeem.value.identifier, actorId);
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
        systeem.value.identifier,
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
        systeem.value.identifier,
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

    // activiteiten.Add(activiteit);
  }

  return logItems;
};
</script>

<style>
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
