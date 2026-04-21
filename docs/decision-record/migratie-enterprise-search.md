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

## Migratieaanpak

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

## Migratiestappen

### Stap 0: Huidige query template exporteren

**Wat:** Voordat er iets verwijderd wordt, moet het volledige `search_explain` response worden opgeslagen — dit bevat alle Relevance Tuning veldgewichten.

**Hoe:** Roep `search_explain` aan op de huidige omgeving en sla het complete response op (zowel `query_string` als `query_body`).

### Stap 1: `search_explain` vervangen door statisch/configureerbaar template

**Wat:** De `search_explain` endpoint retourneert een `multi_match` query met veld-specifieke gewichten. Dit is de **enige** reden dat Enterprise Search runtime wordt aangeroepen.

**Hoe:**
- Vervang `useQueryTemplate()` in `src/features/search/service.ts` (regels 167-212) door een statisch query-template of een BFF config-endpoint
- Verwijder `Kiss.Bff/Extern/EnterpriseSearch/EnterpriseSearchProxyConfig.cs`
- Verwijder de Enterprise Search route uit `Kiss.Bff/Config/ProxyConfig.cs`
- Verwijder `services.AddEnterpriseSearch(...)` uit `Kiss.Bff/Program.cs`

**Aandachtspunten:**
- Relevance Tuning was voorheen instelbaar via Kibana UI. Na migratie moeten gewichten in code/config aangepast worden.
- Het template bevat ook de lijst van doorzoekbare indices — deze moet na migratie uit configuratie komen.

### Stap 2: Meta-engine vervangen door Elasticsearch index alias

**Wat:** De `kiss-engine` meta-engine aggregeert alle bron-engines.

**Hoe:**
- Maak een Elasticsearch alias (bijv. `kiss-search`) die naar alle relevante indices wijst
- OF bied een config-gestuurde lijst van indices aan via het BFF
- De `useSources()` functie leidt beschikbare indices af uit het template; na migratie moet deze lijst uit configuratie komen

### Stap 3: Enterprise Search Web Crawler vervangen door Open Web Crawler

**Wat:** Websites worden gecrawld via de ingebouwde crawler van Enterprise Search, beheerd door KISS-Elastic-Sync.

**Hoe:**
- Neem de [Elastic Open Web Crawler](https://github.com/elastic/crawler) in gebruik (Docker-based, CLI)
- Configureer schrijven naar reguliere ES indices (bijv. `kiss-crawl-{domein}`)
- Draai via K8s CronJob

**Schemaverschillen:**

| Enterprise Search veld | Open Crawler veld | Status |
|---|---|---|
| `body_content` | `body` | **Afwijkend** — mapping nodig |
| `title` | `title` | Gelijk |
| `headings` | `headings` | Gelijk |
| `url` | `url` | Gelijk |
| `domains` | `domains` | Gelijk |
| `meta_description` | `meta_description` | Gelijk |
| N/A | `last_crawled_at` | Nieuw veld |

**Oplossing voor schemaverschillen:**
- Gebruik een [ingest pipeline](https://github.com/elastic/crawler/blob/main/docs/features/INGEST_PIPELINES.md) om `body` → `body_content` te mappen
- Voeg `object_bron` veld toe via ingest pipeline (op domein-URL zetten) zodat filteraggregaties blijven werken
- Maak custom index mappings aan met `.enum` keyword sub-velden

### Stap 4: KISS-Elastic-Sync updaten

**Wat:** [KISS-Elastic-Sync](https://github.com/Klantinteractie-Servicesysteem/KISS-Elastic-Sync) maakt momenteel engines, meta-engines en crawlers aan via de Enterprise Search API.

**Hoe:**
- Verwijder alle Enterprise Search API-aanroepen
- Voor **websites**: integreer Open Crawler CLI of deploy apart als K8s CronJob
- Voor **gestructureerde bronnen**: blijf direct naar ES indices indexeren
- Beheer index aliases ter vervanging van de meta-engine

**Let op:** Dit is een apart repository en waarschijnlijk het grootste werkpakket.

### Stap 5: `indices_boost` en indexreferenties bijwerken

**Wat:** De frontend gebruikt `indices_boost: [{ ".ent-search*": 1 }, { "*": 10 }]` om crawlerresultaten lager te ranken.

**Hoe:** Wijzig naar het nieuwe crawler index-patroon, bijv. `[{ "kiss-crawl-*": 1 }, { "*": 10 }]`

### Stap 6: Frontend veldmappings bijwerken

**Wat:** `mapResult()` in `service.ts` leest `_source.body_content`. De Open Crawler gebruikt `body`.

**Hoe:**
- Als ingest pipeline `body` → `body_content` mapt: geen frontend-wijziging nodig
- Anders: `obj?._source?.body_content ?? obj?._source?.body` in `mapResult`

### Stap 7: Enterprise Search infrastructuur verwijderen

- Verwijder `Kiss.Bff/Extern/EnterpriseSearch/EnterpriseSearchProxyConfig.cs`
- Verwijder env vars: `ENTERPRISE_SEARCH_BASE_URL`, `ENTERPRISE_SEARCH_PRIVATE_API_KEY`, `ENTERPRISE_SEARCH_PUBLIC_API_KEY`, `ENTERPRISE_SEARCH_ENGINE`
- Werk Helm chart bij: `values.yaml`, `values.schema.json`, `configmap.yaml`, `secret.yaml`
- Werk documentatie bij

### Stap 8: Tests bijwerken

- Verwijder `Kiss.Bff.Test/EnterpriseProxyConfigTests.cs`
- Werk `Kiss.Bff.Test/ElasticsearchControllerTests.cs` bij indien indexpatronen wijzigen
- Voeg tests toe voor het nieuwe query template config-endpoint

## Risico's en overwegingen

| Risico | Mitigatie |
|---|---|
| **Verlies van Relevance Tuning UI** | Exporteer huidige gewichten vóór migratie; overweeg een beheer config-bestand |
| **Open Crawler is nog in beta** | Het is de officiële vervanging van Elastic; functionaliteit dekt KISS-behoeften |
| **Schemaverschillen** | ES ingest pipelines om Open Crawler output te normaliseren |
| **KISS-Elastic-Sync is apart repo** | Gecoördineerde release nodig |
| **Downtime tijdens migratie** | Beide systemen parallel draaien tijdens transitie |
