## Meerdere zaaksystemen

**Soms weten we niet in welk zaaksysteem een bepaald gegeven te vinden is en bevragen we alle zaaksystemen tegelijk. wat te doen als zaaksysteem A een error geeft en zaaksysteem B een succes response?**
We kiezen ervoor de error te negeren en de response van B te tonen.
Helemaal niets tonen als een van de systemen een error retourneert lijkt ons in dit geval storender dan de mogelijke verwarring die kan ontstaan door dat het tonen van potentieel incomplete data.
Wel loggen we de error.

**Soms weten we wel al in welk zaaksysteem een bepaald gegeven te vinden is. Bij de communicatie tussen front-end en backend volgen we zoveelmogelijk de standaard api's. Hoe communiceren we extra informatie over het bronsysteem zonder van de api's af te wijken?**
We kiezen ervoor een header mee te sturen.
Het alternatief zou zijn, om deze informatie helemaal te negeren en altijd elk gegeven in alle beschikbare zaaksystemen op te zoeken. De belangrijkste overweging om dit niet te doen is, naast het grote aantal redundante calls, de complexiteit die ontstaat in het correct afhandelen van fouten en niet gevonden gegevens.

**Voor het Permission based autorisatie systeem hebben we een RequirePermissionAttribute. Dit kan bij alle controller endpoints gebruikt worden. Voor Yarp proxy routes is dit attribute niet te gebruiken. Hoe lossen we dit op?**
We kiezen ervoor een PermissionAuthorizationPolicyProvider toe te voegen zodat Permissions ook bij de Yarp routes gebruikt kunnen worden.
Het alternatief zou zijn om nu alle Yarp proxy routes om te bouwen naar Controllers. Dit paste niet binnen de scope van de story (PC-1682).
In de toekomst is het goed om de Yarp proxies te vervangen door Controllers zodat de custom PolicyProvider weer verwijderd kan worden.
