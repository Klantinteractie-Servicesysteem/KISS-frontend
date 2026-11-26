# Configuratie

De configuratie van KISS is beschreven in de {{ helm_readme_link }}. Die README is de enige plek waar alle instellingen staan, met per instelling:

- de helm value (bijvoorbeeld `settings.oidc.authority`);
- de bijbehorende **environment variabele** (bijvoorbeeld `OIDC_AUTHORITY`), voor wie KISS zonder de helm chart installeert of lokaal draait;
- uitleg, standaardwaarden en voorbeelden.

De link hierboven verwijst naar de README van dezelfde versie als deze documentatie ({{ helm_readme_version }}).

**Andere versie van KISS?** Vervang dan `<versie>` in deze URL door je KISS-versie (bijvoorbeeld `vX.Y.Z`):

```text
https://github.com/Klantinteractie-Servicesysteem/KISS-frontend/blob/<versie>/helm/kiss-chart/README.md
```

Gebruik je een versie van vóór deze wijziging? Dan staat de volledige configuratie nog op deze pagina in de documentatie van die versie. Kies die versie via het versiemenu van Read the Docs.

Hieronder staat achtergrondinformatie die helpt om de configuratie te begrijpen.

## Gekoppelde bronnen

Er zijn diverse bronnen die vanuit KISS via API's bevraagd worden. Sommige worden alleen geraadpleegd, zoals de KvK-API en de API voor Haal Centraal BRP Personen bevragen, en de Objecten API voor het ophalen van Afdelingen, Groepen en Medewerkers. Andere registraties worden niet alleen geraadpleegd, maar er worden ook gegevens in weggeschreven. Dit zijn in ieder geval een Klanten- en Contactmomentenregister, zoals Open Klant, het Objecten Register zoals Open Objecten, en een Zaaksysteem (m.b.v. ZGW API's) zoals Open Zaak.

Daarnaast zijn er bronnen die binnen KISS doorzocht moeten worden. Die worden via Elasticsearch ontsloten:

- KISS gebruikt de Objecten API om Kennisartikelen (PDC-producten) mee op te halen, en naar Elastic te pushen.
- KISS gebruikt op dit moment de Objecten API om de Medewerkers in het Smoelenboek op te halen en naar Elastic te pushen.
- KISS gebruikt op dit moment de Objecten API om de Vraag Antwoord Combinaties (VAC) op te halen en naar Elastic te pushen.
- SharePoint-sites en websites kunnen ook als bron worden toegevoegd.

KISS-Elastic-Sync is het component dat zorgt dat deze bronnen naar de juiste indexen in Elasticsearch worden gepushed, met de benodigde gegevens hieraan toegevoegd. Websites worden gecrawld door de Elastic Open Crawler. Zie ook [Elasticsearch](../configuration/elastic.md).
