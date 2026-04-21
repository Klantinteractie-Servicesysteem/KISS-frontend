---
marp: true
theme: default
paginate: true
style: |
  section {
    font-family: 'Segoe UI', Arial, sans-serif;
    color: #1a1a2e;
    background: #ffffff;
    padding: 36px 48px;
  }
  section.title {
    background: #0057b8;
    color: #ffffff;
    display: flex;
    flex-direction: column;
    justify-content: center;
  }
  section.title h1 { font-size: 2rem; font-weight: 700; color: #ffffff; margin-bottom: 0.4em; }
  section.title p { font-size: 1.05rem; opacity: 0.85; color: #ffffff; }
  h1 { font-size: 1.35rem; color: #0057b8; margin-bottom: 0.4em; }
  h2 { font-size: 0.98rem; color: #1a1a2e; margin: 0.6em 0 0.2em; }
  ul { margin: 0; padding-left: 1.3em; }
  li { margin-bottom: 0.2em; line-height: 1.4; font-size: 0.93em; }
  p { margin: 0.4em 0; font-size: 0.93em; line-height: 1.45; }
  table { width: 100%; border-collapse: collapse; font-size: 0.78em; }
  th { background: #0057b8; color: white; padding: 5px 9px; text-align: left; }
  td { padding: 4px 9px; border-bottom: 1px solid #e5e7eb; }
  tr:nth-child(even) td { background: #f9fafb; }
  blockquote { border-left: 4px solid #0057b8; padding: 0.4em 0.9em; background: #f0f6ff; margin: 0.6em 0; border-radius: 0 6px 6px 0; font-style: normal; font-size: 0.88em; }
  blockquote p { margin: 0; }
  footer { font-size: 0.7em; color: #9ca3af; }
---

<!-- _class: title -->

# Migrating from Elasticsearch 8 to 9 and away from Enterprise Search

---

# Why we need to migrate

Elastic is **removing Enterprise Search entirely** in Elasticsearch 9. KISS currently depends on it for three things:

- **A search recipe** — Enterprise Search tells KISS how to search: which fields to look in and how much weight to give each one
- **Grouping data sources** — it organises all content (VAC, Kennisbank, websites, …) into one searchable unit
- **Website crawling** — it crawls websites and stores the content for searching

All three need a replacement before we can upgrade to ES9.

> At runtime, KISS only calls Enterprise Search for one thing: fetching the search recipe.
> Everything else already goes directly to Elasticsearch.

---

# How search works in KISS today

When a user searches, three steps happen:

1. **KISS asks Enterprise Search:** _"How should I search for this?"_
   It returns a set of search instructions — which fields to search, how much weight each gets, and how broad or strict the matching should be.

2. **KISS adjusts those instructions:**
   Adds filters (e.g. show only VAC), ranking rules (website results rank lower), and autocomplete hints.

3. **KISS sends the final search to Elasticsearch**, which runs it and returns results.

All structured content (VAC, Kennisbank, Smoelenboek, SharePoint) is already stored directly in Elasticsearch — Enterprise Search only _registers_ them. The actual data storage and retrieval has never depended on it.

---

# Changes

| What                                     | Current state                                      | After migration                                            |
| ---------------------------------------- | -------------------------------------------------- | ---------------------------------------------------------- |
| VAC, Kennisbank, Smoelenboek, SharePoint | Already stored directly in Elasticsearch           | No change to how data is stored                            |
| Sync tool (KISS-Elastic-Sync)            | Calls Enterprise Search only to _register_ indices | Remove one file, two function calls                        |
| Search recipe                            | Fetched from Enterprise Search at runtime          | Stored as configuration; applied by the server             |
| Website crawler                          | Built into Enterprise Search                       | Replace with Elastic's standalone Open Crawler             |
| Relevance / precision tuning             | Admin screens in Kibana                            | Export current settings first; rebuild UI later (optional) |

---

# Suggestion: merge the sync tool into this repository

KISS-Elastic-Sync lives in a separate repository, but it is part of the same product — same team, same release cycle.

**The problem during this migration:**

- Changes span both codebases significantly — two repos means two PRs, two reviews, two releases
- AI tools work best with the full picture in one place
- A developer fixing a search issue needs to trace code across two repositories

**The proposal:**
Move the sync tool code into the KISS repository.
Deployment stays separate — it still runs as a scheduled job, not alongside the web server.
Git history can be preserved with `git subtree` or `git filter-repo`.

> **Recommendation:** merge first, then do the migration.
> The scope of sync tool changes during this migration makes the two-repo friction immediately painful.

---

# Design choice: where does the search logic live?

Right now the **frontend** builds the full search request. There are two options.

## Option A — Keep it in the browser _(less work)_

- ✅ Least migration effort
- ❌ Full search configuration visible to anyone who opens the browser dev tools
- ❌ Authenticated users can send arbitrary search requests

## Option B — Move it to the server _(recommended)_

The browser sends simple parameters. The server builds the search request, applies weights, and talks to Elasticsearch.

- ✅ Search configuration stays on the server, not visible in the browser
- ✅ Cleaner frontend code
- ❌ More migration work — new server-side search component needed

---

# Relevance Tuning UI

This is **independent of Option A vs B**. It can be combined with either.

After migration, the Kibana tuning screens disappear. The question is what replaces them.

**Option: build a simple admin page in KISS** where authorised users can adjust:

- Which fields matter most and by how much _(e.g. "title is 3× more important than body text")_
- How strict the search is _(broad / normal / strict)_

> **Recommendation:** not a blocker for the core migration.
> First, save the current weights and precision level as configuration.
> Build the UI as a follow-on once the core migration is stable.

---

# Precision & relevance tuning — and our history

**Relevance tuning** — which fields matter most (weights per field).
**Precision tuning** — how strict the search is (slider 1–11: broad → strict).

Both are set via Kibana and baked into the search recipe from Enterprise Search.

| Precision     | Behaviour                                 |
| ------------- | ----------------------------------------- |
| 1–2 (default) | Broad — includes loose matches and typos  |
| 3–8           | Increasingly strict                       |
| 9–11          | Only close matches; all terms must appear |

- Customers were **unhappy with the default (level 2)** — results felt noisy
- We experimented with **level 8** — noticeably better for our content
- After migration, precision should be an **explicit config value**, not hidden in a recipe snapshot

---

# An opportunity to improve search quality

The current setup was built around App Search's generic defaults — designed for any content in any language.

What this means:

- The setup supports 11 precision levels with different matching rules for each — KISS uses one level at a time, so most of that complexity is unused
- Field weights have likely never been reviewed against real search results
- A purpose-built Dutch search configuration may work better than inherited generic defaults

---

# Our approach to improving quality

> Migrate first, improve after.

1. **Reproduce current behavior** in native Elasticsearch — safe, low-risk, preserves what works today
2. **Improve iteratively** — with real user feedback as the measure, not inherited defaults

Moving search logic to the server (Option B) makes iteration much easier: tuning changes don't require a frontend release.

---

# Before touching anything — capture the current state

**The most important first step:**

Ask Enterprise Search: _"What search recipe are you currently using?"_
Save the complete answer. This gives us:

- All **field weights** (what Relevance Tuning is currently set to)
- The **precision level** in use (what Precision Tuning is currently set to)
- The **list of all data sources** being searched

---

# Migration phases

| Phase                              | Work                                                                      | User impact |
| ---------------------------------- | ------------------------------------------------------------------------- | ----------- |
| **1 — Prepare**                    | Capture search recipe · set up new crawler on staging                     | None        |
| **2 — Server search** _(Option B)_ | Build server-side search component · apply field weights · verify quality | None        |
| **3 — Update browser**             | Remove Enterprise Search calls · connect to new server endpoints          | Transparent |
| **4 — Update sync tool**           | Remove Enterprise Search calls · configure Open Crawler                   | None        |
| **5 — Clean up**                   | Remove Enterprise Search service · delete old indices · update config     | None        |

---

# Replacing the web crawler

|                     | Enterprise Search Crawler           | Elastic Open Crawler                             |
| ------------------- | ----------------------------------- | ------------------------------------------------ |
| Where it runs       | Built into Enterprise Search        | Separate service (scheduled job)                 |
| Where content lands | Hidden internal Elasticsearch index | Regular Elasticsearch index                      |
| Status              | Discontinued in ES9                 | Official Elastic replacement (currently in beta) |

---

# Recommended approach

| Decision            | Recommendation                                             |
| ------------------- | ---------------------------------------------------------- |
| Repositories        | Merge KISS-Elastic-Sync into this repo first               |
| Architecture        | Option B — move search logic to the server                 |
| Crawler             | Replace with Elastic Open Crawler                          |
| Precision & weights | Capture from live system; store as explicit config         |
| Search quality      | Migrate with same behavior first, then improve iteratively |
| Relevance Tuning UI | Follow-on feature after core migration is stable           |

---

# Component overview

| Component         | Change                                 |
| ----------------- | -------------------------------------- |
| Enterprise Search | Remove entirely                        |
| Web crawler       | Replace with Open Crawler              |
| Sync tool         | Remove Enterprise Search calls         |
| Backend server    | Add search logic (Option B)            |
| Frontend code     | Simplify, remove recipe fetching       |
| Search config     | Export current recipe; store as config |
