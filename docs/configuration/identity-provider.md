# Configuratie van uw Identity Provider

Bij de installatie van KISS regelt u de koppeling met uw OpenIDConnect Identity Provider. Daarnaast moet u in uw Identity Provider configureren dat gebruikers die in moeten kunnen loggen bij KISS in ieder geval een 'klantcontactmedewerker'-rol hebben. Een rol is in dit geval een claim van het type `role` of `roles` (beiden worden ondersteund). De waarde die correspondeert met een kiss-medewerker kunt u instellen tijdens de installatie. Standaard is dit `Klantcontactmedewerker`. Voor medewerkers die beheertaken op KISS uitvoeren, zijn aparte rollen ingeregeld. Ook de naam van deze rol kunt u instellen tijdens de installatie. Standaard is dit `Redacteur` of `Beheerder`.

**Let op:** Als een ingelogde gebruiker naar KISS gaat, maar niet de juiste rechten heeft, dan verschijnt er een melding op in beeld: "Je hebt niet de juist rechten voor het gebruik van deze applicatie. Neem contact op met Functioneel Beheer." De melding is zo algemeen geformuleerd, omdat KISS bij verschillende organisaties kan draaien.

Als een ingelogde gebruiker alleen maar de rol Redacteur heeft, dan ziet deze niet de knop Nieuw contactoment, en ook geen bronnenzoekbalk. De nieuwsberichten en werkinstructies zijn wel zichtbaar, omdat de rol Redacteur juist bestaat om die berichten te kunnen berwerken.

## Claims uit uw Identity provider

KISS gebruikt de claims uit uw Identity Provider om de gegevens van de ingelogde Klantcontactmedewerker toe te voegen aan de Contactmomenten (`klantcontacten`) en Contactverzoeken die vanuit KISS worden geregistreerd.
Zie [de installatiehandleiding](../installation/voorbereidingen.md#Authenticatie) voor hoe u dit configureert.

Er is in JWT geen standaard claim voor voorletters of voorvoegsel (tussenvoegsel). KISS gebruikt daarom deze mapping:

- Voorletters => given_name
- Achternaam => family_name indien beschikbaar, anders: name indien beschikbaar, anders: http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name
- Op dit moment doen we niets met Tussenvoegsel, omdat er geen voorvoegsel-claim bestaat. In de huidige implementatie komt tussenvoegsel alleen in het `klantcontact`, als onderdeel van de Achternaam.

## Voorbeeldinrichting in Azure Active Directory

Als u gebruik maakt van Azure Active Directory als Identity Provider, kunt u dit op de volgende manier inrichten.

1. Bij de installatie van KISS heeft u een App Registration aangemaakt. Ga binnen Azure AD naar App registrations en klik op de applicatie.

   ![image](https://raw.githubusercontent.com/Klantinteractie-Servicesysteem/.github/main/docs/images/AzureAD-01.png)

1. Navigeer naar App roles, kies Create app role en vul de benodigde velden in. Belangrijk is dat u kiest voor Users/Groups bij Allowed member types en bij Value kiest voor de rol die u bij de installatie [geconfigureerd](../installation/voorbereidingen.md#Autorisatie) hebt.

   ![image](https://raw.githubusercontent.com/Klantinteractie-Servicesysteem/.github/main/docs/images/AzureAD-02.png)

1. Herhaal de vorige stap voor het instellen van de andere rollen die u tijdens de installatie geconfigureerd heeft.
1. Nu kunt u deze rollen toekennen aan gebruikers of groepen uit uw organisatie. Hiervoor moet u eerst terug naar Azure Active Directory. Navigeer daar naar Enterprise applications.

   ![image](https://raw.githubusercontent.com/Klantinteractie-Servicesysteem/.github/main/docs/images/AzureAD-03.png)

1. In dit scherm vindt u een applicatie met dezelfde naam als de App registration die u eerder heeft aangemaakt. Klik op de applicatie.

   ![image](https://raw.githubusercontent.com/Klantinteractie-Servicesysteem/.github/main/docs/images/AzureAD-04.png)

1. Navigeer naar Users and groups, en klik op Add user/group.

   ![image](https://raw.githubusercontent.com/Klantinteractie-Servicesysteem/.github/main/docs/images/AzureAD-05.png)

1. In dit scherm kunt u de rollen die u eerder gedefinieerd heeft, toekennen aan individuele gebruikers en - als uw licentie van Active Directory dit toestaat - groepen.
