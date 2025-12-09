# Contactverzoeken 

## Contactverzoek als interneTaak
Voor Contactverzoeken is nog niet een éénduidige standaard. Sommige gemeenten zien een contactverzoek (terugbelverzoek) als een Zaak. Voor meerdere gemeenten binnen de projectgroep van KISS is dat geen optie. Binnen [de voorzet voor de nieuwe standaard Klantinteracties door de VNG](https://vng-realisatie.github.io/klantinteracties/) is het concept Interne taak geïntroduceerd dat ingezet kan worden voor het Contactverzoek (zie bv. [C9167 - Klant belt KCC met een vraag die niet direct beantwoord kan worden en maakt een terugbelverzoek](https://vng-realisatie.github.io/klantinteracties/achtergronddocumentatie/artefacten/9167.html)). 

In eerdere versies maakte KISS gebruik van een nieuw objecttype voor de interne taak, waarin alle verschillende nieuwe objecten (Betrokkene, Actor) in één object waren gecombineerd. 

KISS is inmiddels aangepast om te koppelen met [Open Klant 2.x](https://github.com/maykinmedia/open-klant), waarin Klantinteracties, en dus de `interneTaak` is geïmplementeerd. Dus bij koppeling met de Klantinteracties API maakt KISS géén gebruik meer van het Objecttype Interne Taak. 

Het Contactverzoek als geheel bestaat uit een Klantcontact dat gekoppeld is aan Interne Taak. De contactgegevens staan in de aan het Klantcontact gekoppelde Betrokkene, en de aan de Betrokkene gekoppelde Digitale Adressen. De afdeling, groep of medewerker die het Contactverzoek moet oppakken worden opgeslagen in de Actoren aan wie de Interne Taak is toegewezen. 

## Afdelingen en groepen
Een Contactverzoek moet binnen de eigen organisatie worden opgepakt. Binnen Klantinteracties is hiervoor het Actor-object beschikbaar, die van het soort `medewerker`, `organisatorische eenheid` of `geautomatiseerde actor` kan zijn (zie [de API Specificatie van Open Klant 2](https://github.com/maykinmedia/open-klant?tab=readme-ov-file#api-specificatie)). KISS maakt gebruik van de eerste twee soorten. Binnen de gebruikersgroep was de behoefte om bij `organisatorische eenheid` onderscheid te kunnen maken tussen Afdeling of Groep. Daarom zijn er ook twee objecttypen hiervoor gemaakt: afdeling en groep. Deze objecttypen staan bij [de Community concepts Objecttypen](https://github.com/open-objecten/objecttypes/tree/main/community-concepts).

Bij de opzet van deze objecten zijn we uitgegaan van een hierarchische relatie tussen Afdeling, Groep en Medewerker. Bij de integratie van KISS met de e-Suite (in gebruikt bij Dimpact-gemeenten), in januari-februari 2024, blijkt dat de hierarchische relatie tussen afdelingen en groepen niet altijd bestaat. Groepen zijn daar een ander soort verzameling van medewerkers. Daarom is er een aanpassing gedaan in het groep-object, om te zorgen dat het `afdelingId` niet verplicht is (zie [PR 13](https://github.com/open-objecten/objecttypes/pull/13)). De KISS frontend doet vanaf v0.5.0 niets meer met die hierarchische relatie.

Op plekken waar we naar afdelingen zoeken (bv. afdeling bij kennisartikelen, of bij medewerker) is de afdeling/groepsnaam gekozen als zoeksleutel. Dit is gedaan op basis van de toen beschikbare (test)data. Daarin was alleen naam altijd aanwezig, identificatie niet altijd. 




## Medewerkers en smoelenboek
Een KCM wil kunnen zoeken in bronnen met informatie, maar ook kunnen zoeken naar informatie over collega’s [#96](https://github.com/Klantinteractie-Servicesysteem/KISS-frontend/issues/96). Ook hier is nog geen standaard voor. In KISS Fase 1B is een voorzet gedaan voor een Medewerker API (deels gebaseerd op  de [HR-JSON-standaarden van HR Open Standards](https://www.hropenstandards.org/)). In KISS Fase 1C is deze verplaatst naar de Objecten API ( zie [de Community concepts](https://github.com/open-objecten/objecttypes/tree/main/community-concepts).) De objecten van het smoelenboek zijn geïndexeerd in Elastic. Bij het doorzetten van een contactverzoek naar een medewerker, gebruiken we de gegevens uit Elastic.

Bij de integratie van KISS met de e-Suite (in gebruikt bij Dimpact-gemeenten), blijkt dat de naam van een medewerker niet altijd beschikbaar is in de onderdelen `voornaam`, `voorvoegselAchternaam` en `achternaam`. Daarom zijn de properties `voornaam` en `achternaam` niet langer verplicht, en is het property `volledigeNaam` toegevoegd. (zie [PR 13](https://github.com/open-objecten/objecttypes/pull/13))
