# Configuratie: Environment Variabelen

De configuratie van KISS is beschreven in de {{ helm_readme_link }}. Die README is de enige plek waar alle instellingen staan, met per instelling:

- de helm value (bijvoorbeeld `settings.oidc.authority`);
- de bijbehorende **environment variabele** (bijvoorbeeld `OIDC_AUTHORITY`), voor wie KISS zonder de helm chart installeert of lokaal draait;
- uitleg, standaardwaarden en voorbeelden.

De link hierboven verwijst naar de README van dezelfde versie als deze documentatie ({{ helm_readme_version }}).

**Andere versie van KISS?** Vervang dan `<versie>` in deze URL door je KISS-versie (bijvoorbeeld `v3.2.2`):

```text
https://github.com/Klantinteractie-Servicesysteem/KISS-frontend/blob/<versie>/helm/kiss-chart/README.md
```

Hieronder staat achtergrondinformatie die helpt om de configuratie te begrijpen.

## Gekoppelde bronnen

Er zijn diverse bronnen die vanuit KISS via API's bevraagd worden. Sommige worden alleen geraadpleegd, zoals de KvK-API en de API voor Haal Centraal BRP Personen bevragen, en de Objecten API voor het ophalen van Afdelingen, Groepen en Medewerkers. Andere registraties worden niet alleen geraadpleegd, maar er worden ook gegevens in weggeschreven. Dit zijn in ieder geval een Klanten- en Contactmomentenregister, zoals Open Klant, het Objecten Register zoals Open Objecten, en een Zaaksysteem (m.b.v. ZGW API's) zoals Open Zaak.

Daarnaast zijn er bronnen die binnen KISS doorzocht moeten worden. Die worden via Elasticsearch ontsloten:

- KISS gebruikt de Objecten API om Kennisartikelen (PDC-producten) mee op te halen, en naar Elastic te pushen.
- KISS gebruikt op dit moment de Objecten API om de Medewerkers in het Smoelenboek op te halen en naar Elastic te pushen.
- KISS gebruikt op dit moment de Objecten API om de Vraag Antwoord Combinaties (VAC) op te halen en naar Elastic te pushen.
- SharePoint-sites en websites kunnen ook als bron worden toegevoegd.

KISS-Elastic-Sync is het component dat zorgt dat deze bronnen naar de juiste indexen in Elasticsearch worden gepushed, met de benodigde gegevens hieraan toegevoegd. Websites worden gecrawld door de Elastic Open Crawler. Zie ook [Elasticsearch](../configuration/elastic.md).

## Rollen

KISS kent vier rollen. Welke rol een medewerker heeft, wordt bepaald door de rol-claims van de Identity Provider, of door [PABC](pabc.md) als die koppeling is ingericht. Zie ook [Configuratie van uw Identity Provider](../configuration/identity-provider.md).

| Rol | Wat kan een medewerker met deze rol |
| --- | --- |
| Klantcontactmedewerker | Contactmomenten en Contactverzoeken registreren, en zoeken in bronnen. |
| Redacteur | De inhoud bewerken van Nieuws- en Werkberichten, Links en VAC's (indien geconfigureerd). Zie de [Handleiding beheer KISS](../manual/manual.rst). |
| Beheerder | De inhoud bewerken van Skills, Kanalen, Gespreksresultaten en Contactverzoek formulieren. Zie de [Handleiding beheer KISS](../manual/manual.rst). |
| Kennisbank | Alléén zoeken in de geconfigureerde bronnen zoals Kennisbank, VAC, Smoelenboek en websites. |

## Managementinformatie

Met `MANAGEMENTINFORMATIE_API_KEY` stel je het secret in waarmee KISS het JWT token valideert bij het opvragen van contactmomentdetails. Zie de [Handleiding beheer KISS, hoofdstuk managementinformatie](../manual/managementinformatie.md) voor informatie over het gebruik van deze API.
