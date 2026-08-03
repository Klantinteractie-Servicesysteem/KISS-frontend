# PABC Koppeling (Platform Autorisatie Beheer Component)

## Overzicht

KISS kan optioneel worden gekoppeld met het [Platform Autorisatie Beheer Component (PABC)](https://github.com/Platform-Autorisatie-Beheer-Component/PABC-API). Deze koppeling maakt het mogelijk om op basis van de rollen van een ingelogde gebruiker te bepalen welke zaaktypes deze gebruiker mag inzien.

## Architectuur

De PABC-koppeling werkt als volgt:

1. Een gebruiker logt in bij KISS via de Identity Provider (bijv. Keycloak). De Identity Provider kent **functionele rollen** toe aan de gebruiker.
2. Wanneer de gebruiker zaken opvraagt, stuurt KISS de functionele rollen van de gebruiker naar de PABC API.
3. PABC geeft terug welke **applicatierollen** bij die functionele rollen horen, en voor welke **zaaktypes** (entity types) die applicatierollen gelden.
4. KISS filtert de resultaten: alleen zaken van toegestane zaaktypes worden getoond.

```
┌──────────┐     functionele rollen      ┌──────────┐
│          │ ──────────────────────────►  │          │
│   KISS   │                              │   PABC   │
│          │  ◄────────────────────────── │          │
└──────────┘   applicatierollen +         └──────────┘
               zaaktypes per rol
```

### Concepten

| Concept | Uitleg |
|---------|--------|
| **Functionele rol** | Een rol die door de Identity Provider wordt toegekend aan een gebruiker (bijv. "Klantcontactmedewerker", "Behandelaar"). Dit zijn de rollen die de gemeente zelf beheert. |
| **Applicatierol** | Een rol die specifiek is voor een applicatie. In KISS is dit `klantcontactmedewerker`. In PABC wordt geconfigureerd welke functionele rollen toegang geven tot deze applicatierol. |
| **Applicatienaam** | De naam waaronder KISS geregistreerd staat in PABC: `kiss`. |
| **Entity type** | Een type object waartoe de autorisatie betrekking heeft. In het geval van KISS zijn dit zaaktypes. |

## Feature Flag

De PABC-koppeling wordt geactiveerd door de **aanwezigheid** van de environment variabelen `PABC_BASE_URL` én `PABC_API_KEY`. Als één of beide ontbreken, werkt KISS zoals voorheen zonder zaaktype-filtering.

**Let op:** Als de feature flag actief is maar PABC nog niet correct is ingericht (geen zaaktypes gekoppeld aan de juiste applicatierol), dan ziet geen enkele gebruiker zaken. Richt daarom eerst PABC in, en deploy daarna pas KISS met de PABC-configuratie.

## Environment Variabelen

| Variabele | Verplicht | Uitleg |
|-----------|-----------|--------|
| `PABC_BASE_URL` | Ja* | De base URL van de PABC API, zonder trailing slash. Bijvoorbeeld: `https://pabc.mijngemeente.nl` |
| `PABC_API_KEY` | Ja* | De API key voor authenticatie bij PABC (wordt meegestuurd als `X-API-KEY` header) |

\* Verplicht als je de PABC-koppeling wilt activeren. Afwezigheid van deze variabelen schakelt de feature uit.

De applicatienaam (`kiss`) en applicatierol (`klantcontactmedewerker`) zijn hardcoded in KISS.

## PABC Inrichting

Volg deze stappen om PABC in te richten voor gebruik met KISS:

### 1. Registreer KISS als applicatie in PABC

Maak een applicatie aan in PABC met de naam `kiss`.

### 2. Maak een applicatierol aan

Maak binnen de KISS-applicatie een applicatierol aan met de naam `klantcontactmedewerker`.

### 3. Configureer zaaktypes

Voeg de zaaktypes toe die in KISS zichtbaar moeten zijn als entity types (type: `zaaktype`). Gebruik hierbij de **omschrijving** van het zaaktype zoals die in het zaaksysteem (catalogi API) bekend is.

### 4. Koppel functionele rollen

Koppel de functionele rollen uit je Identity Provider aan de applicatierol van KISS, met de gewenste zaaktypes. Hierdoor bepaal je welke gebruikers welke zaaktypes mogen inzien.

**Tip:** De naam van de functionele rol in PABC moet exact overeenkomen met de rolnaam zoals die door de Identity Provider wordt meegegeven.

## Functionele Gevolgen

Wanneer de PABC-koppeling actief is:

- **Zaak zoeken:** Alleen zaken van toegestane zaaktypes worden getoond in zoekresultaten.
- **Klantbeeld (Zaken tab):** Alleen zaken van toegestane zaaktypes worden getoond bij een klant.
- **Melding:** De gebruiker ziet een melding dat mogelijk niet alle zaken zichtbaar zijn vanwege autorisatie-instellingen.
- **Geen toegang:** Als een gebruiker geen functionele rol heeft die in PABC is gekoppeld aan de KISS-applicatierol, dan ziet deze gebruiker geen enkele zaak.

## Meer informatie

- [PABC API Documentatie](https://pabc-api.readthedocs.io/)
- [PABC GitHub Repository](https://github.com/Platform-Autorisatie-Beheer-Component/PABC-API)
- [PABC API Specificatie (OpenAPI)](https://redocly.github.io/redoc/?url=https://raw.githubusercontent.com/Platform-Autorisatie-Beheer-Component/PABC-API/refs/heads/main/PABC.Server/PABC.Server.json)
