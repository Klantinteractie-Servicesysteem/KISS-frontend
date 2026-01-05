# Changelog

## Latest version

### New features

- For Kennisbank users: restricted search fields can be filtered.
- UI updates make Persoon/Bedrijf more clearly identifiable. #1201
- Added warning box: BRP data may only be used for verification. #1125
- Added workaround for open-klant bug to allow contactmomenten with more than 7 questions. #1340

## v1.4.0

### New features

- Supporting multiple zgw api url structures #1115
- Supporting `Kennisbank` role with limited KISS interface #1028
- Find a page indexed from a not public sharepoint #1291
- Find all the pages indexed from a non-public Sharepoint from a starting point #1295
- Update for OpenZaak 1.20 and up: Zaken for Vestigingen and Niet Natuurlijke Personen #1155

### Bugfixes

### Maintenance

Let op, REGISTERS__N__ZAAKSYSTEEM_BASE_URL is deprecated. Zie het installatiehandleiding voor de gewijzigde ZAAKSYSTEEM Environment Variabelen.

## v1.3.1

### Maintenance

- Upstream vulnerability fixes in the dotnet runtime alpine base image
- Frontend package updates

## v1.3.0

### New features

- View all the 'Logboek' information belonging to a Contactverzoek (related to ITA) #1104
- Sluiten/annuleren van gestarte contactmomenten/verzoeken #1263
- Fetch Zaken for NietNatuurlijkePersoon: rsin and kvk #1264

### Bugfixes

- Zoeken op postcode bij bedrijven is hoofdlettergevoelig #1277
- Error bij het laden van het Logboek voor een Contactverzoek #1282

### Maintenance

Er zijn nieuwe release variabelen toegevoegd (LOGBOEK_BASE_URL, LOGBOEK_TOKEN, LOGBOEK_OBJECT_TYPE_URL, LOGBOEK_OBJECT_TYPE_VERSION ). Zie handleiding.

## v1.2.1

### Warnings and deployment notes

- This release contains new image tags for KISS-Elastic-Sync and PodiumD-Adapter, because they are now based on alpine base images. We recommend updating the tags in your helm values file because they have a reduced attack vector.
- This release contains a new version of the helm chart for kiss, because we now use the bitnami legacy repository for postgres by default. We recommend updating to this new version because the old bitnami repository will stop working on August 28th, 2025.

### Bugfixes

- Better feedback when internetaak.toelichting has too many characters #1170
- Filtering search results on specific website #1172
- Warning about session end no longer appears #1233

### Maintenance

- Use alpine base image to reduce the attack vector of our docker images
- Use the bitnami archive helm repository by default for postgres in the kiss helm chart

## v1.2.0

### New features

- Ability to use collapsible sections using standard HTML #981
- Better handling of maximum number of characters in klantcontact.onderwerp #1150
- Support case insensitive property of afdelingnaam for Kennisartikelen #1159
- Validation on max length of Notitie #1175
- Implement 4th "Personen" search option #1157
- Insight into the registers a KISS instance communicates with #1216

### Bugfixes

- pc-1685 KISS | Veld 'Medewerker' is niet verplicht, resulteert in foutmelding

### Maintenance

- PC-1569 Improving accessibility of the search boxes to select Afdeling, Groep, or Medewerker
- Make contactmomenten & contactverzoeken accessible #1176

## v1.1.0

### New features

- Klant pas aanmaken in register bij opslaan contactmoment/verzoek #1078
- In dropdown titel (functie + afdeling) onder medewerker verwijderen #1165
- Support specifying untrusted ca certificates in the Kiss helm chart #1166

### Bugfixes

- vulnerability fixes van Snyk verwerkt.
- manier waarop gekoppelde zaken bij Contactmomenten en Contactverzoeken worden opgehaald verbeterd #1168

### Maintenance

- Betere beveiliging Elasticsearch en Enterprisesearch-endpoints zodat overige elastic functionaliteit is afgeschermd #1167

## v1.0.1

### Warnings and deployment notes

Deze versie bevat een bugfix voor v1.0.0 waarbij het tonen van het klantbeeld van een onderneming niet goed ging wanneer er geen contactgegevens van deze klant bekend zijn in het default register, maar wel in een secundair register.

### What's Changed

- fix for PC-1193 (Dimpact) #1118
- fix for PC-1218 (Dimpact) #1119

## v1.0.0

### Warnings and deployment notes

De configuratie van de registers voor klantinteracties en zaken is sterk gewijzigd. Het is nu mogelijk om registers aan elkaar te koppelen. Per samenstelling van registers dient er een `systeem` geconfigureerd te worden. De variabelen rondom een specifiek systeem heeft steeds REGISTERS`index` als prefix. Voor de esuite (in combinatie met de podiumd-adapter) zijn de variabelnamen anders dan voor OpenKlant2 / OpenZaak. Van precies 1 systeem moet worden aangegeven dat het het default systeem is.

Dit houdt in dat dit systeem gebruikt wordt voor contactmomenten en contactverzoeken die niet gekoppeld zijn aan een zaak, en dat dit het systeem is waar de klantinformatie primair wordt opgeslagen.

(Release bevat ook een uitgebreid configuratievoorbeeld met OpenKlant2/OpenZaak + eSuite/PodiumD-Adapter.)

## v0.7.1

### New features

- Verbetering koppeling met BRP proxy: mogelijkheid om per type zoekopdracht een andere header (bv doelbinding) mee te geven. Zie documentatie.
- PC-1108: custom headers per type zoekopdracht in haalcentraal #1089

## v0.7.0

### New features

- Koppeling met BRP/KvK proxies door middel van custom headers. Zie [documentatie](https://kiss-klantinteractie-servicesysteem.readthedocs.io/en/v0.7.0/installation/configuratie.html#gebruik-kiss-met-proxy-voor-brp-kvk-bijvoorbeeld-iconnect).
- Verbeterde styling van VACs en kennisartikelen (zie [PC-909](https://dimpact.atlassian.net/browse/PC-909)

## v0.6.1

### Maintenance

- vulnerability patches
- buffer json content bij het aanmaken van vacs, zodat dit werkt als je rechtstreeks met de objectenregistratie koppelt
- uitgebreidere logging

## v0.6.0

### New features

- Herontwerp Contactverzoek invoeren #705
- Verplaatsen van OnderwerpLinks #805
- Genereren KISS mockup content [interne data] #977
- Versienummer zichtbaar in de website #978
- Anonieme Contactverzoeken zoeken #810
- Klantcontacten bij een Zaak tonen #809
- Soorten digitaal adres aanpassen naar gebruik enums #891
- Tonen van digitale adressen aanpassen naar gebruik enums #974
- Digitale adressen opslaan conform validatieregels OK2 #939
- Partij-identificatoren aanpassen naar gebruik enums #890
- Contactverzoekformuliertjes / vragensetjes óók voor groepen #954
- Pagesize meegeven bij ophalen contactverzoeken en contactmomenten #896
- VAC items vindbaar maken voor beheer #1004
- VAC items toevoegen #1005
- VAC item bewerken #1006
- VAC beheerfunctionaliteit kunnen verbergen #1007
- VAC item verwijderen #1008
- Kan geen tekst selecteren binnen harmonica componenten #945
- Contactverzoek voor een Medewerker moet gemaild kunnen worden #817
- title gebruiken ipv eerste in headings bij tonen webpagina in zoekresultaten. #999

### Warnings and deployment notes

Zie installatiehandleiding voor instructies

- Minimale lengte van secrets is verhoogd van 16 naar 32 tekens
- Nieuwe Environment Variabelen nodig voor functionaliteit omtrent beheer van VACs:
  `VAC_OBJECTEN_BASE_URL`, `VAC_OBJECT_TYPE_URL`, `VAC_OBJECT_TYPE_VERSION`,
  `VAC_OBJECTEN_TOKEN` en `USE_VACS`.
- Nieuwe Environment Variabele om contactverzoeken voor medewerkers te kunnen mailen:
  `USE_MEDEWERKEREMAIL`

### Bugfixes

- Afdwingen max token lifetime van /api/contactmomentendetails #915
- Duidelijker paginering in /api/contactmomentendetails #914
- Verschillen objecttypes met community concepts wegwerken #986
- Kommafix m.b.v. afdelingsnamen (PC-652)
- E-mailadres komt niet meer mee in contactverzoek naar e-Suite (PC-884)
- Contactverzoeken zoeken gaat niet goed om met paginering in response (PC-911)

### Maintenance

- Upgrade vue from 3.4.31 to 3.5.13 (PR #928, #966, #1031)
- 3 vulnerabilities in the nuget dependencies of this project (PR #927, #1031)
- Upgrade ckeditor5 from 42.0.0 to 44.1.0 (PR #929, #963, #964, #1031)
- Upgrade pinia from 2.1.7 to 2.3.1 (PR #930, #967, #1031)
- Upgrade vue-router from 4.4.0 to 4.5.0 (PR #931, #1031)
- Upgrade dompurify from 2.5.5 to 3.2.3 (PR #965, #1031)
- Upgrade .Net naar v8 (PR #968)

## v0.5.2

### Maintenance

- PC-765 update vulnerable/obsolete dependencies from 0.5.x by @felixcicatt in #1029

## v0.5.1

### Features

- Verschillen in de objecttypes met community concepts weggewerkt #986
