# Autorisatie
KISS is ontwikkeld met het oog op gebruik van een aantal api's (Klanten API, Contactmomenten, ZGW API's, Objecten API en Objecttypen API). Deze API's gaan ervan uit dat de autorisatie door de aanroepende applicatie wordt gefaciliteert en niet door het register zelf. Daarom  gebeurt het inrichten van de autorisatie op applicatieniveau en niet op gebruikersniveau. En daarom worden er géén user tokens meegestuurd vanuit KISS naar de onderliggende registers, tenzij dat vereist wordt om specifieke redenen zoals bv. logging. 

Bij de eerste implementatie van KISS binnen een gemeente, moet KISS koppelen met een ouder register, de e-Suite. Deze kan de autorisatie (wat een gebruiker mag zien en doen) doen op gebruikerniveau. Als KISS op de e-Suite koppelt met gebruikmaking van de bestaande 'autorisatie-op-applicatieniveau', dan kan een KISS-gebruiker zaken en contacten zien, die deze gebruiker in de e-Suite niet mag zien. 

Om dat te voorkomen moet KISS dus een fijnmaziger autorisatie op gebruikersniveau mogelijk kunnen maken bij aansluiting op een onderliggend register. Hierdoor is de noodzaak onstaan om wél bij elk request userid mee te kunnen sturen in een JWT-token. 

Hierop is de koppeling met Overige Objecten aangepast: met moet namelijk, afhankelijk van configuratie per objecttype, mogelijk zijn om de calls naar het onderliggende register ófwel te voorzien van een token, ófwel van een token met user-id. 
