<template>
  <section v-if="logboekActiviteiten?.length">
    <utrecht-heading :level="level ? level + 1 : 3">Logboek</utrecht-heading>
    <ul class="logboek">
      <li
        v-for="logboekItem in logboekActiviteiten"
        :key="logboekItem.datum"
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
import { onMounted, ref, watchEffect } from "vue";
import { Heading as UtrechtHeading } from "@utrecht/component-library-vue";
import DateTimeOrNvt from "@/components/DateTimeOrNvt.vue";
import { fetchActor, fetchKlantcontact } from "@/services/openklant2";
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
  kanaal?: string | undefined;
  tekst?: string | undefined;
  contactGelukt?: string | undefined;
  uitgevoerdDoor: string | undefined;
  notitie?: string | undefined;
}

const useLogboek = ref<boolean>(false);
const logboekActiviteiten = ref<LogboekActiviteit[]>([]);

const activiteitTypes = {
  klantcontact: "klantcontact",
  toegewezen: "toegewezen",
  verwerkt: "verwerkt",
  zaakGekoppeld: "zaak-gekoppeld",
  zaakkoppelingGewijzigd: "zaakkoppeling-gewijzigd",
  interneNotitie: "interne-notitie",
};

onMounted(() => {
  fetchLoggedIn("/api/environment/use-logboek")
    .then((r) => r.json())
    .then((x) => {
      useLogboek.value = x.useLogboek;
    });
});

watchEffect(async () => {
  if (!useLogboek.value) {
    return;
  }

  logboekActiviteiten.value = [];
  await fetchLoggedIn(
    `/api/logboek/api/v2/objects?data_attr=heeftBetrekkingOp__objectId__exact__${props.contactverzoekId}`,
  )
    .then(throwIfNotOk)
    .then(parseJson)
    .then(
      async (r) =>
        (logboekActiviteiten.value = await mapAndEnrichLogboek(r.results)),
    );
});

const mapAndEnrichLogboek = async (
  logboek: any,
): Promise<LogboekActiviteit[]> => {
  const logItems = [];

  const activiteiten = logboek[0]?.record?.data?.activiteiten;
  for (let i = (activiteiten?.length ?? 1) - 1; i >= 0; i--) {
    const item = activiteiten[i];

    const activiteit: LogboekActiviteit = {
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
      default: {
        break;
      }
    }

    logItems.push(activiteit);
  }

  return logItems;
};

async function enrichActiviteitWithKlantContactInfo(
  activiteit: LogboekActiviteit,
  item: { heeftBetrekkingOp: { objectId: string }[]; notitie: string },
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
  activiteit: LogboekActiviteit,
  item: { heeftBetrekkingOp: { objectId: string }[]; notitie: string },
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
  activiteit: LogboekActiviteit,
  item: { heeftBetrekkingOp: { objectId: string }[]; notitie: string },
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

function enrichActiviteitWithVerwerktInfo(activiteit: LogboekActiviteit) {
  activiteit.tekst = "Contactverzoek afgerond";
}

function enrichActiviteitWithNotitieInfo(
  activiteit: LogboekActiviteit,
  item: { heeftBetrekkingOp: { objectId: string }[]; notitie: string },
) {
  activiteit.notitie = item.notitie;
}

const getActionTitle = (type: string) =>
  new Map<string, string>([
    [activiteitTypes.klantcontact, "Klantcontact"],
    [activiteitTypes.toegewezen, "Opgepakt"],
    [activiteitTypes.verwerkt, "Afgerond"],
    [activiteitTypes.zaakGekoppeld, "Zaak gekoppeld"],
    [activiteitTypes.zaakkoppelingGewijzigd, "Zaakkoppeling gewijzigd"],
    [activiteitTypes.interneNotitie, "Interne notitie"],
  ]).get(type) || "Onbekende actie";
</script>

<style scoped>
.highlight {
  --highlight-border-width: 4px;

  border-inline-start: var(--highlight-border-width) var(--color-primary) solid;
  padding-inline-start: calc(
    var(--spacing-default) - var(--highlight-border-width)
  );
  background-color: var(--color-secondary);
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

.logboek li > * {
  padding-block: var(--spacing-small);
  padding-inline: var(--spacing-default);
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

.logboek .meta li > * {
  padding-block: 0;
  padding-inline: 0;
}
</style>
