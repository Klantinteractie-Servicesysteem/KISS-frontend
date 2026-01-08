# Voorbereidingen / Benodigdheden

## Authenticatie

De authenticatie van gebruikers binnen KISS gebeurt m.b.v. een OIDC koppeling met bijvoorbeeld Azure Active Directory.

## Postgresql Database

KISS heeft een postgresql database nodig voor het bijhouden van verschillende interne gegevens. De helm chart installeert geen postgresql database mee. Voor het hosten van een postgresql in Kubernetes raden wij [CloudNativePG](https://github.com/cloudnative-pg/cloudnative-pg) aan.

## Elasticsearch, Kibana en Enterprise Search

KISS gebruikt Elasticsearch om verschillende gegevens doorzoekbaar te maken, zoals bijvoorbeeld medewerkers, afdelingen en groepen. Enterprise Search wordt gebruikt om gemeentelijke websites te crawlen. Kibana kan gebruikt worden om de weging van zoekresultaten te finetunen. Om historische redenen hebben we in het verleden [een eigen helm chart](https://github.com/Klantinteractie-Servicesysteem/.github/tree/main/docs/scripts/elastic) aangeboden om deze applicaties in een kubernetes cluster te installeren. Deze wordt echter niet langer onderhouden en we raden aan om gebruik te maken van [de officiele helm charts van Elasticsearch](https://github.com/elastic/cloud-on-k8s).

## Registers

KISS maakt verder gebruik van verschillende registers:

- Een zaakregister dat voldoet aan de ZGW-standaard (of de eSuite)
- Een contactmoment/contactverzoek register dat voldoet aan de klantinteractie-standaard (of de eSuite)
- Een BRP-register dat voldoet aan de HaalCentraal-standaard
- Een KvK-register dat voldoet aan de KvK-API-standaard
- Een Objectenregister dat voldoet aan de api specs van de [Objects Api](https://github.com/maykinmedia/objects-api)

## Helm / Kubernetes

KISS is ontwikkeld om in een Kubernetes cluster te draaien. We gaan er in de handleiding van uit dat je dat hebt, en dat je weet hoe je een helm chart installeert. Er is een [helm chart](https://github.com/Klantinteractie-Servicesysteem/KISS-frontend/tree/main/helm/kiss-chart) beschikbaar die je kunt gebruiken, rechtstreeks of ter inspiratie.
