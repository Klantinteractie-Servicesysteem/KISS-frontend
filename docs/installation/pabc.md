# PABC Koppeling (Platform Autorisatie Beheer Component)

## Overzicht

KISS kan optioneel worden gekoppeld met het [Platform Autorisatie Beheer Component (PABC)](https://github.com/Platform-Autorisatie-Beheer-Component/PABC-API). Deze koppeling wordt gebruikt voor twee, onafhankelijke doeleinden:

1. **Applicatierollen van KISS bepalen bij het inloggen** — welke KISS-rollen (Redacteur, Beheerder, Klantcontactmedewerker, Kennisbank) een ingelogde gebruiker heeft.
2. **Zaaktype-filtering** — op basis van de rollen van een ingelogde gebruiker bepalen welke zaaktypes deze gebruiker mag inzien.

Beide gebruiken dezelfde PABC-configuratie (`PABC_BASE_URL` / `PABC_API_KEY`) en dezelfde applicatienaam (`kiss`), maar zijn functioneel gescheiden.

## Applicatierollen bepalen bij inloggen

### Zonder PABC (standaard werking)

Als PABC **niet** geconfigureerd is, worden de applicatierollen van een ingelogde gebruiker op exact dezelfde manier bepaald als voorheen: KISS leest de rol-claims die de Identity Provider meegeeft, en vergelijkt deze met de geconfigureerde rolnamen (`OIDC_REDACTEUR_ROLE`, `OIDC_BEHEERDER_ROLE`, `OIDC_KLANTCONTACTMEDEWERKER_ROLE`, `OIDC_KENNISBANK_ROLE`, zie [configuratie.md](configuratie.md)).

### Met PABC

Als PABC **wel** geconfigureerd is, wordt de oude rol-configuratie van KISS genegeerd. In plaats daarvan gebeurt het volgende, direct na het inloggen:

1. KISS stuurt de functionele rollen van de zojuist ingelogde gebruiker (de rol-claims van de Identity Provider) naar PABC.
2. PABC geeft de applicatierollen terug die bij die functionele rollen horen, voor de applicatie `kiss`.
3. Alleen de applicatierollen die **niet gekoppeld zijn aan een specifiek zaaktype** (d.w.z. rollen zonder entity type/domein) tellen mee als KISS-applicatierol. Zaaktype-specifieke rollen zijn alleen relevant voor de zaaktype-filtering (zie hieronder), niet voor de KISS-applicatierollen.
4. De rol-claims van de gebruiker worden vervangen door claims voor de gevonden KISS-applicatierollen. Alle bestaande autorisatiechecks in KISS (bijv. `IsRedacteur`, `IsKcm`, `IsKennisbank`, de permissie-configuratie) werken hierdoor ongewijzigd door, alleen de bron van de rol-claims verandert.

## Zaaktype-filtering

De PABC-koppeling maakt het ook mogelijk om op basis van de rollen van een ingelogde gebruiker te bepalen welke zaaktypes deze gebruiker mag inzien.

### Architectuur

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

| Concept             | Uitleg                                                                                                                                                                                                                                                                                                                                 |
| ------------------- | -------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| **Functionele rol** | Een rol die door de Identity Provider wordt toegekend aan een gebruiker (bijv. "Klantcontactmedewerker", "Behandelaar"). Dit zijn de rollen die de gemeente zelf beheert.                                                                                                                                                              |
| **Applicatierol**   | Een rol die specifiek is voor een applicatie. In KISS zijn dit de rollen `Redacteur`, `Beheerder`, `Klantcontactmedewerker` en `Kennisbank` (zie hierboven), en voor zaaktype-filtering specifiek `klantcontactmedewerker-zaaktype-filter`. In PABC wordt geconfigureerd welke functionele rollen toegang geven tot een applicatierol. |
| **Applicatienaam**  | De naam waaronder KISS geregistreerd staat in PABC: `kiss`.                                                                                                                                                                                                                                                                            |
| **Entity type**     | Een type object waartoe de autorisatie betrekking heeft. In het geval van zaaktype-filtering zijn dit zaaktypes. Applicatierollen zonder entity type gelden juist voor de KISS-applicatierollen (zie hierboven).                                                                                                                       |

## Feature Flag

De PABC-koppeling (voor zowel applicatierollen bij inloggen als zaaktype-filtering) wordt geactiveerd door de **aanwezigheid** van de environment variabelen `PABC_BASE_URL` én `PABC_API_KEY`. Als één of beide ontbreken, werkt KISS zoals voorheen: rollen op basis van OIDC-rolclaims, zonder zaaktype-filtering.

**Let op:** Als de feature flag actief is maar PABC nog niet correct is ingericht (geen applicatierollen gekoppeld aan de juiste functionele rollen), dan krijgt geen enkele gebruiker een KISS-applicatierol en ziet niemand zaken. Richt daarom eerst PABC in, en deploy daarna pas KISS met de PABC-configuratie.

## Environment Variabelen

| Variabele                            | Verplicht | Uitleg                                                                                                                                                |
| ------------------------------------ | --------- | ----------------------------------------------------------------------------------------------------------------------------------------------------- |
| `PABC_BASE_URL`                      | Ja\*      | De base URL van de PABC API, zonder trailing slash. Bijvoorbeeld: `https://pabc.mijngemeente.nl`                                                      |
| `PABC_API_KEY`                       | Ja\*      | De API key voor authenticatie bij PABC (wordt meegestuurd als `X-API-KEY` header)                                                                     |
| `REGISTERS__X__ZAAKSYSTEEM_USE_PABC` | Nee       | Per zaaksysteem instellen of PABC-filtering wordt toegepast. Default: `true`. Zet op `false` voor zaaksystemen met eigen autorisatie (bijv. e-Suite). |

\* Verplicht als je de PABC-koppeling wilt activeren. Afwezigheid van deze variabelen schakelt de feature uit.

De applicatienaam (`kiss`) en de applicatierol voor zaaktype-filtering (`klantcontactmedewerker-zaaktype-filter`) zijn hardcoded in KISS ([PabcConfig.cs](../../Kiss.Bff/Extern/Pabc/PabcConfig.cs)).

## PABC Inrichting

Volg deze stappen om PABC in te richten voor gebruik met KISS:

### 1. Registreer KISS als applicatie in PABC

Maak een applicatie aan in PABC met de naam `kiss`.

### 2. Maak applicatierollen aan

Maak binnen de KISS-applicatie de applicatierollen aan die je nodig hebt:

- Voor **KISS-applicatierollen bij inloggen**: `Redacteur`, `Beheerder`, `Klantcontactmedewerker`, `Kennisbank` (de standaard KISS-rolnamen — zie hierboven, niet hernoemen). Deze rollen mag je **niet** koppelen aan een zaaktype/entity type in PABC, anders tellen ze niet mee als KISS-applicatierol.
- Voor **zaaktype-filtering**: een applicatierol met de naam `klantcontactmedewerker-zaaktype-filter`, gekoppeld aan de gewenste zaaktypes.

**Let op:** dit moeten twee aparte applicatierollen zijn, ook als ze aan dezelfde functionele rol gekoppeld worden. Eén PABC-mapping kan niet tegelijk "zonder entity type" én "gekoppeld aan (alle) zaaktypes" zijn voor dezelfde combinatie van functionele rol en applicatierol — dat is een tegenstrijdige configuratie die PABC niet toestaat.

### 3. Configureer zaaktypes

Voeg de zaaktypes toe die in KISS zichtbaar moeten zijn als entity types (type: `zaaktype`). Gebruik hierbij de **omschrijving** van het zaaktype zoals die in het zaaksysteem (catalogi API) bekend is.

### 4. Koppel functionele rollen

Koppel de functionele rollen uit je Identity Provider aan de gewenste applicatierollen van KISS:

- Koppel aan de rollen `Redacteur`/`Beheerder`/`Klantcontactmedewerker`/`Kennisbank` **zonder** zaaktype, zodat gebruikers de bijbehorende KISS-applicatierol krijgen bij het inloggen.
- Koppel aan de rol `klantcontactmedewerker-zaaktype-filter` **met** de gewenste zaaktypes, zodat bepaald wordt welke gebruikers welke zaaktypes mogen inzien.

**Tip:** De naam van de functionele rol in PABC moet exact overeenkomen met de rolnaam zoals die door de Identity Provider wordt meegegeven.

## Functionele Gevolgen

Wanneer de PABC-koppeling actief is:

- **Zaak zoeken:** Alleen zaken van toegestane zaaktypes worden getoond in zoekresultaten.
- **Klantbeeld (Zaken tab):** Alleen zaken van toegestane zaaktypes worden getoond bij een klant.
- **Melding:** De gebruiker ziet een melding dat mogelijk niet alle zaken zichtbaar zijn vanwege autorisatie-instellingen.
- **Geen toegang:** Als een gebruiker geen functionele rol heeft die in PABC is gekoppeld aan de KISS-applicatierol, dan ziet deze gebruiker geen enkele zaak.

## Meerdere zaaksystemen (e-Suite)

Wanneer KISS is gekoppeld met meerdere zaaksystemen (bijv. OpenZaak én e-Suite), kan het wenselijk zijn om PABC-filtering alleen toe te passen op specifieke zaaksystemen. De e-Suite heeft een eigen autorisatiemechanisme, waardoor PABC-filtering daar niet nodig is en dubbel beheer zou opleveren.

Gebruik hiervoor de `REGISTERS__X__ZAAKSYSTEEM_USE_PABC` variabele:

```
REGISTERS__0__ZAAKSYSTEEM_USE_PABC=false  # e-Suite: eigen autorisatie
REGISTERS__1__ZAAKSYSTEEM_USE_PABC=true   # OpenZaak: PABC autorisatie
```

Als deze variabele niet is ingesteld, wordt standaard `true` aangenomen (PABC-filtering is actief).

## Meer informatie

- [PABC API Documentatie](https://pabc-api.readthedocs.io/)
- [PABC GitHub Repository](https://github.com/Platform-Autorisatie-Beheer-Component/PABC-API)
- [PABC API Specificatie (OpenAPI)](https://redocly.github.io/redoc/?url=https://raw.githubusercontent.com/Platform-Autorisatie-Beheer-Component/PABC-API/refs/heads/main/PABC.Server/PABC.Server.json)
