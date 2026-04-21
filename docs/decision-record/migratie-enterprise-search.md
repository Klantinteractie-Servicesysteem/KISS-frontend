# Migratie: Enterprise Search → Native Elasticsearch (ES9)

## Aanleiding

Enterprise Search (App Search + Workplace Search) wordt niet meer meegeleverd in Elasticsearch 9. KISS maakt op dit moment op drie manieren gebruik van Enterprise Search:

1. **`search_explain` API** — de frontend haalt via `POST /api/as/v1/engines/kiss-engine/search_explain` een Elasticsearch query-template op dat de Relevance Tuning veldgewichten bevat. De daadwerkelijke zoekopdrachten gaan al rechtstreeks naar Elasticsearch.
2. **Meta-engine (`kiss-engine`)** — voegt crawler-engines (`.ent-search*` indices) en gestructureerde-bron-index-engines samen tot één doorzoekbaar geheel.
3. **Enterprise Search Web Crawler** — beheerd via KISS-Elastic-Sync, crawlt websites naar `.ent-search*` indices.

## Huidige dataflow

```
Frontend                          BFF (.NET)                       Elastic Stack
────────                          ──────────                       ─────────────
1. POST search_explain            → YARP proxy (Bearer auth)     → App Search API
   {"query":"{{query}}"}          ← ES query template (DSL)      ←
                                    (met Relevance Tuning weights)

2. Frontend past template aan:
   - verwijdert from/size/_source
   - voegt indices_boost toe (.ent-search*:1, *:10)
   - voegt suggest-blok toe
   - vervangt {{query}} door echte zoekterm
   - voegt filterclausules toe (object_bron, domains)

3. POST /{indices}/_search        → ElasticsearchController      → Elasticsearch
   {aangepaste ES query DSL}        (rol-gebaseerde veldfilter)  ←
```

## Migratie als kans voor vereenvoudiging

KISS maakt grotendeels gebruik van de **standaardinstellingen van App Search** — met één bekende uitzondering:

- **Precision Tuning** is actief geëxperimenteerd. De App Search standaardinstelling is **2** (hoge recall, lage precisie), maar klanten uitten onvrede over de brede, weinig trefzekere resultaten. Er is getest met instelling **8** (preciezer, minder ruis). De huidige live-instelling is niet zeker — **exporteer altijd het `search_explain` response vóór de migratie begint** (Stap 0), want dat weerspiegelt de werkelijke instelling.
- **Relevance Tuning** veldgewichten zijn waarschijnlijk (bijna) ongewijzigd.
- **`mapping.json`** en **`engine.json`** in KISS-Elastic-Sync zijn afgeleid van App Search interne mappings en ondersteunen alle elf precisieniveaus via zeven sub-velden per string-veld.
- **`field.json`** regelt completion-velden en is overgenomen van App Search defaults.

De precisie-experimenten laten zien dat dit een instelling is die er toe doet voor gebruikers. Na de migratie moet precisie een **expliciete, instelbare configuratie** zijn — niet iets dat verborgen zit in een geëxporteerde query template. Dit is een sterk argument voor een Relevance Tuning UI in KISS zelf (Optie C).

**Aanbeveling:** Doe de kernmigratie eerst met minimale wijzigingen (huidige `search_explain` output vastleggen en reproduceren in native ES). Behandel precisie, veldgewichten en querystructuur daarna als iets om iteratief op te verbeteren met echte zoekresultaten als maatstaf.



Alle Enterprise Search afhankelijkheden vervangen door native Elasticsearch-functionaliteit en de Elastic Open Web Crawler.

### Architectuurbeslissing: query-constructie frontend vs backend

Op dit moment bouwt de frontend de volledige Elasticsearch query DSL op en stuurt deze via een passthrough-endpoint (`api/elasticsearch/{index}/_search`) naar Elasticsearch. Dit is een goed moment om dit te herzien.

#### Optie A: Query-constructie in de frontend houden (minimale wijziging)

Alleen `useQueryTemplate()` vervangen door een statisch template. De rest van de frontend-logica blijft ongewijzigd.

**Voordelen:** Minimale migratie-inspanning, geen nieuwe BFF-endpoints nodig.
**Nadelen:** Elasticsearch query DSL zichtbaar in de browser, indexnamen en veldgewichten lekken naar de client, een geauthenticeerde gebruiker kan willekeurige ES queries uitvoeren via het passthrough-endpoint.

#### Optie B: Query-constructie naar de backend verplaatsen (aanbevolen)

De frontend stuurt eenvoudige zoekparameters (`query`, `page`, `filters`). De BFF bouwt de ES query, voert deze uit, en retourneert een vereenvoudigde response.

**Nieuwe BFF-endpoints:**
- `POST /api/search` — globaal zoeken (vervangt de huidige directe ES calls)
- `GET /api/search/sources` — beschikbare bronnen ophalen (vervangt `useSources()`)
- `POST /api/search/medewerkers` — medewerkers zoeken (vervangt `searchMedewerkers()`)

**Voordelen:**
- ES query DSL niet meer zichtbaar in de browser (beveiliging)
- Open ES proxy verwijderd — gebruikers kunnen geen willekeurige queries meer uitvoeren
- Query template, veldgewichten, indexlijst beheerd op de server (geen frontend-deploy nodig voor tuning)
- Eenvoudiger frontend-code
- Makkelijker om caching, rate limiting, query logging toe te voegen

**Nadelen:** Meer migratie-inspanning (nieuwe controller + service), backend-deploy nodig voor query-wijzigingen.

**Aanbeveling:** Optie B — de migratie is het natuurlijke moment om de architectuur te verbeteren.

### Relevance Tuning UI in KISS zelf bouwen

Los van de keuze voor Optie A of B vervalt de Kibana Relevance Tuning UI. Admins kunnen dan niet meer via een UI de veldgewichten aanpassen.

Naast de veldgewichten (per-field boosts) maakt KISS ook gebruik van **Precision Tuning**: een schuifregelaar van 1 tot 11 die de balans tussen brede en exacte zoekopdrachten bepaalt. De instelling bepaalt welke sub-velden (`.prefix`, `.delimiter`, `.joined`, `.stem`) worden meegenomen in de `multi_match` query, en hoe streng de term-matching is (fuzzy matching aan/uit, `minimum_should_match`). De huidige instelling is impliciet opgenomen in het `search_explain` response.

**Optionele aanvulling:** Bouw een eenvoudige beheerpagina in KISS met:
- Een lijst van velden met boost-waarden (vervanger van Relevance Tuning)
- Een precision-instelling (bijv. een keuze `Breed / Normaal / Strikt` voor niet-technische gebruikers)

Beide worden opgeslagen in de database en bij runtime gebruikt in de `multi_match` query. Dit is het meest logisch als uitbreiding op Optie B (BFF bouwt de query al).

**Aanbeveling:** Plan dit als een **follow-on feature** na de kernmigratie. Voor de initiële migratie: exporteer de huidige gewichten en precision-instelling uit `search_explain` en sla ze op als configuratie.

## Migratiestappen

### Stap 0: Huidige query template exporteren

**Wat:** Voordat er iets verwijderd wordt, moet het volledige `search_explain` response worden opgeslagen — dit bevat alle Relevance Tuning veldgewichten.

**Hoe:** Roep `search_explain` aan op de huidige omgeving en sla het complete response op (zowel `query_string` als `query_body`).

> **Goed nieuws:** De gestructureerde bronindices (kennisbank, vac, smoelenboek, sharepoint) zijn al **gewone Elasticsearch indices**. KISS-Elastic-Sync indexeert al direct via de `_bulk` API en past daarvoor een rijke custom mapping toe (`mapping.json` met Dutch analyzers, `.enum` / `.stem` / `.prefix` sub-fields, `_completion` voor autocomplete). Enterprise Search wordt alleen gebruikt om deze indices te registreren in de meta-engine — dat stukje vervalt gewoon.

### Stap 1: `search_explain` vervangen door statisch/configureerbaar template

**Wat:** De `search_explain` endpoint retourneert een `multi_match` query die gebruik maakt van de `.stem`, `.prefix`, `.delimiter`, `.joined` sub-velden die KISS-Elastic-Sync via `mapping.json` aanmaakt. Dit is de **enige** reden dat Enterprise Search runtime wordt aangeroepen.

**Hoe:**
- Vervang `useQueryTemplate()` in `src/features/search/service.ts` (regels 167-212) door een statisch query-template of een BFF config-endpoint
- Verwijder `Kiss.Bff/Extern/EnterpriseSearch/EnterpriseSearchProxyConfig.cs`
- Verwijder de Enterprise Search route uit `Kiss.Bff/Config/ProxyConfig.cs`
- Verwijder `services.AddEnterpriseSearch(...)` uit `Kiss.Bff/Program.cs`

**Aandachtspunten:**
- Relevance Tuning was voorheen instelbaar via Kibana UI. Na migratie moeten gewichten in code/config aangepast worden.
- Het template bevat ook de lijst van doorzoekbare indices — deze moet na migratie uit configuratie komen.

### Stap 2: Meta-engine vervangen door Elasticsearch index alias

**Wat:** De `kiss-engine` meta-engine aggregeert alle bron-engines. Na migratie worden dit gewoon aliassen.

**Hoe:**
- Maak een Elasticsearch alias (bijv. `kiss-search`) die naar alle relevante indices wijst
- OF bied een config-gestuurde lijst van indices aan via het BFF

### Stap 3: Enterprise Search Web Crawler vervangen door Open Web Crawler

**Wat:** Websites worden gecrawld via de ingebouwde crawler van Enterprise Search. De crawler zet documenten in `.ent-search-engine-documents-*` hidden indices.

**Hoe:**
- Neem de [Elastic Open Web Crawler](https://github.com/elastic/crawler) in gebruik (Docker-based, CLI)
- Configureer schrijven naar reguliere ES indices (bijv. `kiss-crawl-{domein}`)
- Pas de **volledige `mapping.json`** toe op de crawler index vóór de eerste crawl (dit zorgt voor de `.enum`, `.stem` sub-velden die de query template nodig heeft)
- Gebruik een ingest pipeline voor: `body` → `body_content` (Open Crawler gebruikt `body`) en `object_bron` instellen op het domein
- Draai via K8s CronJob

**Schemaverschillen:**

| Enterprise Search veld | Open Crawler veld | Oplossing |
|---|---|---|
| `body_content` | `body` | Ingest pipeline: rename |
| `object_bron` | ontbreekt | Ingest pipeline: set op domein-URL |
| `title`, `headings`, `url`, `domains` | Gelijk | Geen actie |

### Stap 4: KISS-Elastic-Sync updaten

**Wat:** [KISS-Elastic-Sync](https://github.com/Klantinteractie-Servicesysteem/KISS-Elastic-Sync) roept voor gestructureerde bronnen al direct de ES `_bulk` API aan — dat blijft intact. Wat vervalt:
- `ElasticEnterpriseSearchClient.cs` (hele klasse)
- De aanroepen `AddIndexEngineAsync()`, `AddDomain()`, `CrawlDomain()` in `Program.cs`
- De `domain`-command-afhandeling → vervangen door Open Crawler configuratie

**Toevoegen:** beheer van index aliases ter vervanging van de meta-engine.

**Let op:** Dit is een apart repository en vereist een gecoördineerde release.

### Stap 5: `indices_boost` bijwerken

Wijzig van `.ent-search*` naar het nieuwe crawler index-patroon, bijv. `kiss-crawl-*`.

### Stap 6: Frontend veldmappings bijwerken

`mapResult()` in `service.ts` leest `body_content`. Als de ingest pipeline `body` → `body_content` mapt, is geen frontend-wijziging nodig. Anders: `obj?._source?.body_content ?? obj?._source?.body`.

### Stap 7: Enterprise Search infrastructuur verwijderen

- Verwijder `Kiss.Bff/Extern/EnterpriseSearch/EnterpriseSearchProxyConfig.cs`
- Verwijder env vars: `ENTERPRISE_SEARCH_BASE_URL`, `ENTERPRISE_SEARCH_PRIVATE_API_KEY`, `ENTERPRISE_SEARCH_PUBLIC_API_KEY`, `ENTERPRISE_SEARCH_ENGINE`
- Werk Helm chart bij: `values.yaml`, `values.schema.json`, `configmap.yaml`, `secret.yaml`
- Verwijder de Enterprise Search K8s service
- Werk documentatie bij

### Stap 8: Tests bijwerken

- Verwijder `Kiss.Bff.Test/EnterpriseProxyConfigTests.cs`
- Werk `Kiss.Bff.Test/ElasticsearchControllerTests.cs` bij indien nodig
- Voeg tests toe voor het nieuwe query template config-endpoint

## Risico's en overwegingen

| Risico | Mitigatie |
|---|---|
| **Verlies van Relevance Tuning UI** | Exporteer huidige gewichten vóór migratie; overweeg een beheer config-bestand |
| **Open Crawler is nog in beta** | Het is de officiële vervanging van Elastic; functionaliteit dekt KISS-behoeften |
| **Schemaverschillen** | ES ingest pipelines om Open Crawler output te normaliseren |
| **KISS-Elastic-Sync is apart repo** | Gecoördineerde release nodig |
| **Downtime tijdens migratie** | Beide systemen parallel draaien tijdens transitie |
| **App Search defaults klakkeloos overnemen** | Vermijd het volledig nabootsen van de App Search complexiteit in native ES; gebruik de migratie als kans om te vereenvoudigen |
