# Configuratie van Elasticsearch voor KISS

Met behulp van de [Kiss.Elastic.Sync tool](../../README.md#elastic-sync) — onderdeel van deze repository — is het mogelijk om een aantal gestructureerde bronnen doorzoekbaar te maken vanuit KISS.

## Ondersteunde bronnen

De [Kiss.Elastic.Sync tool](../../README.md#elastic-sync) ondersteunt de volgende bronnen:

| Bron                      | Argument          | Beschrijving                                                                |
| ------------------------- | ----------------- | --------------------------------------------------------------------------- |
| SDG Producten             | _(geen argument)_ | Synchroniseert producten uit de Objects API op basis van het SDG-objecttype |
| VAC                       | `vac`             | Synchroniseert Vraag/Antwoord-combinaties uit de Objects API                |
| Medewerkers (Smoelenboek) | `smoelenboek`     | Synchroniseert medewerkers uit de Objects API of PodiumD Adapter            |
| SharePoint                | `sharepoint`      | Synchroniseert een SharePoint-site inclusief alle subsites                  |

## Crawler

Het doorzoeken van een website binnen KISS wordt mogelijk door de website te crawlen met de [Elastic Open Crawler](https://www.elastic.co/guide/en/elasticsearch/reference/current/es-connectors-crawler.html). De crawler draait als een Kubernetes CronJob en schrijft de gecrawlde pagina's rechtstreeks naar een Elasticsearch-index. Configuratie vindt plaats via de Helm-chart, onder `settings.syncJobs.website`. Het is aan te raden om overleg te hebben met uw websitebeheerder over het finetunen van de crawler. Mogelijk zijn er aanpassingen nodig in uw robots.txt, is het raadzaam een KISS-specifieke sitemap.xml op te stellen of zijn er aanvullende filterinstellingen nodig.

## Syncen van bronnen

De sync tool schrijft elke bron naar een eigen Elasticsearch-index (patroon: `search-<bronnaam>`). Bij de eerste synchronisatie worden de volgende velden toegevoegd aan de index:

- `object_bron`
- `object_meta`
- De velden die bij de bron horen, voorafgegaan door de naam van de bron (bijv. `VAC.trefwoorden.trefwoord`)

## Relevance Tuning

KISS doorzoekt alle indices die overeenkomen met het patroon `search-*`. De zoekresultaten worden bepaald door de veldweging in de Elasticsearch-query. Via Kibana kunt u de index-instellingen en geïndexeerde documenten bekijken. De veldgewichten zelf liggen vast in de broncode en zijn niet via Kibana aan te passen.
