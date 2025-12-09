# Klantinteracties en Open Klant
Bij het koppelen van KISS aan de Klanteracties-API's zoals die in Open Klant 2 zijn geimplementeerd, hebben we een aantal keuzes gemaakt bij de inrichting van de gegevens, die in Klantinteracties door de VNG zijn beschreven als items op een Referentielijst.

## Klantcontact
De volgende properties uit het Klantcontact vullen we hard in met de volgende waarden:
- `indicatieContactGelukt`: altijd  true
- `taal`: altijd nld
- `vertrouwelijk`: altijd false
- `plaatsgevondenOp`: altijd de datum van registratie van het klantcontact.


## Partij-identificatoren

Sinds de aansluiting op Open Klant 2.7.0 maakt KISS gebruik van partij-identificatoren op de manier zoals dit vanaf deze versie van Open Klant noodzakelijk is. Zie [#944](https://github.com/Klantinteractie-Servicesysteem/KISS-frontend/issues/944)

**Let op:** dit betekent dat KISS versies vóór 1.0.0 niet kunnen koppelen met Open Klant 2.6 of hoger.


### Digitale adressen
Sinds de aansluiting op Open KLant 2.2 gebruikt KISS de soorten digitaal adres zoals de API dit voorschrijft. Zie issue [#891](https://github.com/Klantinteractie-Servicesysteem/KISS-frontend/issues/891). De benodigde validatie die sinds Open Klant 2.4 noodzakelijk is, is middels validatie in de KISS-frontend ondersteund. Zie issue [#939](https://github.com/Klantinteractie-Servicesysteem/KISS-frontend/issues/939).


## Actor-identificator

### Actor klantcontact

De Actor die we opslaan bij het Klantcontact, vullen we op basis van de gegevens van de ingelogde gebruiker, waarbij we er in eerste instantie vanuit gaan dat dit altijd is o.b.v. e-mailadres:

```

"actoridentificator": {
    "objectId": "user@domein.nl",
    "codeObjecttype": "mdw",
    "codeRegister": "msei",
    "codeSoortObjectId": "email"
    }

```

### Actor Contactverzoek (interneTaak)
Van de Actor die we opslaan bij een interne taak hebben we geen gegevens uit EntraID / OIDC, maar alleen uit Objectenregister. Dus voorlopig slaan we bij deze Actoren ook vast in de code de Actor-identificator properties op, die verwijzen naar het Objectenregister. Bij zowel Afdelingen, Groepen als Medewerkers gebruikt KISS als SoortObjectId de waarde van het property `identificatie`.

**Actoridentificator als medewerker**
```
"actoridentificator": {
    "objectId": "hassel006",
    "codeObjecttype": "mdw",
    "codeRegister": "obj",
    "codeSoortObjectId": "idf"
    }
```

**_Actoridentificator medewerker bij handmatig opgevoerd e-mailadres_**

Op verzoek van de gemeente Utrecht is het mogelijk gemaakt om Contactverzoeken aan te maken voor medewerkers, zonder dat er een Smoelenboek is gekoppeld. Hiervoor is een feature-switch aanwezig (zie ook [Installatiehandleiding, onderdeel configuratie, bij Feature switches](../installation/configuratie.md#kiss-frontend-feature-switches))



In dat geval wordt de actoridentificator op een iets andere manier opgebouwd.

```
"actoridentificator": {
	"objectId": "surbhi.jha@info.nl",
	"codeObjecttype": "mdw",
	"codeRegister": "handmatig",
	"codeSoortObjectId": "email"
	}
```

**Actoridentificator als AFDELING**
```
"actoridentificator": {
    "objectId": "PaBH",
    "codeObjecttype": "afd",
    "codeRegister": "obj",
    "codeSoortObjectId": "idf"
    }
```
**Actoridentificator als GROEP**
```
"actoridentificator": {
    "objectId": "VTH_OVG_ZV",
    "codeObjecttype": "grp",
    "codeRegister": "obj",
    "codeSoortObjectId": "idf"
    }
```

## Onderwerp van het Klantcontact: maximale lengte
Binnen KISS kan een KCM zowel een Vraag als een Specifieke vraag opgeven. Een Vraag is altijd de titel van een Kennisartikel, of de Vraag binnen een VAC. Een KCM kan daarbij een specifieke vraag opgeven. Als er géén Kennisartikel of VAC is geraadpleegd, of als er géén is geselecteerd als Vraag, dan is Specifieke vraag verplicht. 

Deze twee gegevens worden bij het wegschrijven van een Contactmoment opgeslagen in het property `klantcontact.onderwerp`. Dit is een property met een maximale lengte van 200 tekens. We hebben een wijzigingsverzoek ingediend bij Open Klant, om te vragen of dit getal kan worden opgehoogd, zie: [Increase field sizes for klantcontact and interne taak · Issue #461](https://github.com/maykinmedia/open-klant/issues/461)].

Tot die tijd geldt, dat de kans dat de gecombineerde lengte van de twee gegevens groter is dan 200, aanzienlijk is. Om te voorkomen dat een KCM het Contactmoment niet kan opslaan vanwege de lengte, zijn er in KISS de volgende restricties toegevoegd: 

- Een KCM kan niet meer dan 180 tekens invoeren in het veld Specifieke vraag. 
- De inhoud van Specifieke vraag wordt altijd in zijn geheel opgenomen in `klantcontact.onderwerp`.
- De inhoud van Vraag wordt afgekapt, zodat de totale lengte niet meer dan 200 teken is. 
- Als er alleen Vraag is, dan wordt Vraag afgekapt tot 197 tekens, en staan er 3 puntjes achter
- Als er een Vraag én een Specifiek vraag is, dan worden er 6 tekens van Vraag afgehaald: 3 puntjes om aan te geven dat het is afgekapt, en daarna een spatie en de inhoud van Vraag tussen twee haakjes. 

De reden dat we Vraag afkappen, is dat deze uiteindelijk altijd gevuld wordt vanuit een Bron. We gaan ervan uit dat men die bron altijd kan opzoeken. 
We willen Specifieke vraag niet afkappen, omdat dit specifieke info is door de KCM ingevuld.  
Vraag wordt altijd wel volledig opgeslagen in de [Contactmomentdetails](../manual/managementinformatie.md) in KISS zelf. 

Als Open Klant wordt aangepast, en de maximale lengte van `klantcontact.onderwerp` groter wordt, dan zullen deze restricties herzien. Omdat er zeer waarschijnlijk altijd een Maximale lengte is, zal het principe waarbij we Vraag afkappen waarschijnlijk wel gehandhaafd blijven. 

## Inhoud van het Klantcontact: maximale lengte
Tijdens het Contactmoment kan een KCM in een kladblok aantekeningen maken. Op het moment dat de KCM naar afronden gaat, wordt de inhoud van dat kladblok ingevuld in veld Notitie. 

Het veld Notitie komt terecht in `klantcontact.inhoud`. Dit veld heeft op dit moment in Open Klant een maximale lengte van 1000 tekens. Om te voorkomen dat er foutmeldingen optreden, op het moment dat het Klantcontact wordt weggeschreven naar Open Klant, zit er een controle op die maximale lengte van 1000 tekens op het veld Notitie in het Afhandelingscherm. 

Omdat we denken dat het vervelend is om die maximale lengte ook te hebben op het Kladblok, hebben we daar géén maximale lengte op gezet. Maar hiermee kan het veld Notitie dus vooringevuld worden met meer dan 1000 tekens.

- In het afhandelingscherm kan een KCM hier nooit meer an 1000 tekens intypen.
- Op het moment dat de inhoud van het veld, vanuit het Kladblok, meer dan 1000 tekens bevat, verschijnt er een validatiemelding. Deze foutmelding geeft aan wat de maximale lengte is, hoeveel tekens er in het veld staan en hoeveel tekens er verwijderd moeten worden voordat het Klantcontact kan worden opgeslagen. Deze foutmelding verdwijnt pas, als de inhoud 1000 tekens is of minder. 

Tegelijkertijd hebben we een verzoek ingediend bij Open Klant of het mogelijk is de maximale lengte van `klantcontact.inhoud` op te hogen, zie: [Increase field sizes for klantcontact and interne taak · Issue #461](https://github.com/maykinmedia/open-klant/issues/461).

## Contactverzoek en toelichting voor collega: maximale lengte
Bij het maken en opstellen van een Contactverzoek kan een KCM interne informatie voor de collega invullen. Hiervoor is een invoerveld in het scherm van Contactverzoek, en ook in het Afhandelscherm. Daarnáást kan een KCM, voor sommige afdelingen, aanvullende vragen beantwoorden, in een zogenaamd Contactverzoekformuliertje (zie ook de [beheerhandleiding](../manual/formulieren.md)). 

Zowel de interne toelichting voor de collega, áls de inhoud van een Contactverzoekformuliertje, voegen we toe aan het Contactverzoek, in één veld: `internetaak.toelichting`. Van een Contactverzoekformuliertje worden zowel de vraagteksten als de bijbehorende antwoorden toegevoegd. 

Het veld `internetaak.toelichting` heeft op dit moment in Open Klant een maximale lengte van 1000 tekens. Het is lastig om hiervoor native HTML-foutmeldingen te realiseren: het kan per sessie heel erg verschillen over hoeveel velden het gaat. En de lengte van het Contactverzoekformuliertje verschilt per geselecteerd formuliertje. Ook is het nog niet heel duidelijk of deze maximale lengte een probleem gaat zijn. 

Daarom hebben we besloten om deze foutmelding een andere weergave te geven in de vorm van een toaster. Een toaster is een melding die na verloop van tijd vanzelf verdwijnt. In dit geval is het 30 seconden. Voor deze specifieke toaster is het ook mogelijk om deze dicht te klikken. In de tekst van de foutmelding, proberen we zo goed mogelijk uit te leggen wat een KCM kan doen, om de fout te verhelpen. De tekst luidt (de in italic geschreven getallen tekens zijn een voorbeeld): "De interne informatie van vraag 1 bevat _1048_ tekens, dit mag maximaal 1000 tekens zijn. Verwijder _48_ tekens uit de Interne toelichting voor de medewerker, of uit de velden van een contactverzoekformulier."
