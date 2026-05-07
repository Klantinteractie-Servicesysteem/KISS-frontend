# Story: Replace Enterprise Search Crawler with Elastic Open Crawler

## User Story

**As a** developer, **I want to** replace the Enterprise Search web crawler with the [Elastic Open Web Crawler](https://github.com/elastic/crawler), **so that** website content continues to be indexed after Enterprise Search is removed.

## Context

Enterprise Search's built-in crawler creates documents in hidden `.ent-search-engine-*` indices. The Open Crawler is the official Docker-based replacement, outputting to regular ES indices.

## Acceptance Criteria

- [ ] New crawler index (e.g., `kiss-crawl-example-nl`) is created with the **full `mapping.json`** applied (including all sub-fields: `.enum`, `.stem`, `.prefix`, `.delimiter`, `.joined`, `.date`, `.float`, and the `_completion` completion suggester)
- [ ] Crawler-specific fields are mapped: `title` and `headings` have `copy_to: _completion`; `body_content`, `url`, `object_bron`, `domains` are mapped per the migration doc
- [ ] An ingest pipeline `kiss-crawler-pipeline` is created that:
  - Renames `body` → `body_content` (Open Crawler uses `body`)
  - Sets `object_bron` to the domain URL (needed for filter aggregations)
- [ ] Open Crawler config (`crawl-config.yml`) is created for each crawled domain, pointing to the correct ES cluster, index, and ingest pipeline
- [ ] Open Crawler runs as a K8s CronJob (one per domain or a wrapper iterating domains)
- [ ] `mapResult()` in the frontend handles `body_content` correctly (fallback to `body` if ingest pipeline isn't active: `obj?._source?.body_content ?? obj?._source?.body`)
- [ ] In KISS-Elastic-Sync: remove Enterprise Search crawler API calls (`AddDomain`, `CrawlDomain` from `ElasticEnterpriseSearchClient.cs`) and the `domain` command path
- [ ] In KISS-Elastic-Sync: remove `AddIndexEngineAsync()`, `EnsureMetaEngineAsync()`, and all remaining Enterprise Search API calls
- [ ] Replace the meta-engine with either an index alias (`kiss-search`) or a config-driven index list in the BFF
- [ ] Crawled documents appear in the new index and are searchable with the same query template
- [ ] Old `.ent-search*` indices can be deleted after verification

## Technical Notes

- Open Crawler is currently in beta but is the official Elastic replacement. Feature coverage is sufficient for KISS.
- Run both crawlers in parallel during transition to avoid search downtime.
- The `domains.enum` and `object_bron.enum` sub-fields are handled by the index mapping's dynamic template, not the ingest pipeline.
