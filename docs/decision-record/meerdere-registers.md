# Meerdere registers

Een gemeente kan KISS koppelen aan meerdere Zaakregisters. Omdat in sommige gevallen er een specifiek Contactmomenten- en Klantenregister is voor een Zaaksysteem, is het nodig om per gekoppeld Zaakregister aan te geven wat het bijbehorende Contactmomenten- en Klantenregister is. Dit heeft gevolgen voor de opbouw van de Environment Variabelen. Zie ook [de installatiehandleiding](../installation/installatie.md) .

## Uitgangspunten: meerdere gekoppelde registers
- Er is één Default-register: dit is het register waarin alle Contactmomenten en Contactverzoeken, geregistreerd worden, zolang deze  NIET gekoppeld zijn aan een Zaak. Klanten worden opgezocht in het default register.
- Als er een Contactmoment bij een Zaak geregistreerd wordt, dan wordt deze geregistreerd in het bij het Zaakregister behorende Contactmomenten- en Klantenregister.
- Als het nodig is dat er ook een Klant gekoppeld is aan het Contactmoment, dan zal ook deze gezocht en eventueel aangemaakt worden, in dat bij het Zaakregister behorende Contactmomenten- en Klantenregister.

## Type Contactmomenten- en Klantenregister: ondersteuning van de e-Suite.
KISS koppelt alleen met Contactmomenten- en  Klantenregisters op basis van de Klantinteracties API. KISS wil echter de komende twee jaar óók nog de koppeling met de e-Suite ondersteunen. Binnen de Dimpactgemeenten heet wordt deze situatie omschreven als Oud Naast Nieuw. De e-Suite kent géén Klantinteracties API, maar wel een eigen Contactmomenten API en een Klanten API gebaseerd op de VNG Contactmomenten API beschikbaar zoals deze was geïmplementeerd in Open Klant v0.5.0-pre, en Open Klant 1.0.

Om dit te kunnen ondersteunen in een koppeling met meerdere registers, is het dus óók van belang om per 'setje' registers, aan te geven welke standaard het Contactmomenten- en Klantenregister hanteert: `OpenKlant2`, of zoals in het geval van de e-Suite: `OpenKlant1`

