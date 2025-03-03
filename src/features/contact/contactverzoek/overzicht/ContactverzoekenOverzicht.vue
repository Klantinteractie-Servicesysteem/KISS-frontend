<template>
  <section>
    <expandable-table-list :items="contactverzoeken" item-key="url">
      <template #header>
        <span id="datum-header" class="icon-after sort-descending">Datum</span>
        <span id="onderwerp-header">Onderwerp</span>
        <span id="medewerker-header">Status</span>
        <span id="kanaal-header">Behandelaar</span>
      </template>

      <template
        v-slot:item="{
          item: contactverzoek,
        }: {
          item: ContactverzoekOverzichtItem;
        }"
      >
        <summary>
          <dutch-date
            v-if="contactverzoek.registratiedatum"
            :date="new Date(contactverzoek.registratiedatum)"
          />
          <span v-else />
          <span class="max18char" :title="contactverzoek.onderwerp"
            >{{ contactverzoek.onderwerp }}
          </span>
          <span>{{ contactverzoek.status }}</span>
          <span>{{ contactverzoek.behandelaar }}</span>
        </summary>
        <dl>
          <dt>Starttijd</dt>
          <dd>
            <dutch-time
              v-if="contactverzoek.registratiedatum"
              :date="new Date(contactverzoek.registratiedatum)"
            />
          </dd>

          <dt>Toelichting voor de collega</dt>
          <dd class="preserve-newline">
            {{ contactverzoek.toelichtingVoorCollega }}
          </dd>

          <template v-if="contactverzoek.betrokkene?.persoonsnaam?.achternaam">
            <dt>Naam betrokkene</dt>
            <dd>
              {{ fullName(contactverzoek.betrokkene.persoonsnaam) }}
            </dd>
          </template>

          <template v-if="contactverzoek.betrokkene?.organisatie">
            <dt>Organisatie</dt>
            <dd>{{ contactverzoek.betrokkene?.organisatie }}</dd>
          </template>

          <template
            v-for="(adres, idx) in contactverzoek.betrokkene
              ?.digitaleAdressen || []"
            :key="idx"
          >
            <dt>
              {{
                capitalizeFirstLetter(
                  adres.omschrijving || adres.soortDigitaalAdres || "contact",
                )
              }}
            </dt>
            <dd>
              {{ adres.adres }}
            </dd>
          </template>

          <template v-if="contactverzoek.aangemaaktDoor">
            <dt>Aangemaakt door</dt>
            <dd>{{ contactverzoek.aangemaaktDoor }}</dd>
          </template>

          <dt v-if="contactverzoek.zaaknummers.length">Zaaknummer</dt>
          <dd
            v-for="zaaknummer in contactverzoek.zaaknummers"
            :key="zaaknummer"
          >
            {{ zaaknummer }}
          </dd>

          <template v-if="contactverzoek?.vraag">
            <dt>Vraag</dt>
            <dd>{{ contactverzoek.vraag }}</dd>
          </template>

          <template v-if="contactverzoek?.toelichtingBijContactmoment">
            <dt>Toelichting</dt>
            <dd>
              {{ contactverzoek.toelichtingBijContactmoment }}
            </dd>
          </template>
        </dl>
      </template>
    </expandable-table-list>
  </section>
</template>

<script lang="ts" setup>
import DutchDate from "@/components/DutchDate.vue";
import DutchTime from "@/components/DutchTime.vue";
import { fullName } from "@/helpers/string";
import ExpandableTableList from "@/components/ExpandableTableList.vue";
import type { ContactverzoekOverzichtItem } from "./types";

defineProps<{
  contactverzoeken: ContactverzoekOverzichtItem[];
}>();

const capitalizeFirstLetter = (val: string) =>
  `${val?.[0]?.toLocaleUpperCase() || ""}${val?.substring(1) || ""}`;
</script>

<style scoped>
.preserve-newline {
  white-space: pre-line;
}

.max18char {
  max-width: 18ch;
  overflow: hidden;
  white-space: nowrap;
  text-overflow: ellipsis;
}
</style>
