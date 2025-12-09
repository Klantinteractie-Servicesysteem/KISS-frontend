# Configuratie van Elasticsearch voor KISS
Met behulp van [KISS-Elastic-Sync tool](https://github.com/Klantinteractie-Servicesysteem/KISS-Elastic-Sync) is het mogelijk om websites en een aantal gestructureerde bronnen doorzoekbaar te maken vanuit KISS. De eerste keer dat u deze tool gebruikt, wordt er een meta-engine aangemaakt met de naam `kiss-engine`.

## Crawler
Het doorzoeken van een website binnen KISS wordt mogelijk door de website te crawlen vanuit Elastic Search. Hiervoor gebruikt u de [KISS-Elastic-Sync tool](https://github.com/Klantinteractie-Servicesysteem/KISS-Elastic-Sync). Het is aan te raden om verder overleg te hebben met uw websitebeheerder, over het verdere finetunen van de crawler. Mogelijk zijn er aanpassingen nodig in uw robots.txt, is het raadzaam een KISS-specifieke sitemap.xml op te stellen of zijn er aanvullende filterinstellingen nodig. 

Op het moment dat de crawler de eerste keer gedraaid heeft, wordt het engine schema uitgebreid met de [velden die horen bij de crawler](https://www.elastic.co/guide/en/app-search/current/web-crawler-reference.html#web-crawler-reference-web-crawler-schema).

## Syncen van bronnen
De eerste keer dat er via de synctool Kennisartikelen (PDC-producten), Medewerkers of VACs worden geindexeerd in Elastic, wordt het Engine Schema uitgebreid met een aantal velden. Dit zijn: 
- `object_bron`

- `object_meta`

- De velden die bij de bron horen: voor elk property in het schema van de bron, wordt een property aangemaakt binnen de KISS-engine, voorafgegaan door de waarde van `objectbron`. 

## Relevance Tuning
Om de informatie uit de Kennisartikelen en websites doorzoekbaar te maken, moeten deze velden opgenomen zijn in het Engine Schema, en doorzoekbaar gezet in de Relevance tuning. Om de instellingen voor relevance tuning aan te passen en de beheren, heb je toegang nodig tot de Kibana web interface van Elastic. Zijn er nieuwe velden toegevoegd aan het Engine Schema, bijvoorbeeld omdat er een nieuw type bron is toegevoegd? Dan moeten deze velden doorzoekbaar gemaakt worden, door op knop 'Update search settings' te klikken op de pagina Manage engine schema. U kunt ook op de pagina Relevance Tuning aangeven dat een schemaveld doorzoekbaar moet zijn. 

Om de zoekresultaten te beïnvloeden, moet u vervolgens de Relevance Tuning instellen. [Zie ook de documentatie van Elasticsearch](https://www.elastic.co/guide/en/app-search/current/relevance-tuning-guide.html)

Elasticsearch zal binnen de meta-engine voor elk property van een object (een VAC, een medewerker of een Kennisartikel) een property aanmaken. Bijvoorbeeld `Kennisartikel.vertalingen.bezwaarEnBeroep` of `VAC.trefwoorden.trefwoord`. Elasticsearch zal ook zelf voor deze property's aangeven of ze doorzoekbaar moeten zijn of niet. Websitepagina's worden door de crawler aan de meta-engine toegevoegd, en zijn vindbaar via de velden van [het crawler-schema](https://www.elastic.co/guide/en/app-search/8.9/web-crawler-reference.html#web-crawler-reference-web-crawler-schema).

Door vervolgens de relevantie van specifieke velden hoger te zetten, kunt u ervoor zorgen dat deze zwaarder meetellen in het zoekresultaat. Door de properties `VAC.trefwoorden.trefwoord` en `Kennisartikel.vertalingen.trefwoorden.trefwoord` hogere relevantiescore te geven, zullen de trefwoorden zwaarder meetellen.  

Doordat elke bron eigen properties krijgt in de meta-engine, kunt u specifieke bronnen zwaarder laten wegen. Als `VAC.trefwoorden.trefwoord` een hogere relevantiescore heeft dan `Kennisartikel.vertalingen.trefwoorden.trefwoord`, zullen de VAC's hoger in het zoekresultaat staan.

Alle property's van het type text worden standaard doorzoekbaar gemaakt. Het is raadzaam deze standaard instellingen na te lopen. Vooral bij Kennisartikelen (waarvan het object gebaseerd is op de API van de SDG invoervoorziening) zijn er veel text property's, die niet relevant zijn voor de zoekresultaten.
