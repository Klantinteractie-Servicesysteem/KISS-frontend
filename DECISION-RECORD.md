## Meerdere zaaksystemen

**Soms weten we niet in welk zaaksysteem een bepaald gegeven te vinden is en bevragen we alle zaaksystemen tegelijk. wat te doen als zaaksysteem A een error geeft en zaaksysteem B een succes response?**
We kiezen ervoor de error te negeren en de response van B te tonen.
Helemaal niets tonen als een van de systemen een error retourneert lijkt ons in dit geval storender dan de mogelijke verwarring die kan ontstaan door dat het tonen van potentieel incomplete data.
Wel loggen we de error.

**Soms weten we wel al in welk zaaksysteem een bepaald gegeven te vinden is. Bij de communicatie tussen front-end en backend volgen we zoveelmogelijk de standaard api's. Hoe communiceren we extra informatie over het bronsysteem zonder van de api's af te wijken?**
We kiezen ervoor een header mee te sturen.
Het alternatief zou zijn, om deze informatie helemaal te negeren en altijd elk gegeven in alle beschikbare zaaksystemen op te zoeken. De belangrijkste overweging om dit niet te doen is, naast het grote aantal redundante calls, de complexiteit die ontstaat in het correct afhandelen van fouten en niet gevonden gegevens.

## technisch ontwerp zaken bij bedrijven nieuwe stijl

### Rechtspersoon

```ts
const queries = [
  new URLSearchParams({
    rol__betrokkeneIdentificatie__nietNatuurlijkPersoon__kvkNummer: kvkNummer,
  }),

  new URLSearchParams({
    rol__betrokkeneIdentificatie__nietNatuurlijkPersoon__innNnpId: kvkNummer,
  }),

  new URLSearchParams({
    rol__betrokkeneIdentificatie__nietNatuurlijkPersoon__innNnpId: rsin,
  }),
];
```

```ts
if (
  // ignore if it is a vestiging
  "vestigingsNummer" in betrokkeneIdentificatie &&
  betrokkeneIdentificatie.vestigingsNummer
)
  return false;

if (
  // ignore if it
  // has a kvkNummer AND
  "kvkNummer" in betrokkeneIdentificatie &&
  // the kvkNummer is of another company
  betrokkeneIdentificatie.kvkNummer !== kvkNummer
)
  return false;

if (
  // ignore if it
  // has a innNnpId AND
  "innNnpId" in betrokkeneIdentificatie &&
  // the value doesn't match our kvkNummer AND
  betrokkeneIdentificatie.innNnpId !== kvkNummer &&
  // the value doesn't match our rsin
  betrokkeneIdentificatie.innNnpId !== rsin
)
  return false;

// otherwise it is a match
return true;
```

### Vestiging

```ts
const queries = [
  new URLSearchParams({
    rol__betrokkeneIdentificatie__vestiging__vestigingsNummer: vestigingsnummer,
  }),

  new URLSearchParams({
    rol__betrokkeneIdentificatie__nietNatuurlijkPersoon__vestigingsNummer:
      vestigingsnummer,
    rol__betrokkeneIdentificatie__nietNatuurlijkPersoon__kvkNummer: kvkNummer,
  }),
];
```

```ts
if (
  // ignore if it
  // is not a vestiging OR
  !("vestigingsNummer" in betrokkeneIdentificatie) ||
  // is another vestiging
  betrokkeneIdentificatie.vestigingsNummer !== vestigingsnummer
)
  return false;

if (
  // ignore if it
  // has a kvkNummer in the response AND
  "kvkNummer" in betrokkeneIdentificatie &&
  // the kvkNummer is of a different company
  betrokkeneIdentificatie.kvkNummer !== kvkNummer
)
  return false;

return true;
```
