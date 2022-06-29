## Definition of Done

- De aanpassingen staan op de devomgeving (https://kiss-dev.commonground.nu/) en voldoen aan alle acceptatiecriteria.

- De applicatie voldoet aan de ontwerpprincipes van Gebruiker Centraal.
  De eindgebruiker is betrokken is gedurende het hele project betrokken bij ontwerp en specificatie. De applicatie wordt regelmatig getest door de gebruikers.

- Code is gereviewd zijn door een andere developer.
  Er wordt ontwikkeld in feature branches en gedeployed vanaf de development branch. zodoende is in github inzichtelijk wie aan een story gewerkt heeft en wie een pull request approved heeft.

- Automatische tests
  Alle Automatische tests zijn uitgevoerd en geslaagd (zichtbaar als badges op de readme)

  - Unit Tests (Nieuwe unit tests worden toegevoegd naar inzicht van de developer.)
  - Snyk (vulnerability en OWASP scanning)
  - Docker vulnerablity check
  - Bettercodehub (code quality)
  - Aanwezigheid ongewenste variabelen (tokens, secrets etc) (alleen indien we hier een tool voor kunnen vinden, anders schrappen we dit uit de DoD en wordt het eventueel een audit/review onderdeel)

- Handmatige tests
  Alle handmatige tests zijn uitgevoerd en geslaagd (afgevinkt en bij voorkeur een rapport toegevoegd aan de story)
  
  - Functioneel testen.
  - WCAG. wordt handmatig gecontroleerd mbv een tool.
  - Pentests (frontend en backend) Wordt periodiek uitgevoerd (maandelijks). we maken de afspraak dat resultaten uit een vorige pentest bij de eerstvolgende story opgelost worden. De oplossing wordt echter pas gecontroleerd bij de volgende Pentest.

- Documentatie

  - Documentatie voor contributors: code voorzien in incode documentatie dit wordt bij de code review beoordeeld
  - Documentatie voor eindegebruikers: is geen onderdeel van het project?
  - Documentatie voor implementatiepartners: Changes.md is aangevuld met release notes aan de hand van symantic versioning en per release werken we een implementatie handleiding bij


De DoD is een waarborg voor (een deel van) de architectuurkaders

_Deze Defition of Done is gebaseerd op de inhoud van Discussion #36, op 29-6-2022. _
