# Story: Update Elasticsearch on Test Environment to Version 9

## User Story

**As a** developer, **I want to** upgrade the Elasticsearch instance on the test environment from the current version to version 9, **so that** we can validate all migration changes against the target ES version.

## Context

Enterprise Search is fully removed in ES9. The test environment should run ES9 to validate that our native ES approach works correctly. This story covers the infrastructure upgrade only.

## Acceptance Criteria

- [ ] Elasticsearch on the test environment is upgraded to version 9.x
- [ ] Kibana is upgraded to the matching 9.x version
- [ ] All existing indices are accessible after upgrade (or re-indexed if needed due to breaking changes)
- [ ] The KISS BFF can connect to ES9 and execute search queries
- [ ] KISS-Elastic-Sync can connect to ES9 and index documents via `_bulk`
- [ ] Any deprecated ES APIs used by KISS (check Elasticsearch deprecation logs after upgrade) are identified and documented for follow-up
- [ ] Helm chart / K8s manifests for ES are updated for v9 (ECK operator version, Elasticsearch CRD spec, etc.)
- [ ] The legacy `docs/scripts/elastic/eck/templates/enterprise.yaml` Enterprise Search CRD is removed (no longer valid in ES9)

## Technical Notes

- Review the [ES9 breaking changes](https://www.elastic.co/guide/en/elasticsearch/reference/current/breaking-changes.html) before upgrading.
- Enterprise Search CRDs will fail on ES9 — remove them before or during the upgrade.
- The `.ent-search*` hidden indices may need cleanup before upgrading if they cause compatibility issues.
- Consider taking a snapshot/backup of the test cluster before upgrading.
- This story should ideally be done early so other stories can be validated against ES9.
