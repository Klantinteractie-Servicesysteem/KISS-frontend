# Story: Update Elasticsearch on Test Environment to Version 9

## Context

Enterprise Search is fully removed in ES9. The test environment should run ES9 to validate that our native ES approach works correctly. This story covers the infrastructure upgrade only.

## Acceptance Criteria

- [ ] Elasticsearch on the test environment is upgraded to version 9.x
- [ ] Kibana is upgraded to the matching 9.x version
- [ ] All existing indices are accessible after upgrade (or re-indexed if needed due to breaking changes)
- [ ] The KISS BFF can connect to ES9 and execute search queries
- [ ] KISS-Elastic-Sync can connect to ES9 and index documents

## Technical Notes

- Review the [ES9 breaking changes](https://www.elastic.co/guide/en/elasticsearch/reference/current/breaking-changes.html) before upgrading.
