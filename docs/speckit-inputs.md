# Speckit Inputs — Enterprise Search ES9 Migration

Use the text blocks below as input to the four Speckit slash commands, in order.

---

## 1. `/speckit constitution`

> Run this first. It sets the project-wide principles that all specs must follow.

```
KISS is a customer interaction system (Klantinteractie Servicesysteem) built for Dutch municipalities.

Tech stack:
- Frontend: Vue 3, TypeScript, Vite, Vitest
- BFF: .NET 8, ASP.NET Core, YARP reverse proxy
- Search backend: Elasticsearch (migrating to ES9+)
- Infrastructure: Docker Compose (dev), Kubernetes / ECK (production)
- Data sync tool: KISS-Elastic-Sync (.NET, separate repo — to be merged)

Core principles to encode:
1. Backward Compatibility — search quality must not regress without sign-off. Before removing Enterprise Search, capture the exact query shape (field weights, precision level ~8) from search_explain.
2. Simplicity Over Configuration — replace Enterprise Search inherited defaults with explicit, readable config inside this repo.
3. Layered Architecture (BFF Pattern) — the frontend talks only to the BFF; direct ES calls from the browser are not allowed. New search query logic belongs in Kiss.Bff, not in service.ts.
4. Test Coverage for Search Logic — all query construction must be unit-tested (xUnit + Vitest). Role-based field filtering must be tested. DSL changes require test updates.
5. Single Repository — KISS-Elastic-Sync should be merged into this repo; all deployment-related code belongs together.

Development workflow:
- Feature branches: sequential `###-feature-name`
- Specs in `specs/###-feature-name/` (spec.md, plan.md, tasks.md)
- Migration docs in `docs/`
- Env vars documented in docker-compose.yml or .env.example
- Breaking index schema changes need a documented migration path

Governance: constitution supersedes all other practices; amendments need rationale. Authoritative migration reference: docs/migration-enterprise-search.md
```

---

## 2. `/speckit specify`

> Run after constitution. Creates `specs/001-enterprise-search-migration/spec.md`.

```
Remove all Elastic Enterprise Search (App Search) dependencies from KISS before upgrading to Elasticsearch 9. Enterprise Search is removed in ES9.

Background (see docs/migration-enterprise-search.md for full detail):
- The frontend currently calls Enterprise Search's search_explain API to get a query template, modifies it, then sends it to ES. This is the ONLY runtime call to Enterprise Search.
- Structured indices (kennisbank, vac, smoelenboek) are already native ES via _bulk — no changes needed there.
- The Enterprise Search website crawler must be replaced by Elastic Open Crawler.
- The new crawler index needs the same custom mapping (7 sub-fields per string, Dutch analyzers).
- An ingest pipeline is needed: rename body → body_content, derive object_bron from url_host.
- Precision tuning is currently ~8 (not the default 2); this must be preserved explicitly.
- Field weights from search_explain must be captured from the live system before any code changes.
- Recommended approach: move query construction to the BFF (Option B), removing the frontend's dependency on search_explain entirely.
- KISS-Elastic-Sync (separate repo) also calls Enterprise Search to register engines; those calls must be removed.

User stories:
- P1: A KCM user searches KISS and gets results of the same quality as today — no Enterprise Search dependency at runtime.
- P2: A KCM user can search across website-crawled content (Open Crawler replaces Enterprise Search crawler, results appear in search).
- P3 (optional follow-on, separate decision): An admin can view and update field weights and precision setting via a Relevance Tuning UI within KISS itself.

Feature branch: 001-enterprise-search-migration
```

---

## 3. `/speckit plan`

> Run after spec. Creates `specs/001-enterprise-search-migration/plan.md`.

```
Generate an implementation plan for the Enterprise Search ES9 migration spec (specs/001-enterprise-search-migration/spec.md).

Key technical decisions already made:
- Option B: BFF builds the ES query DSL (Kiss.Bff/Extern/Elasticsearch/SearchService.cs, new SearchController)
- The frontend service.ts useQueryTemplate() call is removed; frontend sends the raw query string to the BFF endpoint
- KISS-Elastic-Sync repo will be merged into this repository before or alongside migration work

Phases to cover:
0. Pre-migration (blocker): Export search_explain response from the live system to capture field weights and precision level.
1. Merge KISS-Elastic-Sync into this repo (separate sub-project under Kiss.ElasticSync/ or similar).
2. BFF: Create SearchController + SearchService that builds the multi_match query DSL with explicit field weights and precision (minimum_should_match, fuzziness, sub-field selection).
3. Remove EnterpriseSearchProxyConfig.cs, ProxyConfig.cs Enterprise Search route, AddEnterpriseSearch() from Program.cs, related tests.
4. Frontend: Remove useQueryTemplate() call, pass raw query string to new BFF endpoint, remove indices_boost manipulation (move to BFF).
5. Open Crawler: Apply mapping.json to new kiss-crawl-* index, create ingest pipeline, configure crawler, update indices_boost pattern.
6. KISS-Elastic-Sync cleanup: Remove ElasticEnterpriseSearchClient.cs, remove engine/crawler registration calls from Program.cs.
7. Cleanup: Remove ENTERPRISE_SEARCH_* env vars from docker-compose.yml and documentation, update helm charts.

Risks to note:
- Precision level must be set correctly — wrong value degrades search quality noticeably
- Open Crawler schema differs from Enterprise Search crawler (body vs body_content field name)
- indices_boost pattern changes from .ent-search* to kiss-crawl-*
```

---

## 4. `/speckit tasks`

> Run after plan. Creates `specs/001-enterprise-search-migration/tasks.md`.

```
Generate tasks for the Enterprise Search ES9 migration based on specs/001-enterprise-search-migration/spec.md and specs/001-enterprise-search-migration/plan.md.

Important context:
- See docs/migration-enterprise-search.md for full file-by-file change details and code snippets
- Phase 0 is a hard blocker: the search_explain export MUST happen before any code is changed
- Tasks should reflect the phased approach in the plan
- Include tasks for: KISS-Elastic-Sync merge, BFF SearchService/Controller, frontend simplification, Open Crawler setup, env var cleanup
- Each task should reference the specific file(s) to change
- Mark Phase 0 (capture search_explain) as a dependency for all Phase 2+ tasks
```
