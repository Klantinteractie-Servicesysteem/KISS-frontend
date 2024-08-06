## Meerdere zaaksystemen

**Soms weten we niet in welk zaaksysteem een bepaald gegeven te vinden is en bevragen we alle zaaksystemen tegelijk. wat te doen als zaaksysteem A een error geeft en zaaksysteem B een succes response?**

We kiezen ervoor de error te negeren en de response van B te tonen.
Helemaal niets tonen als een van de systemen een error retourneert lijkt ons in dit geval storender dan de mogelijke verwarring die kan ontstaan door dat het tonen van potentieel incomplete data.
Wel loggen we de error.

**Soms weten we wel al in welk zaaksysteem een bepaald gegeven te vinden is. Bij de communicatie tussen front-end en backend volgen we zoveelmogelijk de standaard api's. Hoe communiceren we extra informatie over het bronsysteem zonder van de api's af te wijken?**

We kiezen ervoor een header mee te sturen.
Het alternatief zou zijn, om deze informatie helemaal te negeren en altijd elk gegeven in alle beschikbare zaaksystemen op te zoeken. De belangrijkste overweging om dit niet te doen is, naast het grote aantal redundante calls, de complexiteit die ontstaat in het correct afhandelen van fouten en niet gevonden gegevens.

## Genereren client code op basis van OpenAPI specs
**Soms kloppen de OpenAPI specs niet met de betreffende API, of zorgen deze voor issues in de generator voor client code.**

We kiezen ervoor om de specs handmatig aan te passen om deze problemen op te lossen.
De verwachting is dat dit minder werk is dan het zelf schrijven en onderhouden van de client code.
Om inzichtelijk te maken wat we precies wijzigen aan de specs, doen we dit in een enkele commit.
