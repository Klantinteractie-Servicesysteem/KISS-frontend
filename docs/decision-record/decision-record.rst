Ontwerpbeslissingen
===================
KISS is ontwikkeld met een eindgebruikersgroep van Klantcontactmedewerkers (KCMs) van verschillende gemeenten. Uitgangspunt bij de ontwikkeling van KISS was om de beschikbare standaarden uit de Common Ground te gebruiken. In het toepassen van de standaarden en het omgaan met ontbrekende standaarden zijn diverse Ontwerpbeslissingen genomen. Deze staan hier vermeld.

**Beslisboom ontbrekende standaarden**

Er was echter niet voor alle functionaliteiten al een standaard beschikbaar Daar waar er vanuit de gebruikersgroep een informatiebehoefte was, maar waar nog geen (plek binnen de) standaard voor was, is een ontwerpbeslissing genomen wat hiermee te doen (zie ook `20230616-PodiumD-flow-afwijkingen-standaarden.pdf <https://github.com/Klantinteractie-Servicesysteem/KISS-frontend/blob/main/docs/files/20230616-PodiumD-flow-afwijkingen-standaarden.pdf>`__). Op het moment dat er een gegevensbehoefte is binnen de applicatie, waarvoor nog geen (volledige) gegevensstandaard beschikbaar is, is de eerste vervolgvraag of deze gegevens ook beschikaar moeten zijn voor ándere applicaties binnen het landschap. Als dat niet het geval is, dan kunnen de gegevens binnen de applicatie zelf worden opgeslagen. Moeten de gegevens ook beschikbaar zijn voor andere applicaties, dan kiezen we ervoor om op basis van een gegevensontwerp een Objecttype aan te maken in de Objectenregistratie.  

.. toctree::
   :maxdepth: 1
   

   contactmomenten/contactmomenten
   contactverzoeken.md
   klantinteracties-openklant.md
   zaken.md
   meerdere-registers.md
   zoeken-in-bronnen.md
   logging-voor-verwerking.md
   autorisatie.md

