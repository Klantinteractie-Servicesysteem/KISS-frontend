# Migration Plan: Enterprise Search → Native Elasticsearch (ES9)

> **Context:** Enterprise Search (App Search + Workplace Search) is removed in Elasticsearch 9. This document describes the current usage and the migration path for KISS.

## Table of Contents

- [Current Architecture](#current-architecture)
- [What Enterprise Search Does for KISS](#what-enterprise-search-does-for-kiss)
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
| `ENTERPRISE_SEARCH_BASE_URL` | Enterprise Search server URL | BFF YARP proxy |
| `ENTERPRISE_SEARCH_PRIVATE_API_KEY` | Bearer token for App Search API | BFF YARP proxy |
| `ENTERPRISE_SEARCH_PUBLIC_API_KEY` | Public API key (stored as K8s secret) | Not used in BFF directly |
| `ENTERPRISE_SEARCH_ENGINE` | Meta-engine name (e.g., `kiss-engine`) | KISS-Elastic-Sync |
| `ELASTIC_BASE_URL` | Elasticsearch cluster URL | BFF HttpClient (stays) |
| `ELASTIC_USERNAME` | ES basic auth username | BFF HttpClient (stays) |
| `ELASTIC_PASSWORD` | ES basic auth password | BFF HttpClient (stays) |
| `ELASTIC_EXCLUDED_FIELDS_KENNISBANK` | Fields hidden from Kennisbank users | ElasticsearchService (stays) |

### Elasticsearch Indices Used

| Index Pattern | Source | Created By |
|---|---|---|
| `.ent-search-engine-*` | Website crawl results | Enterprise Search Crawler |
| `search-smoelenboek` | Employee data | KISS-Elastic-Sync |
| (structured source indices) | VACs, Kennisartikelen, SharePoint | KISS-Elastic-Sync |

---

## What Enterprise Search Does for KISS

### 1. Query Template via `search_explain` (Runtime)

The **only runtime call** to Enterprise Search. The frontend sends:

```json
POST /api/as/v1/engines/kiss-engine/search_explain
{ "query": "{{query}}" }
```

And receives back an Elasticsearch query DSL template containing:
- A `multi_match` query with per-field `^weight` boosts from Relevance Tuning
- A list of indices (derived from the meta-engine's source engines)

The frontend then:
1. Strips `from`, `size`, `_source`
2. Adds `indices_boost: [{ ".ent-search*": 1 }, { "*": 10 }]` (crawler results ranked lower)
3. Adds `suggest` block for autocomplete on `_completion` field
4. Replaces `{{query}}` with the real search term
5. Adds filter clauses for source selection (`object_bron.enum`, `domains.enum`)
6. Sends this modified query directly to Elasticsearch

### 2. Meta-Engine `kiss-engine` (Configuration)

Aggregates multiple source engines into one searchable unit. The KISS-Elastic-Sync tool creates and manages this. The frontend derives the list of searchable indices from the `search_explain` response's `query_string`.

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
- Remove all Enterprise Search API calls (engine/meta-engine/crawler management)
- For websites: generate Open Crawler config YAML + trigger crawl, OR deploy separately
- For structured sources: continue direct ES indexing (likely minimal changes)
- Create/manage the `kiss-search` alias

### Task 5: Update `indices_boost`

In `src/features/search/service.ts`, change:
```typescript
// Before
query_body.indices_boost = [{ ".ent-search*": 1 }, { "*": 10 }];

// After
query_body.indices_boost = [{ "kiss-crawl-*": 1 }, { "*": 10 }];
```

### Task 6: Update Frontend Field Mappings

In `mapResult()` in `src/features/search/service.ts`:
```typescript
// Before
const content = obj?._source?.body_content;

// After (if not using ingest pipeline)
const content = obj?._source?.body_content ?? obj?._source?.body;
```

### Task 7: Remove Enterprise Search Infrastructure

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

### Task 8: Update Tests

- Delete `Kiss.Bff.Test/EnterpriseProxyConfigTests.cs`
- Update `Kiss.Bff.Test/ElasticsearchControllerTests.cs` if index patterns change
- Add tests for the new query template config endpoint (if applicable)

---

## Ingest Pipeline Reference

To normalize Open Crawler documents to match the current schema:

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
        "value": "Website"
      }
    },
    {
      "script": {
        "description": "Add .enum keyword copies for aggregation fields",
        "source": """
          // domains.enum is used for source filtering in the frontend
          // The actual .enum sub-field should be handled by index mappings
        """
      }
    }
  ]
}
```

Index mappings for crawler indices:
```json
PUT kiss-crawl-example-nl
{
  "mappings": {
    "properties": {
      "body_content": { "type": "text" },
      "title": { "type": "text" },
      "url": { "type": "keyword" },
      "domains": {
        "type": "text",
        "fields": { "enum": { "type": "keyword" } }
      },
      "object_bron": {
        "type": "text",
        "fields": { "enum": { "type": "keyword" } }
      },
      "headings": { "type": "text" },
      "meta_description": { "type": "text" },
      "last_crawled_at": { "type": "date" }
    }
  }
}
```

---

## Risks & Considerations

| Risk | Impact | Mitigation |
|---|---|---|
| **Loss of Relevance Tuning UI** | Admins can no longer adjust field weights via Kibana | Export weights before migration; provide a config file/endpoint |
| **Open Crawler is in beta** | Potential bugs, API changes | It's the official Elastic replacement; feature coverage is sufficient for KISS |
| **Schema differences** | `body` vs `body_content`, missing `object_bron` | Use ES ingest pipelines to normalize |
| **KISS-Elastic-Sync coordination** | Separate repo needs simultaneous changes | Plan a coordinated release |
| **Downtime risk** | Search may be unavailable during switchover | Run both systems in parallel during transition |
| **`.enum` sub-fields** | Aggregations depend on `.enum` keyword fields | Set up proper index mappings before first crawl |

---

## Execution Order

### If Option A (frontend query construction — minimal change):

```
Phase 1: Preparation (no user impact)
├── 0. Export current search_explain template and Relevance Tuning weights
├── 1. Set up Open Crawler with ingest pipeline on staging
└── 2. Create index mappings and alias for crawler indices

Phase 2: KISS-frontend changes
├── 3. Replace useQueryTemplate() with static/configurable template
├── 4. Update indices_boost pattern
├── 5. Update mapResult() if not using ingest pipeline
└── 6. Remove Enterprise Search proxy code from BFF

Phase 3: Infrastructure
├── 7. Update KISS-Elastic-Sync (separate repo)
├── 8. Deploy Open Crawler CronJobs in K8s
└── 9. Verify search results match previous behavior

Phase 4: Cleanup
├── 10. Remove Enterprise Search K8s resources
├── 11. Remove env vars and Helm chart entries
├── 12. Update documentation
└── 13. Delete old .ent-search* indices
```

### If Option B (backend query construction — recommended):

```
Phase 1: Preparation (no user impact)
├── 0. Export current search_explain template and Relevance Tuning weights
├── 1. Set up Open Crawler with ingest pipeline on staging
└── 2. Create index mappings and alias for crawler indices

Phase 2: Build new backend search API
├── 3. Create SearchService.cs (query construction, template, field weights)
├── 4. Create SearchController.cs (POST /api/search, GET /api/search/sources,
│      POST /api/search/medewerkers)
├── 5. Create SearchConfig.cs + SearchModels.cs (config binding + DTOs)
├── 6. Move role-based field filtering from ElasticsearchService into SearchService
└── 7. Add tests for new search endpoints

Phase 3: Update frontend
├── 8. Replace useQueryTemplate() + ES query construction with calls to new API
│      (POST /api/search with simple parameters)
├── 9. Replace useSources() with GET /api/search/sources call
├── 10. Replace searchMedewerkers() with POST /api/search/medewerkers call
└── 11. Remove BRON_QUERY, mapSuggestions, getPayload from service.ts

Phase 4: Remove old code
├── 12. Remove Enterprise Search proxy (EnterpriseSearchProxyConfig.cs, YARP route)
├── 13. Deprecate/remove api/elasticsearch passthrough endpoint
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
