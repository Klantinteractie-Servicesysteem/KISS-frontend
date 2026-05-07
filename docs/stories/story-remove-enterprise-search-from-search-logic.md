# Story: Remove Enterprise Search from Search Logic

## Context

Currently the frontend calls `search_explain` on every search to get a query template with field boosts and precision settings from App Search. Enterprise Search is removed in ES9, so we need to capture and inline this template.

## Acceptance Criteria

- [ ] The search queries (in the top search bar and medewerker search in the contactverzoek) no longer need enterprise search to function
- [ ] Search results on the test environment match previous behavior (same queries return comparable results)
- [ ] The field exclusion logic for users with only the kennisbank role still works
- [ ] All enterprise search related code is removed from the back-end and the front-end
- [ ] All enterprise search related configuration is removed from the helm chart / documentation

## Technical Notes

- The best precision setting to use when capturing the query template is likely around **8** (stricter matching was preferred by users over the default of 2).
- The hardcoded template captures both the **relevance tuning** (field weights) and **precision tuning** (which sub-fields are included, `minimum_should_match`, `fuzziness`). Document what precision level the snapshot represents.
- The `mapping.json` and existing index mappings (`.stem`, `.prefix`, `.delimiter`, `.joined` sub-fields) remain unchanged — the query template depends on them.
- **Technical consideration:** Currently the frontend constructs full Elasticsearch query DSL and sends it through a passthrough proxy. This migration is a natural opportunity to move query construction entirely to the backend (a new `SearchService` / `SearchController` in the BFF). This would stop exposing raw ES query DSL to the browser, eliminate the ability for authenticated users to craft arbitrary ES queries, and allow tuning field weights or precision without a frontend deploy.
