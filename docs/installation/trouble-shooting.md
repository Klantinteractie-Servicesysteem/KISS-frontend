## Trouble shooting guide
In dit document houden we bij tegen welke problemen mensen zijn aangelopen bij het installeren van KISS in een gemeentelijk K8S-cluster.

### Open Diensten-producten accepteren geen gebufferde berichten

Met Open Diensten-producten, bedoelen we de registers zoals Open Zaak en Open Klant, maar ook de Objecten en Objecttype API's.

#### Symptoom
Post en Put berichten lijken leeg. 
#### Mogelijke oorzaak
KISS stuurt berichten in chunks weg. Dat is standaard gedrag van het framework waarop ontwikkeld wordt en conform de standaarden. In ons eigen dev-cluster zit er een Ngingx-controller tussen KISS en de onderliggende registers. Deze buffert standaard berichten en zorgt er voor dat deze mét een content-length-indicate naar de OpenDiensten-registers gaan. 

Dit blijkt ook nodig: registers als OpenObjecten en OpenKlant, die op het Django framework zijn gebouwd, kunnen de berichten anders niet verwerken. 

Gezien bij: 
- Een gemeente die een anderen ingress-controler gebruikte dan Nginx, liep tegen onduidelijke foutmeldingen aan bij het registreren van Klantcontacten
- Een gemeente die een vergelijkbare situatie had bij het wegschrijven van nieuwe VACs in Objecten.

### Open Klant 2.7 geeft foutmelding bij zoeken op telefoonnummer dat ook van Partij is

#### Symptoom
Op de tab Contactverzoeken verschijnt soms een foutmelding bij het zoeken op een telefoonnummer of e-mailadres. In de netwerktab is een 500-foutmelding te zien

#### Oorzaak
Open Klant geeft een foutmelding, op het moment dat je zoekt naar digitale adressen, en daarbij een `expand` meegeeft op `vertrektDoorBetrokkene`. Zie ook: [digitaleadressen endpoint results in error when expanding on a property that is null in any of the results · Issue #400](https://github.com/maykinmedia/open-klant/issues/400)


In KISS kun je hier tegenaanlopen bij het zoeken naar anonieme contactverzoeken, op het moment dat het telefoonnummer of e-mailadres óók is gekoppeld aan een Partij. Bv. in dit geval:
- 0205213571 is het telefoonnummer dat is verstrekt door een partij
- Maar dit telefoonnummer is ook geregistreerd bij een zg. Anoniem Contactverzoek: een klantcontact dat resulteerde in een interneTaak , maar dat niet gekoppeld is aan een partij.
- De KCM gaat Contactverzoeken zoeken op telefoonnummer 0205213571