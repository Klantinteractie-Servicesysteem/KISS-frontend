# Managementinformatie
Bij het opslaan van Contactmomenten worden enkele gegevens, die geen plek hebben in de standaarden rondom Klantinteracties, opgeslagen binnen KISS zelf. (zie ook het [onderdeel Contactmomentdetails in Decision Record](../decision-record/contactmomenten/contactmomentdetails)). Deze gegevens leveren managementinformatie over de werkzaamheden van het KCC. 

Binnen KISS is een API endpoint beschikbaar waarmee deze gegevens opgevraagd kunnen worden. Dit endpoint is benaderbaar op: `https://www.kissbijdegemeente.nl/api/contactmomentendetails`.

## Authenticatie

De Contactmomentendetails API is beveiligd en vereist een JWT Bearer Token voor toegang. Dit token wordt gegenereerd op basis van een geheime sleutel (secret) die door KISS wordt verstrekt aan de externe systemen. Dit secret is configureerbaar via de environment variabele `MANAGEMENTINFORMATIE_API_KEY` (zie installatiehandleiding)

### Structuur van de JWT

De JWT bestaat uit drie delen, gescheiden door punten:

1. **Header**: Bevat informatie over het algoritme dat gebruikt wordt om het token te ondertekenen, in dit geval HS256.
    ```json
    {
      "alg": "HS256",
      "typ": "JWT"
    }
    ```

2. **Payload**: De gegevens van de gebruiker of het systeem dat toegang probeert te verkrijgen. De payload bevat het `iat` (issued at) veld met de timestamp van het moment waarop het token is aangemaakt, en het `exp` (expiration) veld om de geldigheidsduur van het token te beperken.

    ```json
    {
      "iat": 1728580531,
      "exp": 1728666931
    }
    ```

    - `iat`: Tijdstip van aanmaak van het token (in UNIX-timestamp).
    - `exp`: Verloopdatum van het token (in UNIX-timestamp), na welke het token niet meer geldig is.

3. **Signature**: De handtekening, gegenereerd op basis van de header, payload, en een geheime sleutel (secret) die wij verstrekken. Dit zorgt ervoor dat het token niet kan worden gewijzigd door een derde partij.

> **Opmerking:** Het is aan te raden om de `exp` waarde zo in te stellen dat het token slechts een beperkte tijd geldig is, om het risico op misbruik te verkleinen.

### Headers voor de API-aanroep

Zorg ervoor dat je de volgende headers meestuurt met elke API-aanroep:

```plaintext
Authorization: Bearer {JWT-Token}
Accept: */*
Cache-Control: no-cache
```

Vervang `{JWT-Token}` door het daadwerkelijke token dat je hebt gegenereerd.

## Query Parameters

| Parameter  | Type   | Verplicht? | Standaard  | Omschrijving                                               |
|------------|--------|------------|------------|------------------------------------------------------------|
| `from`     | string | Ja         | -          | De startdatum in ISO 8601 formaat (bijv. `yyyy-MM-ddTHH:mm:ssZ`). |
| `to`       | string | Ja         | -          | De einddatum in ISO 8601 formaat (bijv. `yyyy-MM-ddTHH:mm:ssZ`). |
| `pageSize` | int    | Nee        | 5000       | Het aantal resultaten per pagina, maximaal 5000.             |
| `page`     | int    | Nee        | 1          | De pagina van de resultaten die moet worden opgehaald.       |

## Voorbeeld Request

```
GET /api/contactmomentendetails?from=2024-10-01T00:00:00Z&to=2024-10-10T23:59:59Z&pageSize=100&page=1
Authorization: Bearer {JWT-Token}
```

Let erop dat de `Authorization` header met de waarde `Bearer {JWT-Token}` wordt toegevoegd bij elk request.
