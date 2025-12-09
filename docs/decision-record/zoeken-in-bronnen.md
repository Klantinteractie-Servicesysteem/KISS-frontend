## Zoeken in bronnen 
### Kennisartikelen / productpagina's (PDC) en VAC's
Een KCM wil kunnen zoeken in bronnen om de vraag van een klant te kunnen beantwoorden (zie [#22](https://github.com/Klantinteractie-Servicesysteem/KISS-frontend/issues/22)). Voor Kennisartikelen / productpagina's en VAC's zijn nog geen defintieve standaarden. Binnen KISS Fase 1C hebben we voor deze twee bronnen objectdefinties gemaakt, gebaseerd op standaarden in wording in overleg met architecten van Dimpact.  

- Het object PDC - Kennisartikel is gebaseerd op de API voor [de SDG-invoervoorziening](https://github.com/maykinmedia/sdg-invoervoorziening)  ([versie 1.7.2](https://redocly.github.io/redoc/?url=https://raw.githubusercontent.com/maykinmedia/sdg-invoervoorziening/1.7.2/src/openapi.yaml&nocors)). Daar zijn properties aan toegevoegd t.b.v. de behoeften van een KCM (interne informatie, informatie over contact opnemen, informatie over bijzonderheden, trefwoorden en verantwoordelijke afdeling). 
- Het object VAC is gebaseerd op een vergelijking van PDC - Kennisartikel, met de structuur van VAC's in SDU-catalogus. 

Beide objecten staan bij [de Community concepts Objectttypen](https://github.com/open-objecten/objecttypes/tree/main/community-concepts). 

**Gebruik van HTML**
Voor beide objecttypen verwacht KISS tekstopmaak in HTML-format. KISS ondersteunt de basale tags zoals headers, vet- en schuingedrukte tekst en tabellen. Enkele aandachtspunten: 

1. De HTML wordt gesanitized zodat er geen ongewenst gedrag van bv. script-tags kan optreden. 
2. Inline styling wordt genegeerd. 
3. De waarde van header-tags wordt bij import verkleind, om te zorgen dat de semantische waarden van het geheel in tact blijft. Dit betekent bijvoorbeeld dat een `<h2>` in de bron, binnen KISS als een `<h4>` weergegeven wordt. 
4. Het is mogelijk om met behulp van de tags `<details>` en `<summary>` uitklapbare secties op te nemen 