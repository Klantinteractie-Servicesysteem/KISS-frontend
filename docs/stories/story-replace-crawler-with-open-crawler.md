# Story: Replace Enterprise Search Crawler with Elastic Open Crawler

## Context

The crawler we use now is part of Enterprise Search and will no longer be available in Elasticsearch 9. The Open Crawler is the official Docker-based replacement, outputting to regular ES indices and is compatible with both version 8 and version 9 of Elasticsearch.

## Acceptance Criteria

- [ ] Open Crawler runs as a K8s CronJob per domain, preferably using the same helm chart configuration values as we use now to create and start the crawler with the sync tool.
- [ ] The pages that are found by the crawler appear in an index and are searchable by KISS in the same way as before. Clicking on a result opens the web page in a new tab.

## Technical Notes

- Open Crawler is currently in beta but is the official Elastic replacement.
