# Story: Remove Enterprise Search from Search Logic

## User Story

**As a** developer, **I want to** replace the runtime dependency on Enterprise Search's `search_explain` API with a hardcoded query template containing the current field weights and precision settings, **so that** KISS search works without Enterprise Search at runtime.

## Context

Currently the frontend calls `search_explain` on every search to get a query template with field boosts and precision settings from App Search. Enterprise Search is removed in ES9, so we need to capture and inline this template.

## Acceptance Criteria

- [ ] **Step 0 (before any other work):** Export the current `search_explain` response from the running system and commit it as reference (`docs/search-explain-snapshot.json`)
- [ ] The `useQueryTemplate()` composable no longer calls the Enterprise Search `search_explain` endpoint
- [ ] A static/configurable query template is used instead, containing the captured `multi_match` with per-field `^weight` boosts and current precision-level settings (`minimum_should_match`, `fuzziness`, included sub-fields)
- [ ] `indices_boost` pattern is updated from `{ ".ent-search*": 1 }` to `{ "kiss-crawl-*": 1 }` (or whatever the new crawler index pattern is)
- [ ] Enterprise Search proxy code is removed from the BFF:
  - Delete `Kiss.Bff/Extern/EnterpriseSearch/EnterpriseSearchProxyConfig.cs`
  - Remove YARP route from `Kiss.Bff/Config/ProxyConfig.cs` (lines ~149-183)
  - Remove `AddEnterpriseSearch(...)` from `Kiss.Bff/Program.cs`
  - Delete `Kiss.Bff.Test/EnterpriseProxyConfigTests.cs`
- [ ] `ENTERPRISE_SEARCH_BASE_URL`, `ENTERPRISE_SEARCH_PRIVATE_API_KEY`, `ENTERPRISE_SEARCH_PUBLIC_API_KEY`, and `ENTERPRISE_SEARCH_ENGINE` env vars are removed from Helm chart (`values.yaml`, `values.schema.json`, `configmap.yaml`, `secret.yaml`)
- [ ] Documentation updated (`docs/configuration/elastic.md`, `docs/installation/configuratie.md`, `docs/installation/installatie.md`, `docs/installation/voorbereidingen.md`)
- [ ] Search results on the test environment match previous behavior (same queries return comparable results)
- [ ] Existing `ElasticsearchControllerTests` still pass (updated if index patterns changed)

## Technical Notes

- The current precision setting is likely around **8** (stricter matching was preferred by users over the default of 2). Verify by inspecting the `search_explain` output before migration.
- The hardcoded template captures both the **relevance tuning** (field weights) and **precision tuning** (which sub-fields are included, `minimum_should_match`, `fuzziness`). Document what precision level the snapshot represents.
- The `mapping.json` and existing index mappings (`.stem`, `.prefix`, `.delimiter`, `.joined` sub-fields) remain unchanged — the query template depends on them.
- **Future consideration:** Currently the frontend constructs full Elasticsearch query DSL and sends it through a passthrough proxy. This migration is a natural opportunity to move query construction entirely to the backend (a new `SearchService` / `SearchController` in the BFF). This would stop exposing raw ES query DSL to the browser, eliminate the ability for authenticated users to craft arbitrary ES queries, and allow tuning field weights or precision without a frontend deploy. This is documented in detail in `docs/migration-enterprise-search.md` under "Option B". It could be picked up as a follow-on story after the hardcoded template is working.
