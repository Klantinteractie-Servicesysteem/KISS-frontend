# Installatie

Als je KISS installeert gaan we er vanuit dat je [de bijbehorende helm chart](https://github.com/Klantinteractie-Servicesysteem/KISS-frontend/tree/main/helm/kiss-chart) gebruikt. Als je KISS op een andere manier wilt installeren, kan deze als inspiratie dienen.

**LET OP**

- Voordat een ingelogde gebruiker kan werken met KISS, moet deze gebruiker de juiste rol hebben in de gekoppelde Identity provider. Zie voor meer informatie het onderdeel [Configuratie van uw Identity Provider in de configuratie-handleiding](../configuration/identity-provider.md).
- Om een betere indruk te krijgen van hoe KISS werkt, is het mogelijk om **voorbeeldata (demodata)** te laden. Zie hiervoor [de uitleg bij de Beheerhandleiding](../manual/voorbeelddata.md).

## KISS-Elastic-Sync

KISS-Elastic-Sync is het component dat zorgt voor het creëren van de benodigde engines in een Elasticsearch-installatie, zodat gekoppelde bronnen eenvoudig door KISS doorzoekbaar zijn. Het ondersteunt zowel websites als gestructureerde bronnen door respectievelijk een crawler en een index te gebruiken. Deze component wordt meegeïnstalleerd met de helm chart van KISS, en aangeroepen door middel van cron jobs.
