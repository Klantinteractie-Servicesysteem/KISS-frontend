# Configuratie t.b.v. Objecten API
KISS maakt voor de koppeling met verschillende registraties gebruik van de Objecten API. Om objecten van een bepaald objecttype op te kunnen halen, moet het de URL weten van dat objecttype in de Objecttype API. Zie ook [de Installatiehandleiding](../installation/configuratie.md) en de [documentatie bij KISS-Elastic-Sync](https://github.com/Klantinteractie-Servicesysteem/KISS-Elastic-Sync/blob/main/README.md) 

## Authenticatie bij de Objecten API
In de meeste gevallen identificeert KISS zich bij de Objectenregistratie m.b.v. een TOKEN.
In sommige gevallen is het nodig om de authenticatie in de Objecten API, voor Afdelingen, Groepen en Interne Taken, Medewerkers in te regelen m.b.v. een client secret en een client id. Dit is bv. het geval als je KISS gebruikt i.c.m. de e-Suite. Afhankelijk van de situatie moet je dus een Token inregelen, en in andere gevallen een id+secret. **NOOIT ALLEBEI!**

Dit heeft invloed op de volgende Environment Variabelen: 

- Bij gebruik in combinatie met OverigeObjecten met een in OverigeObjecten ingesteld token<br/> 
MEDEWERKER_OBJECTEN_TOKEN

- Bij gebruik in combinatie met de e-suite, met een in de e-suite ingesteld secret en id
MEDEWERKER_OBJECTEN_CLIENT_SECRET <br/> MEDEWERKER_OBJECTEN_CLIENT_ID 

**_Onderstaande variabelen zijn alleen nog relevant bij een koppeling met de e-Suite_**
- Bij gebruik in combinatie met OverigeObjecten met een in OverigeObjecten ingesteld token<br/> 
REGISTERS__N__INTERNE_TAAK_TOKEN

- Bij gebruik in combinatie met de e-suite, met een in de e-suite ingesteld secret en id<br/> 
REGISTERS__N__INTERNE_TAAK_CLIENT_SECRET<br/> 
REGISTERS__N__INTERNE_TAAK_CLIENT_ID

## URL configureerbaar
Voor een aantal objecttypen kunt u de URL van het objecttype instellen m.b.v. environment variabelen. 

| Variabele |  Toelichting |
|---|---|
| GROEPEN_OBJECT_TYPE_URL | Objecttype dat gebruikt wordt om Groepen op te halen <br/> voor gebruik in Contactverzoeken |
| AFDELINGEN_OBJECT_TYPE_URL | Objecttype dat gebruikt wordt om Afdelingen op te halen <br/>voor gebruik in Contactverzoeken, Formulieren <br />contactverzoek en registratie van Contactmomenten  |
| SDG_OBJECT_TYPE_URL | Objecttype dat gebruikt wordt om Kennisartikelen op te halen; <br/>dit objecttype is gebaseerd op het object `product` <br />in de SDG Invoervoorziening |
|  | **_Onderstaande is alleen nog relevant voor een KISS installatie die koppelt met de e-Suite_** |
| REGISTERS__N__INTERNE_TAAK_OBJECT_TYPE_URL |  De interne taak is onderdeel van een Contactverzoek |
| REGISTERS__N__INTERNE_TAAK_TYPE_VERSION | De versie van het objecttype die gebruikt wordt in de API-aanroepen. Standaard ingesteld op `1`. |


## URL nog niet configureerbaar
Voor een aantal objecttypes is de URL nog niet configureerbaar. In die gevallen zal KISS zelf de Objecttype API gaan bevragen om de URL op te halen. Hiervoor is het van belang dat de objecttypen in uw registratie de juiste naam hebben.

| Naam Objecttype |  Toelichting |
|---|---|
| VAC | Dit zijn de Vraag Antwoord Combinaties die via Elasticsearch ontsloten worden. |
| Medewerker | Dit zijn de medewerkers die via Elasticsearch ontsloten worden. <br />Deze medewerkers worden ook gebruikt in Contactverzoeken |
