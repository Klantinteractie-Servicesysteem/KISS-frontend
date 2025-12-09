## Contactmomentdetails

KISS registreert bij Contactmomenten enkele gegevens, die geen plek hebben binnen de standaard voor Contactmomenten: de Klantcontacten binnen Klantinteracties. Voor de onderstaande gegevens is tijdens fase 1C de afweging gemaakt, en besloten de gegevens op te slaan binnen KISS zelf. Het gaat om gegevens van een contactoment die niet in het Contactmomentenregister passen. Deze gegevens leveren managementinformatie over de werkzaamheden van het KCC. Zie ook de issues  [#21](https://github.com/Klantinteractie-Servicesysteem/KISS-frontend/issues/21),  [#111](https://github.com/Klantinteractie-Servicesysteem/KISS-frontend/issues/111),  [#610](https://github.com/Klantinteractie-Servicesysteem/KISS-frontend/issues/610), [#611](https://github.com/Klantinteractie-Servicesysteem/KISS-frontend/issues/611).


Bij de overgang van de Contactmomenten API naar de Klantinteracties API is hier aan toegevoegd: de informatie over geraadpleegde bronnen.

Deze gegevens worden opgeslagen binnen KISS, en zijn op te vragen via de Contactmomentdetails API. Zie de [Handleiding beheer KISS, hoofdstuk managementinformatie](../manual/managementinformatie.md) voor informatie over het gebruik van deze API.


| Property  | Type | Toelichting |
|--------|--------|--------|
| `vraag` | string  | Tijdens een Contactmoment kan de KCM verschillende bronnen raadplegen. Bij de afronding kan een KCM één van de bronnen selecteren bij "Vraag". De titel van deze bron wordt opgeslagen in `vraag`. Bij VAC's is dit de vraag. Bij Kennisartikelen / productpagina's is dit de titel van het artikel plus één van de geraadpleegde subonderdelen binnen het artikel. Deze wordt in KISS ook gebruikt in de weergave van Contactmomenten: hiermee kan een KCM zien wat de vraag van de klant was. |
| `specifiekeVraag` | string | In het afhandelscherm kan een KCM de eigenlijke klantvraag invullen. Bijvoorbeeld omdat de `vraag` de eigenlijke vraag niet echt dekt. Of omdat het een nieuwe vraag is, die nog niet in de bron is verwerkt (sticker in je paspoort). Ook deze informatie is in te zien door de KCM bij het bekijken van Contactmomenten. NB: De vraag én de specifieke vraag worden samengevoegd opgeslagen in het Klantcontact, in het property `onderwerp`.| 
| `gespreksresultaat` | string  | Hierin slaan we het gespreksresultaat van het contactmoment op, bijvoorbeeld 'Doorverbonden' of 'Zelfstandig afgehandeld'.  | 
| `startdatum` | string (date-time) | De tijd waar op het Contactmoment is gestart. Samen met `einddatum` bepaaalt dit de gespreksduur. | 
| `einddatum` | string (date-time) | De tijd waarop het Contactmoment is beëindigd. Samen met `startdatum` bepaaalt dit de gespreksduur. | 
| `verantwoordelijkeAfdeling` | string | Dit is de afdeling voor wie het contactmoment is afgehandeld. Deze moet de KCM kiezen in het afhandelscherm. Waar mogelijk wordt deze vooringevuld vanuit de gekozen bron bij `vraag`. | 
| `bronnen` | array | In `bronnen` worden alle geraadpleegde bronnen opgeslagen, die een KCM relevant vindt voor het te registreren Contactmoment. Elk item in deze array heeft 3 onderdelen (alle 3 strings).:
| `bronnen.soort` | string |  Hierin staat het soort bron waar het om ging. Dit is kennisartikel, vac, website, nieuwsbericht of werkinstructie |
| `bronnen.titel` | string | De titel van de bron. In het geval van een VAC is dit de Vraag. |
| `bronnen.url` | string | Dit is de url van de bron.  |
|  |  |  | 


### Vraag en specifieke vraag
Binnen het `contactmoment` was geen aparte plek voor Vraag en Sepcifieke vraag. Dus zijn deze gegevens gescheiden opgenomen in de Contactmomentdetails. Binnen het `klantcontact` is er echter wel een property `onderwerp`. KISS schrijft nu de gegevens van Vraag en Specifieke vraag weg in het `onderwerp`. 

1. Als er alleen een waarde is voor Vraag, staat alleen dit in het `onderwerp`.
2. Als er alleen een waarde is voor Specifieke vraag, omdat er geen bron is aangewezen als Vraag, dan staat alleen de specifieke vraag in het `onderwerp`.
3. Als er zowel een Vraag áls een specifieke vraag is, dan staan beide gegevens in het `onderwerp`: "`vraag` (`specifiekeVraag`)"

