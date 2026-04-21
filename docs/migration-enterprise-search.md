# Migration Plan: Enterprise Search → Native Elasticsearch (ES9)

> **Context:** Enterprise Search (App Search + Workplace Search) is removed in Elasticsearch 9. This document describes the current usage and the migration path for KISS.

## Table of Contents

- [Current Architecture](#current-architecture)
- [What Enterprise Search Does for KISS](#what-enterprise-search-does-for-kiss)
- [Migration as an Opportunity to Simplify](#migration-as-an-opportunity-to-simplify)
- [Architecture Decision: Frontend vs Backend Query Construction](#architecture-decision-frontend-vs-backend-query-construction)
- [Migration Tasks](#migration-tasks)
- [Detailed File Changes](#detailed-file-changes)
- [Open Crawler Setup Guide](#open-crawler-setup-guide)
- [Ingest Pipeline Reference](#ingest-pipeline-reference)
- [Risks & Considerations](#risks--considerations)
- [Execution Order](#execution-order)

---

## Current Architecture

### Data Flow

```
Frontend (Vue.js)                 BFF (.NET YARP)                  Elastic Stack
─────────────────                 ───────────────                  ─────────────
1. POST search_explain            → YARP proxy (Bearer auth)     → Enterprise Search
   {"query":"{{query}}"}          ← ES query template            ←  App Search API
                                    (with Relevance Tuning)         (search_explain)

2. Frontend modifies template:
   - removes from/size/_source
   - adds indices_boost
   - adds suggest block
   - replaces {{query}} with real search term
   - wraps in bool filter for source/domain selection

3. POST /{indices}/_search        → ElasticsearchController      → Elasticsearch
   {modified ES query DSL}          → ElasticsearchService       ← (basic auth)
                                    → role-based field removal
```

### Key Components

| Component | File(s) | Purpose |
|---|---|---|
| Frontend search service | `src/features/search/service.ts` | Fetches template from App Search, executes search queries against ES |
| Enterprise Search proxy | `Kiss.Bff/Extern/EnterpriseSearch/EnterpriseSearchProxyConfig.cs` | YARP proxy config with Bearer auth |
| Proxy route registration | `Kiss.Bff/Config/ProxyConfig.cs` (lines ~149-183) | Locks down to only `POST search_explain` |
| Elasticsearch controller | `Kiss.Bff/Extern/Elasticsearch/ElasticsearchController.cs` | Proxies search to ES with field filtering |
| Elasticsearch service | `Kiss.Bff/Extern/Elasticsearch/ElasticsearchService.cs` | Role-based field removal for Kennisbank users |
| DI registration | `Kiss.Bff/Program.cs` | `AddEnterpriseSearch()` + ES HttpClient setup |
| Proxy tests | `Kiss.Bff.Test/EnterpriseProxyConfigTests.cs` | Validates only search_explain is exposed |
| ES controller tests | `Kiss.Bff.Test/ElasticsearchControllerTests.cs` | Tests field removal and proxy behavior |
| ECK Enterprise CRD | `docs/scripts/elastic/eck/templates/enterprise.yaml` | K8s Enterprise Search deployment (legacy, not maintained) |
| Sync tool | [KISS-Elastic-Sync](https://github.com/Klantinteractie-Servicesysteem/KISS-Elastic-Sync) (separate repo) | Creates engines, crawlers, syncs structured data |

### Configuration Environment Variables

| Variable | Purpose | Used By |
|---|---|---|
| `ENTERPRISE_SEARCH_BASE_URL` | Enterprise Search server URL | BFF YARP proxy + KISS-Elastic-Sync |
| `ENTERPRISE_SEARCH_PRIVATE_API_KEY` | Bearer token for App Search API | BFF YARP proxy + KISS-Elastic-Sync |
| `ENTERPRISE_SEARCH_PUBLIC_API_KEY` | Public API key (stored as K8s secret) | Not used in BFF directly |
| `ENTERPRISE_SEARCH_ENGINE` | Meta-engine name (e.g., `kiss-engine`) | KISS-Elastic-Sync |
| `ELASTIC_BASE_URL` | Elasticsearch cluster URL | BFF HttpClient (stays) + KISS-Elastic-Sync |
| `ELASTIC_USERNAME` | ES basic auth username | BFF HttpClient (stays) + KISS-Elastic-Sync |
| `ELASTIC_PASSWORD` | ES basic auth password | BFF HttpClient (stays) + KISS-Elastic-Sync |
| `ELASTIC_EXCLUDED_FIELDS_KENNISBANK` | Fields hidden from Kennisbank users | ElasticsearchService (stays) |

### Elasticsearch Indices Used

| Index Pattern | Source | Created By | Index type |
|---|---|---|---|
| `.ent-search-engine-documents-engine-{name}` | Website crawl results | Enterprise Search Crawler | Hidden `.ent-search*` index |
| `kennisbank` | SDG Producten / Kennisartikelen | KISS-Elastic-Sync (direct `_bulk`) | Regular ES index |
| `smoelenboek` | Medewerkers | KISS-Elastic-Sync (direct `_bulk`) | Regular ES index |
| `vac` | VAC's | KISS-Elastic-Sync (direct `_bulk`) | Regular ES index |
| `sharepoint-*` | SharePoint pages | KISS-Elastic-Sync (direct `_bulk`) | Regular ES index |

> **Key insight:** All structured source indices are already **native Elasticsearch indices**. KISS-Elastic-Sync only calls the Enterprise Search API to register them into the meta-engine (`AddIndexEngineAsync`) and to manage crawler domains. The actual indexing goes direct to ES via `_bulk`.

### Index Mapping (mapping.json)

KISS-Elastic-Sync applies a rich custom mapping when it creates a structured source index. This mapping is central to how search works — it must be preserved in the migration.

```
Dynamic template applies to ALL string fields, giving each field these sub-fields:
  .enum        — keyword, used for aggregations (object_bron.enum, domains.enum)
  .stem        — Dutch stemmer (nl-stem-filter)
  .prefix      — edge-ngram for prefix search
  .delimiter   — word delimiter (split on case change, numerics, etc.)
  .joined      — bigram for phrase proximity
  .date        — date parsing
  .float       — numeric parsing

Custom Dutch analyzers: iq_text_base, iq_text_stem, iq_text_delimiter, i_text_bigram, q_text_bigram
Dutch stop words + stemmer built in.

Special fields:
  _completion  — completion suggester (autocomplete). Source fields copy_to this via field.json.
  headings     — also copies to _completion (engine.json, applied to crawler indices)
```

The `search_explain` query from App Search leverages all these sub-fields (`.stem`, `.prefix`, `.delimiter`, `.joined`) via a `multi_match` with per-field boosts. **This mapping is already applied by KISS-Elastic-Sync** and is the same format as what App Search uses internally — which is why `search_explain` works against native indices.

### Document Structure (KissEnvelope)

Every structured-source document written by KISS-Elastic-Sync has this shape:

```json
{
  "title": "Parkeervergunning aanvragen",
  "object_meta": "U kunt een parkeervergunning aanvragen via...",
  "object_bron": "Kennisbank",
  "url": "https://...",
  "Kennisbank": { ...full source object... }
}
```

`object_bron` is the source name (= index name, capitalised). The full object is nested under the source name, which is why field paths like `Kennisbank.vertalingen.deskMemo` are used in `ELASTIC_EXCLUDED_FIELDS_KENNISBANK`.

The `CompletionFields` per source client (e.g., `["vertalingen.tekst", "vertalingen.titel"]` for SDG) drive which nested fields get `copy_to: _completion` in the index mapping.

---

## What Enterprise Search Does for KISS

### 1. Query Template via `search_explain` (Runtime)

The **only runtime call** to Enterprise Search. The frontend sends:

```json
POST /api/as/v1/engines/kiss-engine/search_explain
{ "query": "{{query}}" }
```

And receives back an Elasticsearch query DSL template containing:
- A `multi_match` query across all indexed fields with per-field `^weight` boosts from Relevance Tuning
- The multi_match leverages the `.stem`, `.prefix`, `.delimiter`, `.joined` sub-fields from `mapping.json`
- A list of indices (derived from the meta-engine's source engines)

The frontend then:
1. Strips `from`, `size`, `_source`
2. Adds `indices_boost: [{ ".ent-search*": 1 }, { "*": 10 }]` (crawler results ranked lower)
3. Adds `suggest` block for autocomplete on `_completion` field
4. Replaces `{{query}}` with the real search term
5. Adds filter clauses for source selection (`object_bron.enum`, `domains.enum`)
6. Sends this modified query directly to Elasticsearch

### 2. Meta-Engine `kiss-engine` (Configuration)

Aggregates multiple source engines into one searchable unit. KISS-Elastic-Sync creates:
- An **index engine** per structured source (e.g., `engine-kennisbank` wrapping the `kennisbank` index)
- A **crawler engine** per website domain (e.g., `engine-engine-crawler` with `.ent-search*` index)
- The **meta-engine** `kiss-engine` linking all source engines

The frontend derives the list of searchable indices from the `search_explain` response's `query_string`.

### 3. Enterprise Search Web Crawler (Data Ingestion)

The KISS-Elastic-Sync tool manages crawlers via the Enterprise Search API:
- `domain https://www.example.nl` command sets up a crawler for a domain
- Crawled data lands in `.ent-search-engine-*` hidden indices
- Documents follow the [App Search crawler schema](https://www.elastic.co/guide/en/app-search/8.9/web-crawler-reference.html#web-crawler-reference-web-crawler-schema)

### 4. Relevance Tuning (Admin UI)

Configured in Kibana via the App Search Relevance Tuning page:
- Per-field weight configuration (e.g., `VAC.trefwoorden.trefwoord^3`)
- Controls which fields are searchable
- Changes are reflected in `search_explain` output

### 5. Precision Tuning (Admin UI)

Also configured in Kibana. Precision tuning is a slider from **1 to 11** that controls the tradeoff between precision (exact matches) and recall (broad matches):

| Level | Analyzers active | Fuzzy matching | Phrase matching |
|---|---|---|---|
| 1–2 | All (default, stem, prefix, delimiter, joined) | Yes | Partial — few terms need to match |
| 3–8 | All | Yes | Increasingly strict |
| 9 | Default, stem, joined (prefix disabled) | No | All terms, same field |
| 10 | Default, stem (delimiter/joined disabled) | No | All terms, same field |
| 11 | Default only (stem disabled) | No | Exact tokenized match |

In practice, the precision level controls **which sub-fields** from `mapping.json` are included in the `multi_match` query:
- Lower precision → includes `.prefix`, `.delimiter`, `.joined` (broad, forgiving)
- Higher precision → strips those sub-fields, uses only `.stem` and base (strict)

It also controls the `minimum_should_match` parameter and whether `fuzziness` is applied.

**The current precision setting is implicitly baked into the `search_explain` output.** Capturing the template captures the precision level as it exists today.

---

## Migration as an Opportunity to Simplify

KISS has largely stuck with **App Search defaults** throughout its setup — with one known exception:

- **Precision tuning** has been actively experimented with. The App Search default is **2** (high recall, low precision), but customers reported unhappiness with the broad results this produces. The setting has been tested at **8** (more precise, fewer false matches). The current live value is not certain — **always capture the current `search_explain` output before any migration work begins** (Step 0), as it reflects the actual precision in use.
- **Relevance tuning** field weights are probably close to defaults or were set once and never revisited.
- **`mapping.json`** and **`engine.json`** in KISS-Elastic-Sync are derived from App Search's internal mappings. They define seven sub-fields per string (`.enum`, `.stem`, `.prefix`, `.delimiter`, `.joined`, `.date`, `.float`) because App Search needs to support all eleven precision levels.
- **`field.json`** controls completion fields and was copied from App Search defaults.

The precision experimentation confirms that this setting **matters to users** and should be explicitly controlled after migration — not left as an implicit side effect of which query template was last exported.

### What could be simplified

| Area | Current state | Possible simplification |
|---|---|---|
| Sub-fields per string | 7 (`.enum`, `.stem`, `.prefix`, `.delimiter`, `.joined`, `.date`, `.float`) | `.enum` (aggregations) + `.stem` are likely sufficient; `.prefix` useful for prefix-search; `.delimiter`/`.joined` mainly matter at low precision levels |
| Analyzers | 5 custom Dutch analyzers + bigrams | Fewer analyzers if fewer sub-fields are used |
| `minimum_should_match` | Implicitly controlled by precision slider | Should be set explicitly after migration |
| Fuzzy matching | On at low precision levels, off at ≥9 | At precision 8 fuzzy is still on; make this an explicit config rather than implicit |
| Completion suggester | `_completion` field with `copy_to` from specific nested fields | Works well; keep unless autocomplete quality needs improving |
| `engine.json` for crawlers | Adds `_completion` + `headings` copy to hidden crawler index | Replicate only what's needed in the new crawler index mapping |

### Simplification might improve results

The App Search internal analyzers were designed for a general-purpose product across many languages. A simpler, purpose-built Dutch query (for example, a `multi_match` over `title`, `object_meta`, and a few nested source fields with Dutch stemming plus an explicit `minimum_should_match`) might deliver better relevance than the current auto-generated multi-analyzer query — especially for the specific types of municipal content KISS indexes.

The precision complaints from customers also point to a broader need: **the precision/recall tradeoff should be an explicit, tunable setting**, not something buried in a query template. This is a strong argument for the in-KISS Relevance Tuning UI (Option C), which could surface a precision slider or mode selector directly to admins.

**Recommendation:** Do the core migration with minimal changes first (capture the current `search_explain` output, reproduce its behavior in native ES). Once running, treat precision, field weights, and the query structure as things to iterate on with real search quality testing. Option B (backend query construction) makes this iteration much easier since query changes no longer require frontend deploys.

---

## Architecture Decision: Frontend vs Backend Query Construction

Currently, the frontend constructs full Elasticsearch query DSL and sends it directly to the `api/elasticsearch/{index}/_search` passthrough endpoint. This migration is a good opportunity to move query construction to the backend. Below is a comparison.

### Option A: Keep query construction in the frontend (current approach, minimal change)

The frontend continues to build Elasticsearch query DSL. The only change is replacing the `search_explain` call with a static/configurable template.

```
Frontend                           BFF                              Elasticsearch
────────                           ───                              ─────────────
builds full ES query DSL    →  passthrough proxy             →  executes query
                               (+ role-based field removal)
```

**Pros:**
- Minimal migration effort — only `useQueryTemplate()` changes
- No new BFF endpoints needed
- Frontend has full control over query features (filters, pagination, suggest)

**Cons:**
- Frontend is tightly coupled to Elasticsearch query DSL
- Full ES query DSL is exposed to the browser (visible in DevTools)
- Index names, field names, boost weights all leak to the client
- Any query change requires a frontend deploy
- The BFF "passthrough" pattern means an authenticated user can craft arbitrary ES queries

### Option B: Move query construction to the backend (recommended)

The frontend sends simple search parameters. The BFF constructs the ES query, executes it, and returns a simplified response.

```
Frontend                           BFF                              Elasticsearch
────────                           ───                              ─────────────
POST /api/search                                                    
{ query, page, filters }   →  builds ES query DSL           →  executes query
                               applies role-based filtering
                            ←  returns simplified response   ←
```

#### New BFF API Design

**Global Search:**
```
POST /api/search
Content-Type: application/json

{
  "query": "parkeervergunning",
  "page": 1,
  "pageSize": 10,
  "sources": ["VAC", "https://www.example.nl"]    // optional filter
}

Response:
{
  "results": [
    {
      "id": "abc123",
      "source": "VAC",
      "title": "Parkeervergunning aanvragen",
      "content": "U kunt een parkeervergunning aanvragen via...",
      "url": "https://www.example.nl/parkeervergunning",
      "jsonObject": { ... }
    }
  ],
  "totalPages": 5,
  "page": 1,
  "pageSize": 10,
  "suggestions": ["parkeervergunning", "parkeerboete"]
}
```

**Sources (for filter checkboxes):**
```
GET /api/search/sources

Response:
{
  "sources": [
    { "name": "VAC", "index": "search-vac" },
    { "name": "https://www.example.nl", "index": "kiss-crawl-example-nl" }
  ]
}
```

**Medewerkers Search:**
```
POST /api/search/medewerkers

{
  "query": "Jansen",
  "filterField": "Smoelenboek.afdelingen.afdelingnaam",
  "filterValue": "ICT"
}

Response:
{
  "results": [
    {
      "value": "Jan Jansen",
      "description": "Systeembeheerder bij ICT",
      "identificatie": "12345",
      "afdelingen": [...],
      "groepen": [...]
    }
  ]
}
```

#### What moves from frontend to backend

| Responsibility | Current (frontend) | Proposed (backend) |
|---|---|---|
| Query template + field weights | `useQueryTemplate()` calls Enterprise Search | Config file / appsettings |
| `indices_boost` | `service.ts` line 189 | `SearchService.cs` |
| `suggest` block | `service.ts` lines 190-199 | `SearchService.cs` |
| `{{query}}` substitution | `service.ts` line 85-88 | `SearchService.cs` |
| Filter clauses (object_bron, domains) | `service.ts` lines 96-124 | `SearchService.cs` |
| Pagination (from/size) | `service.ts` lines 82-83, 91-92 | `SearchService.cs` |
| Source aggregation query (BRON_QUERY) | `service.ts` lines 214-242 | `SearchService.cs` |
| Result mapping (ES hits → SearchResult) | `mapResult()` lines 13-33 | `SearchService.cs` |
| Suggestion extraction | `mapSuggestions()` lines 51-57 | `SearchService.cs` |
| Medewerker query construction | `searchMedewerkers()` lines 311-344 | `SearchService.cs` |
| Index list derivation | From `query_string` in template | From config |

**Pros:**
- Elasticsearch query DSL is no longer exposed to the browser
- Eliminates the open ES proxy (security improvement — users cannot craft arbitrary queries)
- Query template, field weights, index list all managed server-side (no frontend deploy for tuning)
- Cleaner frontend code — just simple fetch calls with business parameters
- Existing `ElasticsearchService` role-based filtering integrates naturally
- Easier to add features like caching, rate limiting, query logging

**Cons:**
- More migration effort upfront (new controller + service)
- Any frontend query change now requires a backend deploy (but this is standard practice)
- Need to define and maintain a BFF API contract

#### Implementation outline for Option B

**New files to create:**
```
Kiss.Bff/Extern/Search/SearchController.cs    — new REST endpoints
Kiss.Bff/Extern/Search/SearchService.cs       — query construction + ES calls
Kiss.Bff/Extern/Search/SearchModels.cs        — request/response DTOs
Kiss.Bff/Extern/Search/SearchConfig.cs        — query template + index config
```

**Changes to existing files:**
- `Kiss.Bff/Program.cs` — register new services, add config binding
- `src/features/search/service.ts` — replace ES query construction with simple API calls
- `src/features/search/types.ts` — no change (types already match proposed response)

**Files to remove (same as Option A):**
- `Kiss.Bff/Extern/EnterpriseSearch/EnterpriseSearchProxyConfig.cs`
- Enterprise Search YARP route from `ProxyConfig.cs`
- `Kiss.Bff.Test/EnterpriseProxyConfigTests.cs`

**Files to keep or deprecate:**
- `ElasticsearchController.cs` — keep for backward compatibility during transition, then remove
- `ElasticsearchService.cs` — the role-based field filtering logic moves into `SearchService`

#### Recommendation

**Option B is recommended.** The migration is the natural moment to fix the architectural issue of exposing raw ES query DSL to the browser. The extra effort is moderate (one new controller + service) and the security and maintainability benefits are significant.

### Option C: Build a Relevance Tuning UI in KISS itself

Regardless of whether Option A or B is chosen for query construction, the **loss of the Kibana Relevance Tuning UI** is a separate concern. Currently, admins use the App Search UI in Kibana to adjust per-field boosts. After migration this capability disappears.

An alternative is to build a basic relevance tuning interface inside KISS, allowing admins (or technically authorised users) to adjust field weights without touching config files or redeploying.

#### Scope

The minimum viable version would let admins edit a list of `{ field, boost }` pairs that are stored in the database and applied to the search query at runtime.

The query template generated from `search_explain` is a `multi_match` query with entries like:

```json
{
  "multi_match": {
    "query": "{{query}}",
    "fields": [
      "VAC.trefwoorden.trefwoord^3",
      "Kennisbank.vertalingen.titel^2",
      "body_content^1",
      ...
    ]
  }
}
```

A UI that reads and writes these `field^boost` values, stored in a simple DB table, is sufficient to replicate the core of Relevance Tuning.

**Precision tuning** is a second lever currently available in App Search. It is a slider from 1–11 that controls the tradeoff between broad (high recall) and strict (high precision) matching. It manifests in the query as:
- Which sub-fields (`.prefix`, `.delimiter`, `.joined`, `.stem`) are included in the `multi_match`
- The `minimum_should_match` value
- Whether `fuzziness` is applied

A KISS-native precision control could be as simple as a single numeric config value (1–11) that the BFF translates into the appropriate query structure — or a radio button group (`Breed` / `Normaal` / `Strikt`) for non-technical users.

#### What this would require

| Component | Work |
|---|---|
| DB table `search_field_weights` (field, boost, searchable) | Small |
| Admin page in KISS to list/edit weights | Medium |
| BFF reads weights from DB when building query | Small (natural fit for Option B) |
| Authorization: restrict to admin role | Small |

**This is most natural as an extension of Option B** — if the BFF is already building the query, reading weights from a DB table is a small additional step. With Option A (frontend builds the query), you'd need a config endpoint and the weights would still need to be fetched from somewhere.

#### Recommendation

Build the Relevance Tuning UI as a **follow-on feature** after the core migration is complete, not as a blocker. For the initial migration, export the current weights from `search_explain` and store them as config. The UI can be added later without changing the overall architecture.

If Option B is chosen, the existing `api/elasticsearch/{index}/_search` passthrough endpoint should be deprecated and eventually removed.

---

## Migration Tasks

### Task 1: Capture and Replicate the Query Template

**Priority: CRITICAL — must be done first, before removing anything**

1. Call `search_explain` on the running system:
   ```bash
   curl -X POST "https://<enterprise-search>/api/as/v1/engines/kiss-engine/search_explain" \
     -H "Authorization: Bearer <private-api-key>" \
     -H "Content-Type: application/json" \
     -d '{"query": "{{query}}"}'
   ```
2. Save the full response (both `query_string` and `query_body`)
3. Create a static query template from `query_body`:
   - Strip `from`, `size`, `_source` (frontend already does this)
   - This becomes the new baseline template

**Implementation depends on the chosen architecture option:**

- **Option A (frontend):** Store template as static JSON in `service.ts` or serve from BFF config endpoint
- **Option B (backend, recommended):** Store template as `appsettings.json` or a JSON config file loaded by `SearchService`. Expose admin-editable config for field weights.

### Task 2: Replace Web Crawler with Elastic Open Web Crawler

**Replacement:** [Elastic Open Web Crawler](https://github.com/elastic/crawler) — Docker-based, CLI-driven, compatible with ES 8.x and 9.x.

**Deployment:**
```yaml
# Example crawl-config.yml for Open Crawler
output_sink: elasticsearch
output_index: kiss-crawl-example-nl
elasticsearch:
  host: https://kiss-es-http:9200
  port: 9200
  api_key: <encoded-api-key>
  pipeline_enabled: true
  pipeline_name: kiss-crawler-pipeline
domains:
  - url: https://www.example.nl
    seed_urls:
      - https://www.example.nl/sitemap.xml
```

**Run via K8s CronJob** (one per domain, or a wrapper that iterates):
```yaml
apiVersion: batch/v1
kind: CronJob
metadata:
  name: kiss-crawl-example-nl
spec:
  schedule: "0 * * * *"  # hourly
  jobTemplate:
    spec:
      template:
        spec:
          containers:
          - name: crawler
            image: docker.elastic.co/integrations/crawler:latest
            command: ["jruby", "bin/crawler", "crawl", "/config/crawl-config.yml"]
            volumeMounts:
            - name: config
              mountPath: /config
```

### Task 3: Replace Meta-Engine with Index Alias

```json
POST /_aliases
{
  "actions": [
    { "add": { "index": "kiss-crawl-*", "alias": "kiss-search" } },
    { "add": { "index": "search-smoelenboek", "alias": "kiss-search" } },
    { "add": { "index": "search-vac", "alias": "kiss-search" } },
    { "add": { "index": "search-kennisartikel", "alias": "kiss-search" } }
  ]
}
```

Or use a config-driven comma-separated index list in the BFF.

### Task 4: Update KISS-Elastic-Sync

See [KISS-Elastic-Sync repository](https://github.com/Klantinteractie-Servicesysteem/KISS-Elastic-Sync). Changes needed:
- Remove all Enterprise Search API calls (`ElasticEnterpriseSearchClient.cs` — `AddDomain`, `CrawlDomain`, `AddIndexEngineAsync`, `EnsureMetaEngineAsync`, etc.)
- For **structured sources**: the `ElasticBulkClient.IndexBulk()` path is already pure ES — minimal changes needed. Just remove the `enterpriseClient.AddIndexEngineAsync()` call that follows.
- For **website crawling**: remove the `domain` command path and replace with Open Crawler (see Task 2)
- Create/manage the `kiss-search` index alias to replace the meta-engine
- Remove `ENTERPRISE_SEARCH_*` env vars

### Task 5: Apply `mapping.json` to crawler indices

The structured source indices already have the correct mapping because KISS-Elastic-Sync applies `mapping.json` on creation. The **crawler index** currently gets only `engine.json` (a subset) applied to the hidden `.ent-search*` index managed by Enterprise Search.

For the Open Crawler, create the target index with the **full `mapping.json`** before the first crawl, plus crawler-specific fields:

```json
PUT kiss-crawl-example-nl
{
  // ... full contents of mapping.json ...
  "mappings": {
    "properties": {
      "_completion": { "type": "completion" },
      "title": {
        "analyzer": "iq_text_base",
        "index_options": "freqs",
        "type": "text",
        "copy_to": "_completion",
        "fields": { "enum": { "type": "keyword", "ignore_above": 2048 } }
      },
      "headings": {
        "analyzer": "iq_text_base",
        "index_options": "freqs",
        "type": "text",
        "copy_to": "_completion"
      },
      "body_content": { "analyzer": "iq_text_base", "type": "text", "index_options": "freqs" },
      "url":          { "type": "keyword" },
      "object_bron":  { "type": "text", "fields": { "enum": { "type": "keyword", "ignore_above": 2048 } } },
      "domains":      { "type": "text", "fields": { "enum": { "type": "keyword", "ignore_above": 2048 } } }
    },
    "dynamic": "true",
    "dynamic_templates": [ /* ... same as mapping.json ... */ ]
  },
  "settings": { /* ... same analyzers as mapping.json ... */ }
}
```

This ensures the `.enum`, `.stem`, `.prefix` etc. sub-fields exist, making the query template work unchanged against crawler indices.

### Task 6: Update `indices_boost`

In `src/features/search/service.ts`, change:
```typescript
// Before
query_body.indices_boost = [{ ".ent-search*": 1 }, { "*": 10 }];

// After
query_body.indices_boost = [{ "kiss-crawl-*": 1 }, { "*": 10 }];
```

### Task 7: Update Frontend Field Mappings

In `mapResult()` in `src/features/search/service.ts`:
```typescript
// Before
const content = obj?._source?.body_content;

// After (if not using ingest pipeline to rename body → body_content)
const content = obj?._source?.body_content ?? obj?._source?.body;
```

### Task 8: Remove Enterprise Search Infrastructure

**Files to delete:**
- `Kiss.Bff/Extern/EnterpriseSearch/EnterpriseSearchProxyConfig.cs`
- `Kiss.Bff.Test/EnterpriseProxyConfigTests.cs`

**Files to modify:**
- `Kiss.Bff/Config/ProxyConfig.cs` — remove Enterprise Search special-case (lines ~149-183)
- `Kiss.Bff/Program.cs` — remove `AddEnterpriseSearch(...)` call
- `helm/kiss-chart/values.yaml` — remove `settings.enterpriseSearch.*`
- `helm/kiss-chart/values.schema.json` — remove Enterprise Search schema entries
- `helm/kiss-chart/templates/configmap.yaml` — remove Enterprise Search env vars
- `helm/kiss-chart/templates/secret.yaml` — remove Enterprise Search secrets

**Env vars to remove:**
- `ENTERPRISE_SEARCH_BASE_URL`
- `ENTERPRISE_SEARCH_PRIVATE_API_KEY`
- `ENTERPRISE_SEARCH_PUBLIC_API_KEY`
- `ENTERPRISE_SEARCH_ENGINE`

**Documentation to update:**
- `docs/configuration/elastic.md`
- `docs/installation/configuratie.md`
- `docs/installation/installatie.md`
- `docs/installation/voorbereidingen.md`

### Task 9: Update Tests

- Delete `Kiss.Bff.Test/EnterpriseProxyConfigTests.cs`
- Update `Kiss.Bff.Test/ElasticsearchControllerTests.cs` if index patterns change
- Add tests for the new query template config endpoint (if applicable)

---

---

## Ingest Pipeline Reference

An ingest pipeline is needed for the Open Crawler to:
1. Rename `body` → `body_content` (Open Crawler uses `body`; KISS reads `body_content`)
2. Set `object_bron` to the domain URL (used by the filter aggregations)

```json
PUT _ingest/pipeline/kiss-crawler-pipeline
{
  "description": "Normalize Open Crawler docs to match KISS expected schema",
  "processors": [
    {
      "rename": {
        "field": "body",
        "target_field": "body_content",
        "ignore_missing": true
      }
    },
    {
      "set": {
        "field": "object_bron",
        "value": "{{url_host}}"
      }
    }
  ]
}
```

> **Note:** The `.enum` sub-fields for `domains` and `object_bron` are handled entirely by the **index mapping** (the `mapping.json` dynamic template), not by the ingest pipeline. The pipeline only needs to handle field renaming and setting `object_bron`.

> **Note on structured source indices:** KISS-Elastic-Sync already applies `mapping.json` when it creates structured source indices — no ingest pipeline needed there. Only crawler indices require pipeline processing.

---

## Risks & Considerations

| Risk | Impact | Mitigation |
|---|---|---|
| **Loss of Relevance Tuning UI** | Admins can no longer adjust field weights via Kibana | Export weights before migration; store as config. Optionally build a basic Relevance Tuning UI in KISS (see Option C above) |
| **Open Crawler is in beta** | Potential bugs, API changes | It's the official Elastic replacement; feature coverage is sufficient for KISS |
| **`body` vs `body_content` field name** | Crawler uses `body`, KISS reads `body_content` | Use ingest pipeline (see above) |
| **`object_bron` missing from crawler docs** | Filter aggregations break for website sources | Set via ingest pipeline |
| **Crawler index mapping** | Must have `.enum`, `.stem` etc. sub-fields for query template to work | Apply full `mapping.json` to crawler index before first crawl |
| **KISS-Elastic-Sync coordination** | Separate repo; structured-source changes are minimal (remove `AddIndexEngineAsync`) | Plan a coordinated release |
| **Downtime risk** | Search may be unavailable during switchover | Run both systems in parallel during transition |
| **Over-replicating App Search defaults** | Recreating all App Search complexity in native ES gains nothing | Keep the initial migration simple; treat query/mapping as a follow-on improvement (see [Migration as an Opportunity](#migration-as-an-opportunity-to-simplify)) |

---

## Execution Order

### If Option A (frontend query construction — minimal change):

```
Phase 1: Preparation (no user impact)
├── 0. Export current search_explain template and Relevance Tuning weights
├── 1. Create crawler index with full mapping.json + ingest pipeline
└── 2. Set up Open Crawler on staging, verify documents land correctly

Phase 2: KISS-frontend changes
├── 3. Replace useQueryTemplate() with static/configurable template
├── 4. Update indices_boost pattern (.ent-search* → kiss-crawl-*)
├── 5. Update mapResult() body_content fallback (if not using ingest pipeline)
└── 6. Remove Enterprise Search proxy code from BFF

Phase 3: KISS-Elastic-Sync changes
├── 7. Remove AddIndexEngineAsync() / AddDomain() / CrawlDomain() calls
├── 8. Remove ElasticEnterpriseSearchClient.cs
├── 9. Add index alias management
└── 10. Deploy Open Crawler CronJobs in K8s

Phase 4: Verify and clean up
├── 11. Verify search results match previous behavior
├── 12. Remove Enterprise Search K8s service
├── 13. Remove env vars and Helm chart entries
├── 14. Update documentation
└── 15. Delete old .ent-search* indices
```

### If Option B (backend query construction — recommended):

```
Phase 1: Preparation (no user impact)
├── 0. Export current search_explain template and Relevance Tuning weights
├── 1. Create crawler index with full mapping.json + ingest pipeline
└── 2. Set up Open Crawler on staging, verify documents land correctly

Phase 2: Build new backend search API
├── 3. Create SearchService.cs (query construction, template config, field weights)
├── 4. Create SearchController.cs (POST /api/search, GET /api/search/sources,
│      POST /api/search/medewerkers)
├── 5. Create SearchConfig.cs + SearchModels.cs (config binding + DTOs)
├── 6. Move role-based field filtering from ElasticsearchService into SearchService
└── 7. Add tests for new search endpoints

Phase 3: Update frontend
├── 8. Replace useQueryTemplate() + ES query construction with calls to new API
├── 9. Replace useSources() with GET /api/search/sources call
├── 10. Replace searchMedewerkers() with POST /api/search/medewerkers call
└── 11. Remove BRON_QUERY, mapSuggestions, getPayload, useQueryTemplate from service.ts

Phase 4: Remove Enterprise Search from BFF
├── 12. Remove Enterprise Search proxy (EnterpriseSearchProxyConfig.cs, YARP route)
├── 13. Deprecate/remove api/elasticsearch passthrough endpoint
└── 14. Remove EnterpriseProxyConfigTests.cs

Phase 5: KISS-Elastic-Sync changes
├── 15. Remove AddIndexEngineAsync() / AddDomain() / CrawlDomain() calls
├── 16. Remove ElasticEnterpriseSearchClient.cs
├── 17. Add index alias management
└── 18. Deploy Open Crawler CronJobs in K8s

Phase 6: Verify and clean up
├── 19. Verify search results match previous behavior
├── 20. Remove Enterprise Search K8s service
├── 21. Remove env vars and Helm chart entries
├── 22. Update documentation
└── 23. Delete old .ent-search* indices
```
└── 14. Remove EnterpriseProxyConfigTests.cs

Phase 5: Infrastructure
├── 15. Update KISS-Elastic-Sync (separate repo)
├── 16. Deploy Open Crawler CronJobs in K8s
└── 17. Verify search results match previous behavior

Phase 6: Cleanup
├── 18. Remove Enterprise Search K8s resources
├── 19. Remove env vars and Helm chart entries
├── 20. Update documentation
└── 21. Delete old .ent-search* indices
```
