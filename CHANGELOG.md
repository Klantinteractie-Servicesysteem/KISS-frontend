# Change history

## vx.x.x

### New features

### Warnings and deployment notes

De configuratie van de registers voor klantinteracties en zaken is sterk gewijzigd. Het is nu mogelijk om registers aan elkaar te koppelen. Per samenstelling van registers dient er een `systeem` geconfigureerd te worden. De variabelen rondom een specifiek systeem heeft steeds REGISTERS**`index`** als prefix. Voor de esuite (in combinatie met de podiumd-adapter) zijn de variabelnamen anders dan voor OpenKlant2 / OpenZaak. Van precies 1 systeem moet worden aangegeven dat het het default systeem is. Dit houdt in dat dit systeem gebruikt wordt voor contactmomenten en contactverzoeken die niet gekoppeld zijn aan een zaak, en dat dit het systeem is waar de klantinformatie primair wordt opgeslagen.

#### Voorbeeld van een configuratie met zowel de Esuite als OpenKlant

Het eerste systeem hieronder is OpenKlant2 met Openzaak. Het tweede systeem is de Esuite in combinatie met de podiumd-adapter.

```sh
REGISTERS__0__IS_DEFAULT=true
REGISTERS__0__KLANTINTERACTIE_BASE_URL=https://openklant2.mijn-gemeente.nl/klantinteracties
REGISTERS__0__REGISTRY_VERSION=OpenKlant2
REGISTERS__0__KLANTINTERACTIE_TOKEN=xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx
REGISTERS__0__ZAAKSYSTEEM_BASE_URL=https://openzaak.mijn-gemeente.nl
REGISTERS__0__ZAAKSYSTEEM_API_KEY=xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx
REGISTERS__0__ZAAKSYSTEEM_API_CLIENT_ID=client-id-van-kiss-in-open-zaak
REGISTERS__0__ZAAKSYSTEEM_DEEPLINK_URL=https://zaaksysteem.mijn-gemeente.nl
REGISTERS__0__ZAAKSYSTEEM_DEEPLINK_PROPERTY=identificatie

REGISTERS__1__IS_DEFAULT=false
REGISTERS__1__CONTACTMOMENTEN_BASE_URL=https://podiumd-adapter.mijn-gemeente.nl
REGISTERS__1__REGISTRY_VERSION=OpenKlant1
REGISTERS__1__CONTACTMOMENTEN_API_CLIENT_ID=client-id-van-kiss-in-podiumd-adapter
REGISTERS__1__CONTACTMOMENTEN_API_KEY=xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx
REGISTERS__1__INTERNE_TAAK_BASE_URL=https://podiumd-adapter.mijn-gemeente.nl
REGISTERS__1__INTERNE_TAAK_CLIENT_ID=client-id-van-kiss-in-podiumd-adapter
REGISTERS__1__INTERNE_TAAK_CLIENT_SECRET=xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx-
REGISTERS__1__INTERNE_TAAK_OBJECT_TYPE_URL=https://objecttypen.mijn-gemeente.nl/api/v2/objecttypes/1df73259-1a58-4180-bf98-598eefc184d4
REGISTERS__1__INTERNE_TAAK_TYPE_VERSION=1
REGISTERS__1__KLANTEN_BASE_URL=https://podiumd-adapter.mijn-gemeente.nl/klanten
REGISTERS__1__KLANTEN_CLIENT_ID=client-id-van-kiss-in-podiumd-adapter
REGISTERS__1__KLANTEN_CLIENT_SECRET=xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx
REGISTERS__1__ZAAKSYSTEEM_BASE_URL=https://podiumd-adapter.mijn-gemeente.nl
REGISTERS__1__ZAAKSYSTEEM_API_KEY=xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx
REGISTERS__1__ZAAKSYSTEEM_API_CLIENT_ID=client-id-van-kiss-in-podiumd-adapter
REGISTERS__1__ZAAKSYSTEEM_DEEPLINK_URL=https://esuite.mijn-gemeente.nl/mp/zaak/
REGISTERS__1__ZAAKSYSTEEM_DEEPLINK_PROPERTY=identificatie
```

Hiermee komen de onderstaande release variabelen te vervallen.
De waardes blijven hetzelfde, maar deze moeten overgezet worden naar het bovenstaande stramien.
Let op, als je nog oudere versies van KISS wil kunnen blijven deployen, dan zal je de oude variablen voorlopig nog moeten laten staan naast de nieuwe variabelen.

```sh
CONTACTMOMENTEN_API_CLIENT_ID
CONTACTMOMENTEN_API_KEY
CONTACTMOMENTEN_BASE_URL

INTERNE_TAAK_BASE_URL
INTERNE_TAAK_CLIENT_ID
INTERNE_TAAK_CLIENT_SECRET
INTERNE_TAAK_OBJECT_TYPE_URL
INTERNE_TAAK_TOKEN
INTERNE_TAAK_TYPE_VERSION

KLANTEN_BASE_URL
KLANTEN_CLIENT_ID
KLANTEN_CLIENT_SECRET
KLANTINTERACTIES_BASE_URL
KLANTINTERACTIES_TOKEN

ZAAKSYSTEEM__n__API_CLIENT_ID
ZAAKSYSTEEM__n__API_KEY
ZAAKSYSTEEM__n__BASE_URL
ZAAKSYSTEEM__n__DEEPLINK_PROPERTY
ZAAKSYSTEEM__n__DEEPLINK_URL
ZAAKSYSTEEM__n__NIETNATUURLIJKPERSOONIDENTIFIER

ZAKEN_BASE_URL
ZAKEN_API_CLIENT_ID
ZAKEN_API_KEY
ZAAKSYSTEEM_DEEPLINK_URL
ZAAKSYSTEEM_DEEPLINK_PROPERTY

USE_KLANTINTERACTIES
```

### Bugfixes

### Maintenance
