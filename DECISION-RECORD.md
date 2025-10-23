## Meerdere zaaksystemen

**Soms weten we niet in welk zaaksysteem een bepaald gegeven te vinden is en bevragen we alle zaaksystemen tegelijk. wat te doen als zaaksysteem A een error geeft en zaaksysteem B een succes response?**
We kiezen ervoor de error te negeren en de response van B te tonen.
Helemaal niets tonen als een van de systemen een error retourneert lijkt ons in dit geval storender dan de mogelijke verwarring die kan ontstaan door dat het tonen van potentieel incomplete data.
Wel loggen we de error.

**Soms weten we wel al in welk zaaksysteem een bepaald gegeven te vinden is. Bij de communicatie tussen front-end en backend volgen we zoveelmogelijk de standaard api's. Hoe communiceren we extra informatie over het bronsysteem zonder van de api's af te wijken?**
We kiezen ervoor een header mee te sturen.
Het alternatief zou zijn, om deze informatie helemaal te negeren en altijd elk gegeven in alle beschikbare zaaksystemen op te zoeken. De belangrijkste overweging om dit niet te doen is, naast het grote aantal redundante calls, de complexiteit die ontstaat in het correct afhandelen van fouten en niet gevonden gegevens.

**Het is mogelijk om KISS met één klantcontact register en meerdere zaaksystemen te koppelen. Als een contactmoment gekoppeld is aan een zaak, dan zou KISS adhv de gegevens in het onderwerpobject moeten kunnen bepalen in welk zaaksysteem de zaak zich bevindt. Er is echter geen standaard/afspraak/richtlijn voor de waardes die daartoe gebruikt worden in het coderegister veld om het zaaksysteem te identificeren.**
Op dit moment wordt altijd 'openzaak' ingevuld. Als er meerder zaaksystemn zijn gekoppeld aan KISS, dan proberen we de zaak in alle systemen te vinden.
Andere componenten gebruiken dezelfde gegevens, pas als er met alle relevante partijen een conventie is afgesproken kan in KISS worden ingebouwd dat er een configurabele identificeren waardes gebruikt kunnen worden om onderwerpobjecten te laten verwijzen naar specifieke zaaksystemen. 