# KISS-Elastic-Sync

## Introduction

KISS offers the posibility to search for information within specific sources. This search functionality is using Elasticsearch. The KISS-Elastic-Sync-tool is used to create the necessary engines in a an Elasticsearch installation.

Two types of sources are indexed in Elasticsearch to allow them to be easily searched from KISS:

- Websites (by running this tool to set up a `crawler` in Enterprise Search)
- Structured sources (by scheduling this tool to synchronize data from the source to an `index` in Elasticsearch)

## Run locally

1. Make a copy of .env.local.example, rename it .env.local and fill in the required secrets.
2. Set up a port forward for Enterprise Search, e.g.: `kubectl port-forward service/kiss-ent-http 3002`
3. Set up a port forward for Elasticsearch, e.g.: `kubectl port-forward service/kiss-es-http 9200`
4. Build the tool using docker-compose: `docker compose build`
5. Run the tool using docker-compose: `docker compose --env-file ./.env.local run kiss.elastic.sync [ARGS...]`

## When you first set up a source

This tool does the following automatically:

1. Create a Enterprise Search `engine` for the source. For websites, a `crawler` is created and run. For structured sources, an `index` is created and linked to the `engine`.
1. Create a `meta engine`. This is used to aggregate multiple sources. The `engine` from step 1 is linked to this `meta engine`.

## Relevance tuning

You can use `Relevance tuning` from Kibana on the `meta engine`. See also the [KISS-documenation (in Dutch)](https://kiss-klantinteractie-servicesysteem.readthedocs.io/en/latest/CONFIGURATIE/#configuratie-van-elasticsearch-voor-kiss).

## Supported structured sources

- SDG Producten
- Medewerkers (Smoelenboek)
- Vraag/antwoord combinaties (VAC)

## Commands

Examples of how to schedule a cron job in Kubernetes with these arguments [can be found here](deploy)
| Arguments | Description |
| --- | --- |
| No arguments | Sync SDG Producten |
| `vac` | Sync VACs |
| `smoelenboek` | Sync Medewerkers (Smoelenboek) |
| `sharepoint` | Sync a Sharepoint Site, including all SubSites |
| `domain https://www.mywebsite.nl` | Crawl the website https://www.mywebsite.nl |

## Environment variables

### Variables for the Elastic stack

| Variable                          | Description                                                                                                                                                                                                                                                           |
| --------------------------------- | --------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | ---------------- |
| ELASTIC_BASE_URL                  | Base url for Elasticsearch                                                                                                                                                                                                                                            |
| ELASTIC_USERNAME                  | Username to log in to Elasticsearch. This can be the default root user `elastic` or a dedicated user you've created yourself                                                                                                                                          |
| ELASTIC_PASSWORD                  | Password for the username above. If you're using [ECK](https://www.elastic.co/guide/en/cloud-on-k8s/2.8/k8s-overview.html), you can find the password for the default user using the command `kubectl get secret kiss-es-elastic-user -o go-template='{{.data.elastic | base64decode}}'` |
| ENTERPRISE_SEARCH_BASE_URL        | Base url for Enterprise Search. This url is different from the Elasticsearch url                                                                                                                                                                                      |
| ENTERPRISE_SEARCH_ENGINE          | The name of the `meta-engine` that will be used                                                                                                                                                                                                                       |
| ENTERPRISE_SEARCH_PRIVATE_API_KEY | An API key to maintain the `engine`s. You can find this in Kibana at the url `app/enterprise_search/app_search/credentials`                                                                                                                                           |

### Variables for the different sources

| Variable                          | Description                                                                                                                                                     |
| --------------------------------- | --------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| SDG_OBJECTEN_BASE_URL             | The base url for the Objects API to retrieve Producten                                                                                                          |
| SDG_OBJECT_TYPE_URL               | The full url of the object type for Producten                                                                                                                   |
| SDG_OBJECTEN_TOKEN                | The token to connect to the Objects API to retrieve Producten                                                                                                   |
| MEDEWERKER_OBJECTEN_BASE_URL      | The base url for the Objects API to retrieve mededewerkers (smoelenboek), or the PodiumD Adapter if applicable                                                  |
| MEDEWERKER_OBJECT_TYPE_URL        | The full url of the object type for medewerkers (smoelenboek)                                                                                                   |
| MEDEWERKER_OBJECTEN_TOKEN         | The token to connect to the Objects API to retrieve medewerkers (smoelenboek). Use this if you are NOT using the PodiumD Adapter                                |
| MEDEWERKER_OBJECTEN_CLIENT_ID     | The client id to generate a JWT to connect to the PodiumD Adapter to retrieve medewerkers (smoelenboek). This has to match a setting in the PodiumD Adapter     |
| MEDEWERKER_OBJECTEN_CLIENT_SECRET | The client secret to generate a JWT to connect to the PodiumD Adapter to retrieve medewerkers (smoelenboek). This has to match a setting in the PodiumD Adapter |
| VAC_OBJECTEN_BASE_URL             | The base url for the Objects API to retrieve VACs                                                                                                               |
| VAC_OBJECT_TYPE_URL               | The full url of the object type for VACs                                                                                                                        |
| VAC_OBJECTEN_TOKEN                | The token to connect to the Objects API to retrieve VACs                                                                                                        |
| SHAREPOINT_TENANT_ID              | The Azure AD tenant id for the SharePoint app registration (GUID). Example: bd7f67dd-fc8c-4a9d-a1aa-c2e4d687b18f                                                |
| SHAREPOINT_CLIENT_ID              | The client id (application id) of the SharePoint app registration. Example: 0166786a-df05-4e1c-a75c-7c3317002ad8                                                |
| SHAREPOINT_CLIENT_SECRET          | The client secret for the SharePoint app registration (sensitive).                                                                                              |
| SHAREPOINT_SITE_URL               | The full SharePoint site URL to crawl/sync. Example: https://my-tenant.sharepoint.com/sites/my-site                                                             |
| SHAREPOINT_SOURCE_NAME            | A human-friendly name for the SharePoint source, which will show up in the checkboxes in the search bar in KISS. Example: ICATT Sharepoint Test Site            |
