# Installatie

## Placeholders

De yaml-voorbeeldbestanden staan [hier](https://github.com/Klantinteractie-Servicesysteem/KISS-frontend/blob/main/helm/kiss-frontend/kiss.template.yaml).

## Uitvoeren

De installatie kan uitgevoerd worden middels het PowerShell script. Handmatig uitvoeren kan ook.

[install_kiss.ps1](https://github.com/Klantinteractie-Servicesysteem/KISS-frontend/blob/main/docs/scripts/install_kiss.ps1)

**LET OP**

- Voordat een ingelogde gebruiker kan werken met KISS, moet deze gebruiker de juiste rol hebben in de gekoppelde Identity provider. Zie voor meer informatie het onderdeel [Configuratie van uw Identity Provider in de configuratie-handleiding](../configuration/identity-provider.md).
- Om een betere indruk te krijgen van hoe KISS werkt, is het mogelijk om **voorbeeldata (demodata)** te laden. Zie hiervoor [de uitleg bij de Beheerhandleiding](../manual/voorbeelddata.md).

### KISS-Elastic-Sync
KISS-Elastic-Sync is het component dat zorgt voor het creëren van de benodigde engines in een Elasticsearch-installatie, zodat gekoppelde bronnen eenvoudig door KISS doorzoekbaar zijn. Het ondersteunt zowel websites als gestructureerde bronnen door respectievelijk een crawler en een index te gebruiken.

Meer informatie over de KISS-Elastic-Sync tool en hoe deze te installeren, is te vinden op de volgende URL:
[KISS-Elastic-Sync](https://github.com/Klantinteractie-Servicesysteem/KISS-Elastic-Sync/blob/main/README.md)

#### Cronjobs
Naast de sync tool zijn er ook cronjobs die ingesteld moeten worden voor het regelmatig synchroniseren van data.

Meer informatie over de benodigde cronjobs en hoe deze in te stellen, is te vinden op de volgende URL:
[KISS-Elastic-Sync Cronjobs](https://github.com/Klantinteractie-Servicesysteem/KISS-Elastic-Sync/blob/main/deploy/README.md)
