# KISS-Elastic-Sync

## Background
Two types of sources are indexed in Elasticsearch to allow them to be easily searched from KISS:
- Websites (by running this tool to set up a `crawler` in Enterprise Search)
- Structured sources (by scheduling this tool to synchronize data from the source to an `index` in Elasticsearch)

## When you first set up a source
This tool does the following automatically:
1. Create a Enterprise Search `engine` for the source. For websites, a `crawler` is created and run. For structured sources, an `index` is created and linked to the `engine`.
1. Create a `meta engine`. This is used to aggregate multiple sources. The `engine` from step 1 is linked to this `meta engine`.

## Relevance tuning
You can use `Relevance tuning` from Kibana on the `meta engine`.

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
| `domain https://www.mywebsite.nl` | Crawl the website https://www.mywebsite.nl |


## Environment variables
### Variables for the Elastic stack
| Variable | Description |
| --- | --- |
| ELASTIC_BASE_URL | Base url for Elasticsearch |
| ELASTIC_USERNAME | Username to log in to Elasticsearch. This can be the default root user `elastic` or a dedicated user you've created yourself |
| ELASTIC_PASSWORD | Password for the username above. If you're using [ECK](https://www.elastic.co/guide/en/cloud-on-k8s/2.8/k8s-overview.html), you can find the password for the default user using the command `kubectl get secret kiss-es-elastic-user -o go-template='{{.data.elastic | base64decode}}'` |
| ENTERPRISE_SEARCH_BASE_URL | Base url for Enterprise Search. This url is different from the Elasticsearch url |
| ENTERPRISE_SEARCH_ENGINE | The name of the `meta-engine` that will be used |
| ENTERPRISE_SEARCH_PRIVATE_API_KEY | An API key to maintain the `engine`s. You can find this in Kibana at the url `app/enterprise_search/app_search/credentials` |

### Variables for the different sources
| Variable | Description |
| --- | --- |
| SDG_BASE_URL | The base url for the SDG API to retrieve products |
| SDG_API_KEY | The API key to connect to the SDG API |
| MEDEWERKER_OBJECTEN_BASE_URL | The base url for the Objects API to retrieve mededewerkers (smoelenboek) |
| MEDEWERKER_OBJECTEN_TOKEN | The token to connect to the Objects API to retrieve medewerkers (smoelenboek) |
| MEDEWERKER_OBJECTTYPES_BASE_URL | The base url for the Object Types API to retrieve the medewerker object type (smoelenboek) |
| MEDEWERKER_OBJECTTYPES_TOKEN | The token to connect to the Object Types API to retrieve the medewerker object type (smoelenboek) |
| VAC_OBJECTEN_BASE_URL | The base url for the Objects API to retrieve VACs |
| VAC_OBJECTEN_TOKEN | The token to connect to the Objects API to retrieve VACs |
| VAC_OBJECTTYPES_BASE_URL | The base url for the Object Types API to retrieve the VAC object type |
| VAC_OBJECTTYPES_TOKEN | The token to connect to the Object Types API to retrieve the VAC object type |

## Run locally
1. Make a copy of .env.local.example, rename it .env.local and fill in the required secrets.
2. Set up a port forward for Enterprise Search, e.g.: `kubectl port-forward service/kiss-ent-http 3002`
3. Set up a port forward for Elasticsearch, e.g.: `kubectl port-forward service/kiss-es-http 9200`
4. Build the tool using docker-compose: `docker compose build`
4. Run the tool using docker-compose: `docker compose --env-file ./.env.local run kiss.elastic.sync [ARGS...]`