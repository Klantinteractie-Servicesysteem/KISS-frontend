# Architectuur

## KISS i.c.m. OpenKlant2.x

<a href="https://raw.githubusercontent.com/Klantinteractie-Servicesysteem/.github/refs/heads/main/docs/architectuur/KISS%20Architectuur%20schets-Open%20Zaak%20en%20Open%20Klant.drawio.png" target="_blank"><img src="https://raw.githubusercontent.com/Klantinteractie-Servicesysteem/.github/refs/heads/main/docs/architectuur/KISS%20Architectuur%20schets-Open%20Zaak%20en%20Open%20Klant.drawio.png" /></a>

_KISS i.c.m. Open Klant 2.x - Klik op de afbeelding om een grotere versie te zien._

## KISS i.c.m. de e-Suite


<a href="https://raw.githubusercontent.com/Klantinteractie-Servicesysteem/.github/refs/heads/main/docs/architectuur/KISS%20Architectuur%20schets-e-Suite.drawio.png" target="_blank"><img src="https://raw.githubusercontent.com/Klantinteractie-Servicesysteem/.github/refs/heads/main/docs/architectuur/KISS%20Architectuur%20schets-e-Suite.drawio.png" /></a>

_KISS i.c.m. e-Suite - Klik op de afbeelding om een grotere versie te zien._

## Meerdere registers 
Voor de Dimpact gemeente is het mogelijk gemaakt om KISS te koppelen met de e-Suite. Om in de overgangssituatie beide systemen te kunnen ondersteunen, is het mogelijk gemaakt om KISS te koppelen met meerdere registers: zowel met Open Zaak naast Open Klant, als met de e-Suite. Zie ook [Meerdere registers](../decision-record/meerdere-registers.md) in Ontwerpbeslissingen. 

In dat geval is de architectuurplaat een combinatie van bovenstaande afbeeldingen. 

## KISS i.c.m. PABC (Platform Autorisatie Beheer Component)

KISS kan optioneel worden gekoppeld met [PABC](https://github.com/Platform-Autorisatie-Beheer-Component/PABC-API) voor fijnmazige autorisatie op zaaktype-niveau. Wanneer geconfigureerd, vraagt KISS aan PABC welke zaaktypes een gebruiker mag inzien op basis van de functionele rollen uit de Identity Provider. Zie [PABC Koppeling](../installation/pabc.md) voor configuratie en inrichting.