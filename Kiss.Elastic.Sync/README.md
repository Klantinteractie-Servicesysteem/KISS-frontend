# KISS-Elastic-Sync

## Background
Two types of sources are indexed in Elasticsearch to allow them to be easily searched from KISS:
- Websites (by running this tool to set up a `crawler` in Enterprise Search)
- Structured sources (by scheduling this tool to synchronize data from the source to an `index` in Elasticsearch)

## When you first set up a source
This tool does the following:
1. Create a Enterprise Search `engine` for the source. For websites, a `crawler` is setup. For structured sources, an `index` is created and linked to the `engine`.
1. Create a `meta engine`. This is used to aggregate multiple sources. The `engine` from step 1 is linked to this `meta engine`.

## Supported structured sources
- SDG Producten
- Smoelenboek
- Vraag/antwoord combinaties (VAC)

## Relevance tuning
You can use `Relevance tuning` from Kibana on the `meta engine`.
