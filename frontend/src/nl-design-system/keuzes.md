# Overzicht nieuwsberichten
* Geen [List](https://nl-design-system.github.io/denhaag/?path=/docs/react-data-display-list--default-story):
    * Meer dan drie regels tekst, advies is om Card te gebruiken
* Geen [Card](https://nl-design-system.github.io/denhaag/?path=/docs/react-cards-card--default):
    * Implementatie is gekoppeld aan Material UI
    * Niet genoeg vrijheidsgraden voor semantiek: `<article>` en header is een `<p>` ipv een <h...>
* Paragraph: Alleen classes overgenomen, want maakt geen `<p>` maar een `<div>`

# contactmoment registratieformulier
* issue: we maken gebruik van de utrecht-button webcomponent. de shadowdom is niet rechstreeks te stylen en het component biedt geen hook om de cursor aan te passen (alleen door er een submit van te maken, maar dat wil je niet altijd). daardoor kan er niet de voor buttons gebruikelijke pointer cursor ingesteld worden.
* de .utrecht-select overruled het standaard uitklap pijltje van een html select element. het standaarduiterlijk van een select verschilt nogal per OS. Gaat dit op elk OS goed? 
* Pagination: Utrecht biedt geen ruimte voor client-side navigation. Den Haag CSS variant genomen


# Bevindingen
* In de typescript definitie van de vue wrappers utrecht, wordt `modelValue` als parameter vereist, Zelfs bij bijvoorbeeld headings. Dit is niet de bedoeling en zorgt voor errors in volar.
* Je moet los van de vue wrappers, de web componenten definieren. Hier is alleen een algemene functie voor, die alle componenten registreert. Dit levert een flinke bundel op. Beter zou zijn om per vue component eenmalig de bijbehorende web component te registreren.

# contactmoment zaak/persoon zoeken
* geen utrecht tab component. wel tab react component van denhaag, maar dat is een wrapper op een  @material-ui/core tab. een designsytem in een designsystem? dat levert classes op die ik niet in het uiteindelijke nldesign system verwacht, dus dit gaan we nu niet gebruiken/nabouwen

# table styles
nldesign table styles worden niet overgenomen. bv --utrecht-table-row-border-block-end-color zou toegepast moeten worden als er een utrecht-html class op de container div staat, maar dat werkt niet. zie https://nl-design-system.github.io/utrecht/storybook/?path=/docs/html-table--default-story
waarschijnlijk zijn er nog aanvullende packages of extra body classes nodig oid... verder uitzoeken kost teveel tijd