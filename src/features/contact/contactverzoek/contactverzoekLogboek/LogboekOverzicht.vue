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
        <article class="highlight">
          <utrecht-heading :level="level ? level + 2 : 4"
            >Interne toelichting</utrecht-heading
          >
          <p>{{ logboekItem.notitie }}</p>
        </article>
        <ul class="meta">
          <li>{{ logboekItem.datum }}</li>
          <li>{{ logboekItem.uitgevoerdDoor }}</li>
          <li>{{ logboekItem.kanaal }}</li>
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

const props = defineProps<{
  contactverzoekId: string;
  level?: 1 | 2 | 3 | 4;
}>();

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

watchEffect(async () => {
  const logboekUrl = `/api/logboek/api/v2/objects?data_attr=heeftBetrekkingOp__objectId__exact__${props.contactverzoekId}`;
  logboekActiviteiten.value = [];
  await fetchLoggedIn(logboekUrl)
    .then(throwIfNotOk)
    .then(parseJson)
    .then((r) => {
      logboekActiviteiten.value = mapLogboek(r.results);
    })
    .catch(() =>
      toast({
        text: "Er is een fout opgetreden bij het ophalen van het contactverzoek logboek. Probeer het later opnieuw.",
        type: "error",
      }),
    );
});

const mapLogboek = (logboek: any) => {
  const logItems = [];

  const activiteiten = logboek[0]?.record?.data?.activiteiten;
  for (let i = 0; i < activiteiten.length; i++) {
    const item = activiteiten[i];

    const activiteit = {
      datum: item.datum,
      type: item.type,
      titel: "ssssss", // GetActionTitle(item.Type)
    };
    activiteit.id = "mmmm";
    activiteit.kanaal = "Onbekend";
    activiteit.tekst = "sdfsdfs";
    activiteit.contactGelukt = "gelukt";
    activiteit.uitgevoerdDoor = "iemand";
    activiteit.notitie = "nnnnnn nnn n n ";

    // switch (item.Type)
    // {
    //     case ActiviteitTypes.Klantcontact when item.HeeftBetrekkingOp.Count == 1:
    //         {
    //             var contactmoment =
    //                 await _openKlantApiClient.GetKlantcontactAsync(item.HeeftBetrekkingOp.Single().ObjectId);
    //             if (contactmoment != null)
    //             {
    //                 activiteit.Id = contactmoment.Uuid;
    //                 activiteit.Kanaal = contactmoment.Kanaal ?? "Onbekend";
    //                 activiteit.Tekst = contactmoment.Inhoud;
    //                 activiteit.ContactGelukt = contactmoment.IndicatieContactGelukt;
    //                 activiteit.UitgevoerdDoor = GetName( item);
    //                 activiteit.Notitie = item.Notitie;

    //                 activiteit.Titel = contactmoment.IndicatieContactGelukt.HasValue && contactmoment.IndicatieContactGelukt.Value
    //                     ? "Contact gelukt"
    //                     : "Contact niet gelukt";
    //             }

    //             break;
    //         }
    //     case ActiviteitTypes.Toegewezen when item.HeeftBetrekkingOp.Count == 1:
    //         {
    //             var actorId = item.HeeftBetrekkingOp.Single().ObjectId;
    //             var actor = await _openKlantApiClient.GetActorAsync(actorId);
    //             if (actor != null)
    //             {
    //                 activiteit.UitgevoerdDoor = GetName(item);
    //                 activiteit.Tekst = $"Contactverzoek opgepakt door {actor.Naam ?? "Onbekend"}";
    //             }

    //             break;
    //         }
    //     case ActiviteitTypes.ZaakGekoppeld:
    //         {
    //             var zaak = await zakenApiClient.GetZaakAsync(item.HeeftBetrekkingOp.Single().ObjectId);
    //             if (zaak != null)
    //             {
    //                 activiteit.UitgevoerdDoor = GetName(item);
    //                 activiteit.Tekst = $"Zaak {zaak.Identificatie} gekoppeld aan het contactverzoek";
    //             }

    //             break;
    //         }
    //     case ActiviteitTypes.ZaakkoppelingGewijzigd:
    //         {
    //             var zaak = await zakenApiClient.GetZaakAsync(item.HeeftBetrekkingOp.Single().ObjectId);
    //             if (zaak != null)
    //             {
    //                 activiteit.UitgevoerdDoor = GetName(item);
    //                 activiteit.Tekst = $"Zaak {zaak.Identificatie} gekoppeld aan het contactverzoek";
    //             }

    //             break;
    //         }
    //     case ActiviteitTypes.Verwerkt:
    //         {

    //             activiteit.UitgevoerdDoor = GetName(item);
    //             activiteit.Tekst = $"Contactverzoek afgerond";

    //             break;
    //         }
    //     case ActiviteitTypes.InterneNotitie:
    //         {
    //             activiteit.UitgevoerdDoor = GetName(item);
    //             activiteit.Notitie = item.Notitie;
    //             break;
    //         }
    // }

    logItems.push(activiteit);

    // activiteiten.Add(activiteit);
  }

  return logItems;
};
</script>

<style>
li > * {
  padding-block: var(--spacing-small);
  padding-inline: var(--spacing-default);
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
