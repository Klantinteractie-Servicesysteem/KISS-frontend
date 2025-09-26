<template>
  <section v-if="loading"><simple-spinner /></section>
  <section v-else-if="logboekActiviteiten?.length">
    <utrecht-heading :level="level">Logboek</utrecht-heading>
    <ul class="logboek">
      <li
        v-for="logboekItem in logboekActiviteiten"
        :key="logboekItem.datum"
        class="ita-step"
      >
        <utrecht-heading :level="level + 1">{{
          logboekItem.titel
        }}</utrecht-heading>

        <p v-if="logboekItem.tekst">{{ logboekItem.tekst }}</p>
        <article class="highlight" v-if="logboekItem.notitie">
          <utrecht-heading :level="level + 1"
            >Interne toelichting</utrecht-heading
          >
          <p>{{ logboekItem.notitie }}</p>
        </article>
        <ul class="meta">
          <li><DutchDateTime :date="logboekItem.datum" /></li>
          <li>{{ logboekItem.uitgevoerdDoor }}</li>
          <li>
            {{ logboekItem.kanaal ? "Kanaal: " + logboekItem.kanaal : "" }}
          </li>
        </ul>
      </li>
    </ul>
  </section>
  <application-message
    v-else-if="error"
    messageType="error"
    message="Er is een fout opgetreden. Het logboek van dit contactverzoek kan niet getoond worden."
  />
</template>

<script lang="ts" setup>
import { fetchLoggedIn, parseJson, throwIfNotOk, useLoader } from "@/services";
import { Heading as UtrechtHeading } from "@utrecht/component-library-vue";
import DutchDateTime from "@/components/DutchDateTime.vue";
import { fetchActor, fetchKlantcontact } from "@/services/openklant2";
import { fetchZaakIdentificatieByUrlOrId } from "@/services/openzaak";
import SimpleSpinner from "@/components/SimpleSpinner.vue";
import ApplicationMessage from "@/components/ApplicationMessage.vue";
import { codeObjecttype } from "@/services/knownConsts";

const { level = 3, ...props } = defineProps<{
  contactverzoekId: string;
  contactverzoekSysteemId: string;
  level?: number;
}>();

type InputLogboekActiviteit = {
  datum: string;
  type: string;
  titel: string;
  actor?: { naam?: string };
  heeftBetrekkingOp: {
    codeObjecttype: string;
    objectId: string;
    codeRegister: string;
  }[];
  notitie?: string;
};

interface EnrichedLogboekActiviteit {
  datum: string;
  type: string;
  titel: string;
  kanaal?: string | undefined;
  tekst?: string | undefined;
  contactGelukt?: string | undefined;
  uitgevoerdDoor: string | undefined;
  notitie?: string | undefined;
}

const activiteitTypes = {
  klantcontact: "klantcontact",
  toegewezen: "toegewezen",
  verwerkt: "verwerkt",
  zaakGekoppeld: "zaak-gekoppeld",
  zaakkoppelingGewijzigd: "zaakkoppeling-gewijzigd",
  interneNotitie: "interne-notitie",
  doorsturen: "doorsturen",
};

const { data: useLogboek } = useLoader(() =>
  fetchLoggedIn("/api/environment/use-logboek")
    .then((r) => r.json())
    .then((x) => !!x.useLogboek),
);

const {
  data: logboekActiviteiten,
  loading,
  error,
} = useLoader(() => {
  if (useLogboek.value)
    return fetchLoggedIn(
      `/api/logboek/api/v2/objects?data_attr=heeftBetrekkingOp__objectId__exact__${props.contactverzoekId}`,
    )
      .then(throwIfNotOk)
      .then(parseJson)
      .then((r) => mapAndEnrichLogboek(r.results));
});

const sortActiviteitByDateDescending = (
  activiteiten: EnrichedLogboekActiviteit[],
) =>
  activiteiten.sort(
    (a, b) => new Date(b.datum).valueOf() - new Date(a.datum).valueOf(),
  );

const mapAndEnrichLogboek = async (
  logboek: {
    record: {
      data: {
        activiteiten: InputLogboekActiviteit[];
      };
    };
  }[],
): Promise<EnrichedLogboekActiviteit[]> => {
  const logItems = [];

  const activiteiten = logboek[0]?.record?.data?.activiteiten ?? [];

  for (const item of activiteiten) {
    const activiteit: EnrichedLogboekActiviteit = {
      datum: item.datum,
      type: item.type,
      titel: getActionTitle(item.type),
      uitgevoerdDoor: item.actor?.naam ?? "Onbekend",
    };

    switch (item.type) {
      case activiteitTypes.klantcontact: {
        await enrichActiviteitWithKlantContactInfo(activiteit, item);
        break;
      }
      case activiteitTypes.toegewezen: {
        await enrichActiviteitWithToegewezenAanInfo(activiteit, item);
        break;
      }
      case activiteitTypes.zaakGekoppeld: {
        await enrichActiviteitWithZaakInfo(activiteit, item);
        break;
      }
      case activiteitTypes.zaakkoppelingGewijzigd: {
        await enrichActiviteitWithZaakInfo(activiteit, item);
        break;
      }
      case activiteitTypes.verwerkt: {
        enrichActiviteitWithVerwerktInfo(activiteit);
        break;
      }
      case activiteitTypes.interneNotitie: {
        enrichActiviteitWithNotitieInfo(activiteit, item);
        break;
      }
      case activiteitTypes.doorsturen: {
        await enrichActiviteitWithDoorsturenInfo(activiteit, item);
        break;
      }
      default: {
        break;
      }
    }

    logItems.push(activiteit);
  }

  return sortActiviteitByDateDescending(logItems);
};

async function enrichActiviteitWithKlantContactInfo(
  activiteit: EnrichedLogboekActiviteit,
  item: InputLogboekActiviteit,
) {
  if (item.heeftBetrekkingOp?.length != 1) {
    return [];
  }

  const contactmoment = await fetchKlantcontact({
    systeemId: props.contactverzoekSysteemId,
    expand: [],
    uuid: item.heeftBetrekkingOp[0].objectId,
  });

  activiteit.kanaal = contactmoment.kanaal ?? "Onbekend";
  activiteit.tekst = contactmoment.inhoud;
  activiteit.contactGelukt = contactmoment.indicatieContactGelukt;

  activiteit.notitie = item.notitie;
  activiteit.titel = contactmoment.indicatieContactGelukt
    ? "Contact gelukt"
    : "Contact niet gelukt";
}

async function enrichActiviteitWithToegewezenAanInfo(
  activiteit: EnrichedLogboekActiviteit,
  item: InputLogboekActiviteit,
) {
  if (item.heeftBetrekkingOp.length != 1) {
    return [];
  }

  const actorId = item.heeftBetrekkingOp[0].objectId;
  const actor = await fetchActor(props.contactverzoekSysteemId, actorId);
  if (actor != null) {
    activiteit.tekst = `Contactverzoek opgepakt door ${actor.naam ?? "Onbekend"}`;
  }
}

async function enrichActiviteitWithZaakInfo(
  activiteit: EnrichedLogboekActiviteit,
  item: InputLogboekActiviteit,
) {
  if (item.heeftBetrekkingOp.length != 1) {
    return [];
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

function enrichActiviteitWithVerwerktInfo(
  activiteit: EnrichedLogboekActiviteit,
) {
  activiteit.tekst = "Contactverzoek afgerond";
}

function enrichActiviteitWithNotitieInfo(
  activiteit: EnrichedLogboekActiviteit,
  item: InputLogboekActiviteit,
) {
  activiteit.notitie = item.notitie;
}

async function enrichActiviteitWithDoorsturenInfo(
  activiteit: EnrichedLogboekActiviteit,
  item: InputLogboekActiviteit,
) {
  activiteit.tekst = "";
  const activiteiten: string[]  = [];
  for (const heeftbetrekkingop of item.heeftBetrekkingOp) {
    if (heeftbetrekkingop.codeRegister === "obj") {
        
      let naam = "";
      let groepOrAfdeling = "";
      
      if (heeftbetrekkingop.codeObjecttype === "afd") {
          naam = (await fetchAfdeling(heeftbetrekkingop.objectId))?.naam;   
          groepOrAfdeling = codeObjecttype[heeftbetrekkingop.codeObjecttype]?.name    
      }
      
      if (heeftbetrekkingop.codeObjecttype === "grp") {
          naam = (await fetchGroep(heeftbetrekkingop.objectId))?.naam;     
          groepOrAfdeling = codeObjecttype[heeftbetrekkingop.codeObjecttype]?.name      
      }
      
      activiteiten.push(`Contactverzoek doorgestuurd aan ${groepOrAfdeling} ${naam}`);

    } else if (heeftbetrekkingop.codeRegister === "handmatig") {
      activiteiten.push(
        `Contactverzoek doorgestuurd aan medewerker ${heeftbetrekkingop.objectId}`,
      );
    }
    activiteit.tekst = activiteiten.join(" en ");
  }
}

const activiteitTitles = new Map<string, string>([
  [activiteitTypes.klantcontact, "Klantcontact"],
  [activiteitTypes.toegewezen, "Opgepakt"],
  [activiteitTypes.verwerkt, "Afgerond"],
  [activiteitTypes.zaakGekoppeld, "Zaak gekoppeld"],
  [activiteitTypes.zaakkoppelingGewijzigd, "Zaakkoppeling gewijzigd"],
  [activiteitTypes.interneNotitie, "Interne notitie"],
  [activiteitTypes.doorsturen, "Doorgestuurd"],
]);

const getActionTitle = (type: string) =>
  activiteitTitles.get(type) || "Onbekende actie";

async function fetchAfdelingOfGroep(id: string, path: string) {
  return await fetchLoggedIn(
    `${path}api/v2/objects?data_attr=identificatie__exact__${id}`,
  )
    .then(throwIfNotOk)
    .then(parseJson)
    .then((json) => {

        if (json.results.length !== 1 ) {
            throw new Error(
                `Expected exactly one result for afdeling or groep ${id}, but got ${json.results.length}`,
            );
        }

        return json.results[0].record.data
    });
}

async function fetchAfdeling(id: string) { return fetchAfdelingOfGroep(id, "/api/afdelingen/") }
async function fetchGroep(id: string) { return fetchAfdelingOfGroep(id, "/api/groepen/") }

</script>

<style scoped>
.highlight {
  --highlight-border-width: 4px;

  border-inline-start: var(--highlight-border-width) var(--color-primary) solid;
  padding-inline-start: calc(
    var(--spacing-default) - var(--highlight-border-width)
  );
  padding-block: var(--spacing-small);
  margin-inline: calc(-1 * var(--spacing-default));
  background-color: var(--color-secondary);
}

.logboek {
  display: flex;
  gap: var(--spacing-default);
  flex-direction: column;
}

.logboek > li {
  background: var(--color-white);
  border: 1px solid var(--color-accent);
  padding-block: var(--spacing-small);
  padding-inline: var(--spacing-default);
  display: flex;
  flex-direction: column;
  gap: var(--spacing-small);
}

.meta {
  display: flex;
  flex-flow: row wrap;
  gap: var(--spacing-default);
  color: var(--color-grey);
  font-style: italic;
}

.meta li:has(+ :last-child) {
  margin-inline-end: auto;
}

.logboek .meta li > * {
  padding-block: 0;
  padding-inline: 0;
}
</style>
