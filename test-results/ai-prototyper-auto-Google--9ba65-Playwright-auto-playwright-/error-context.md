# Page snapshot

```yaml
- dialog "Voordat je verdergaat naar Google Zoeken":
  - img "Google"
  - 'button "Taal: ‪Nederlands‬"':
    - img
    - text: nl
  - link "Inloggen"
  - heading "Voordat je verdergaat naar Google" [level=1]
  - text: We gebruiken
  - link "cookies":
    - /url: https://policies.google.com/technologies/cookies?utm_source=ucbs&hl=nl
  - text: "en gegevens voor het volgende:"
  - list:
    - listitem: Google-services leveren en onderhouden
    - listitem: Uitval bijhouden en bescherming bieden tegen spam, fraude en misbruik
    - listitem: Doelgroepbetrokkenheid en sitestatistieken meten om inzicht te krijgen in hoe onze services worden gebruikt en de kwaliteit van die services te verbeteren
  - text: "Als je Alles accepteren kiest, gebruiken we cookies en gegevens ook voor het volgende:"
  - list:
    - listitem: Nieuwe services ontwikkelen en verbeteren
    - listitem: Advertenties laten zien en de effectiviteit ervan meten
    - listitem: Gepersonaliseerde content laten zien (afhankelijk van je instellingen)
    - listitem: Gepersonaliseerde advertenties laten zien (afhankelijk van je instellingen)
  - text: Als je Alles afwijzen kiest, gebruiken we cookies niet voor deze aanvullende doeleinden. Niet-gepersonaliseerde content wordt beïnvloed door factoren zoals de content die je op dat moment bekijkt, activiteit in je actieve zoeksessie en je locatie. Niet-gepersonaliseerde advertenties worden beïnvloed door de content die je op dat moment bekijkt en je algemene locatie. Gepersonaliseerde content en advertenties kunnen ook relevantere resultaten, aanbevelingen en op jou toegespitste advertenties omvatten die zijn gebaseerd op eerdere activiteit van deze browser, zoals uitgevoerde Google-zoekopdrachten. We gebruiken cookies en gegevens ook om te zorgen dat de functionaliteit geschikt is voor je leeftijd, als dit relevant is. Selecteer Meer opties om meer informatie te bekijken, waaronder over hoe je je privacyinstellingen beheert. Je kunt ook altijd naar g.co/privacytools gaan.
  - button "Alles afwijzen"
  - button "Alles accepteren"
  - link "Meer opties voor personalisatie-instellingen en cookies": Meer opties
  - link "Privacy":
    - /url: https://policies.google.com/privacy?hl=nl&fg=1&utm_source=ucbs
  - link "Voorwaarden":
    - /url: https://policies.google.com/terms?hl=nl&fg=1&utm_source=ucbs
```