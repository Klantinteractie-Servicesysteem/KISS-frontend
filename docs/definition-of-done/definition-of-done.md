# Definition of Done

De aanpassingen staan op een devevelopmentomgeving [https://dev.kiss-demo.nl/](https://dev.kiss-demo.nl/) en productieomgeving [https://prod.kiss-demo.nl/](https://prod.kiss-demo.nl/) en voldoen aan alle acceptatiecriteria.

### De applicatie voldoet aan de ontwerpprincipes van Gebruiker Centraal
De eindgebruiker is gedurende het hele project betrokken bij ontwerp en specificatie. De applicatie wordt regelmatig getest door de gebruikers.

### De applicatie voldoet aan de AVG
Er is een DPIA opgesteld voor de applicatie. Deze kunt u [hier inzien](https://github.com/Klantinteractie-Servicesysteem/.github/blob/main/docs/files/ICATT-KISS-Data-Processing-Impact-Assessment-(DPIA).pdf).

### Code is gereviewd door een andere developer
Er wordt ontwikkeld in feature branches en gedeployed vanaf de main branch. Zodoende is inzichtelijk wie aan een story gewerkt heeft en wie een pull request approved heeft.

### Automatische tests
Alle Automatische tests zijn uitgevoerd en geslaagd (zichtbaar als badges op de README)

- Snyk (vulnerability en OWASP scanning)
- Dependabot (security issues in packages)
- Super-linter (code volgens afspraak geformatteerd)
- Unit tests (in ontwikkeling)

### Unit tests
Nieuwe unit tests worden toegevoegd:

- voor code die, wanneer niet correct geïmplementeerd, kan leiden tot kritieke fouten die moeilijk te detecteren zijn
- voor code die moeilijk te begrijpen is zonder uitvoerbare voorbeelden
- voor code die breed hergebruikt wordt binnen de applicatie

### Handmatige tests
- Alle handmatige tests zijn aantoonbaar uitgevoerd op basis van een testscipt per userstory
- Bij oplevering wordt een integratietest doorlopen op basis van een testscript
- WCAG wordt handmatig gecontroleerd m.b.v. lighthouse. De resultaten van deze test vindt u [hier](https://github.com/Klantinteractie-Servicesysteem/.github/blob/main/docs/files/WCAG-Lighthouse-Report-20231010.pdf)

### Pentest
Pentests (frontend en backend) worden wekelijks uitgevoerd. Een voorbeeld van een rapport uit juli 2024 staat [hier](https://github.com/Klantinteractie-Servicesysteem/.github/blob/main/docs/files/WebsiteScanner_30077801_771804-20240711-2302_0.pdf).

### Error logging
Error logging is ingeregeld

### Documentatie
Documentatie is compleet en actueel:

- Documentatie voor ontwikkelaars staat in de code
- Voor overige documentatie zie [Readthedocs](https://kiss-klantinteractie-servicesysteem.readthedocs.io/)

### CI/CD
- Er is een werkende CI/CD straat
- De applicatie kan draaien in een Haven Compliant Cluster, conform de checks zoals beschreven op [https://haven.commonground.nl/techniek/compliancy-checker](https://haven.commonground.nl/techniek/compliancy-checker)

