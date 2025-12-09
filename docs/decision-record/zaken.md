# Zaken

## Het zaakdetailscherm

In onderstaande afbeelding is gedocumenteerd welke gegevens uit de verschillende ZGW-objecten worden gebruikt in het Zaak-detailscherm. 

![image](https://raw.githubusercontent.com/Klantinteractie-Servicesysteem/.github/refs/heads/main/docs/images/MappingZaakDetail.png)

## Koppeling van een zaak aan een Contactmoment

Binnen de Klantinteracties API wordt de koppeling van een Zaak aan een klantcontact gelegd m.b.v. een `onderwerpobjectidentificator`. Bij het leggen van de koppeling richt KISS het onderwerpobject als volgt in: 

```json
{
   "onderwerpobjectidentificator": {
       "objectId": "12345678-abcd-1234-efgh-1234567890ab", // het uuid van de gekoppelde zaak
       "codeObjecttype": "zgw-Zaak",
       "codeRegister": "openzaak",
       "codeSoortObjectId": "uuid"
   }
}
```

### Zaakinformatie in het Contactmoment

KISS toont het Zaaknummer van de zaak die is gekoppeld aan een Contactmoment. Hiervoor kijkt KISS naar de waarde `"codeObjecttype": "zgw-Zaak"` én `"codeRegister": "openzaak"`. Dit om te voorkomen dat een uuid wordt gezocht dat niet bestaat in de gekoppelde Open Zaak instantie.


## Zaken zoeken bij bedrijven zonder vestigingsnummer
- **Probleem**
Binnen de ZGW-API's kun je zaken ophalen bij bedrijven zonder vestigingsnummer door te kijken naar betrokkenen bij een zaak met `"betrokkeneType": "niet_natuurlijk_persoon"`. Het identificerende kenmerk van deze betrokkene staat in `betrokkeneIdentificatie.InnNnpId`. De Zaken API geeft als omschrijving: "Het door een kamer toegekend uniek nummer voor de INGESCHREVEN NIET-NATUURLIJK PERSOON". Maar de specificatie geeft niet aan welk uniek nummer je hiervoor moet gebruiken, rsin of KvK-nummer. 
Bij het koppelen met verschillende zaakregistratiesystemen zien we dat het ene systeem in `betrokkeneIdentificatie.InnNnpId` een rsin verwacht, en een andere een KvK-nummer, maar KISS weet niet welk van deze gegevens ze per zaakregistratiesysteem moet gebruiken om naar zaken te zoeken. 

- **Oplossingsrichtingen**: We kunnen per gekoppelde zaakregistratie configurabel maken welk gegeven het systeem verwacht in `betrokkeneIdentificatie.InnNnpId`. We kunnen ook naar elke zaakregistratie beide opties proberen en zien welke een resultaat geeft. Een bijkomend probleem daarbij is dat in sommige gevallen het registratiesysteem een 400 bad request teruggeeft, als het gegeven niet voldoet aan het format van het gekozen gegeven. Dat kan vervuiling van de log opleveren. 
  
- **Keuze**: We kiezen ervoor altijd op beide manieren het register te bevragen. We hebben er hierbij wel voor gezorgd dat eventuele bad request errors vanuit de zaakregistratie niet gelogd worden.
  
- **Overwegingen**: Elke featureswitch levert complexiteit in code en beheer op. In dit geval vereist het correct instellen van de switch bovendien veel kennis van de gekoppelde zaakregistratiesystemen. Door niet met een featureswitch te werken, maar gewoon beide opties te proberen, blijft de code overzichtelijker. Er moeten er wel redundante calls gedaan worden, maar we verwachten geen significante performance problemen. De gekozen oplossingsrichting is bovendien meer in lijn met de werkwijze bij vergelijkbare situtaties, waardoor de code uniformer en begrijpelijk blijft. 
 
