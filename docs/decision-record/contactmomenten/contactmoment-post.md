# Contactmoment: de uitgebreide POST

_**Onderstaande is alleen nog van toepassing op de koppeling met de e-Suite**_

Voordat KISS aangesloten is op Open Klant 2.x en de Klantinteracties API, schreef KISS de Contactmomenten weg m.b.v. de VNG Contactmomenten API zoals deze is geïmplementeerd in Open Klant v0.5-pre. In een eerdere versie van KISS waren de gegevens die nu in Contactmomentdetails zitten, onderdeel van het uitgebreide Contactmoment. Omdat in KISS 1C de weg is gekozen om bestaande standaarden niet uit te breiden, zijn deze gegevens verplaatst naar Contactmomentdetails.

Bij de overgang naar de VNG Contactmomenten API bleven deze gegevens onderdeel van de POST die KISS doet op deze API. Het ontvangende register negeert deze onderdelen en registreert alleen het object zoals gedefinieerd in de Contactmomenten API.


## Uitbreiding van de POST van een Contactmoment
Bij de integratie met de e-Suite (in gebruik bij Dimpact-gemeenten), in januari-februari 2024, was het uitgangspunt dat KISS zoveel mogelijk onveranderd zou blijven. Dit bleek voor de registratie van een Contactverzoek in de e-Suite helaas niet haalbaar. Daarom is de  POST-data van een Contactmoment vanuit KISS uitgebreid.

### Aanleiding
Binnen KISS bestaat een Contactverzoek uit een Interne Taak met daaraan gekoppeld een Contactmoment. Kiss registreert daarom éérst het Contactmoment in het Contactmomentenregister. Vervolgens neemt KISS de URL van dat contactmoment uit de respons van dat register en voegt dat toe  aan de call waarmee KISS de interne taak registreert.

De e-Suite kent geen apart object voor het Contactverzoek. Een Contactverzoek is een Contact dat niet is afgehandeld, en dat aan een behandelaar (afdeling of groep, en evt. medewerker) is gekoppeld.

KISS geeft echter die informatie van de behandelaar  pas door in de tweede call, nádat het Contactmoment is geregistreerd. Zonder behandelaar zet e-Suite het contact op afgehandeld. En een afgehandeld contact kunt je niet meer bijwerken (patchen). Een mogelijkheid zou zijn om in de adapter, aan de eerste call die het Contactmoment registreert, een dummy behandelaar toe te voegen en vervolgens die behandelaar bij te werken vanuit de call die de Interne Taak registreert. Maar zo’n update van het contact is wel zichtbaar in de audittrail van het contact binnen de e-Suite, en dat is niet wenselijk.

### De aanpassing in KISS
Tijdens dit project van integratie is KISS aangepast zodat alle informatie van de interne taak, die nodig is voor het Contactverzoek-gedeelte in de e-Suite, toe te voegen aan de call die het Contactmoment registreert. Een gekoppeld register dat volgens de standaarden Contactmomenten registreert, zoals bv. Open Klant v0.6, zal de extra informatie uit de call negeren, en alleen Contactmoment registreren.

### Proces in KISS voor de aanpassing
Bij registratie van een Contactmoment doet KISS een POST-call vanuit de frontend. In de KISS BFF worden hier de gegevens van de ingelogde gebruiker aan toegevoegd, en dit gaat naar Open Klant. Tegelijkertijd gaat er een call met gegevens die binnen KISS zelf worden opgeslagen. Dit zijn de zg. contactmomentdetails, gegevens voor management informatie die binnen KISS zelf worden opgeslagen.

Op dit moment zijn de calls naar contactmomenten, en die naar de contactmomentdetails gelijk. En bevat de POST naar het Contactmomentenregister al meer informatie dan er door dit register wordt opgeslagen. Zie ook hieronder bij Gegevens.

![image](https://raw.githubusercontent.com/Klantinteractie-Servicesysteem/.github/refs/heads/main/docs/images/contactmoment/uitbreidingcontactmoment-01.png)


### Proces in KISS na de aanpassing
Na de aanpassing zal de inhoud van de POST op de contactmomenten API uitgebreider zijn. Maar de response uit een standaard Contactmomenten API zal nog steeds hetzelfde zijn. En KISS zal daarna nog steeds, o.b.v. de repsonse de url van dat Contactmoment toevoegen aan de Interne taak.

![image](https://raw.githubusercontent.com/Klantinteractie-Servicesysteem/.github/refs/heads/main/docs/images/contactmoment/uitbreidingcontactmoment-02.png)



### Overzicht gegevens - voor de aanpassing

![image](https://raw.githubusercontent.com/Klantinteractie-Servicesysteem/.github/refs/heads/main/docs/images/contactmoment/uitbreidingcontactmoment-03.png)

De url uit de response wordt vervolgens gebruikt om de Interne Taak te registeren. Op dit moment gebeurt dit op basis van de objecten API in de Objectenregistratie.

![image](https://raw.githubusercontent.com/Klantinteractie-Servicesysteem/.github/refs/heads/main/docs/images/contactmoment/uitbreidingcontactmoment-04.png)

![image](https://raw.githubusercontent.com/Klantinteractie-Servicesysteem/.github/refs/heads/main/docs/images/contactmoment/uitbreidingcontactmoment-05.png)


### Overzicht gegevens - na de aanpassing
![image](https://raw.githubusercontent.com/Klantinteractie-Servicesysteem/.github/refs/heads/main/docs/images/contactmoment/uitbreidingcontactmoment-06.png)

![image](https://raw.githubusercontent.com/Klantinteractie-Servicesysteem/.github/refs/heads/main/docs/images/contactmoment/uitbreidingcontactmoment-07.png)

En de url uit de response wordt vervolgens wederom gebruikt om de Interne Taak te registeren. Op dit moment gebeurt dit op basis van de objecten API in de Objectenregistratie.

![image](https://raw.githubusercontent.com/Klantinteractie-Servicesysteem/.github/refs/heads/main/docs/images/contactmoment/uitbreidingcontactmoment-04.png)

![image](https://raw.githubusercontent.com/Klantinteractie-Servicesysteem/.github/refs/heads/main/docs/images/contactmoment/uitbreidingcontactmoment-05.png)





