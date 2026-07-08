using Kiss.Bff.NieuwsEnWerkinstructies.Data.Entities;

namespace Kiss.Bff.Intern.Seed.Features
{
    public class BerichtenService
    {
        public List<Bericht> GenerateBerichten()
        {
            return new List<Bericht>
            {
                new()
                {
                    DateCreated = DateTimeOffset.UtcNow,
                    PublicatieDatum = DateTimeOffset.UtcNow,
                    PublicatieEinddatum = DateTimeOffset.UtcNow.AddYears(1),
                    Titel = "KennisbronnenKaart bijgewerkt",
                    Inhoud = "<p>Welke vraag stel je waar? Er is een nieuwe versie van de KennisbronnenKaart beschikbaar. Deze kun je inzien op ons intranet. Raadpleeg deze kaart om snel een beeld te krijgen van welke informatie waar te vinden is.</p>",
                    IsBelangrijk = false,
                    Type = "Nieuws"
                },
                new()
                {
                    DateCreated = DateTimeOffset.UtcNow,
                    PublicatieDatum = DateTimeOffset.UtcNow,
                    PublicatieEinddatum = DateTimeOffset.UtcNow.AddYears(1),
                    Titel = "Aankomende staking van de vuilnisophaaldienst",
                    Inhoud = "<p>We verwachten veel telefoontjes over de staking van de vuilnisophaal dienst. Daarom is er op Intranet een <a href=\"https://www.speciaalportaal.nl\">speciaal portaal</a> ingericht waarop je alles kan vinden dat je nodig hebt.&nbsp;</p>",
                    IsBelangrijk = false,
                    Type = "Nieuws"
                },
                new()
                {
                    DateCreated = DateTimeOffset.UtcNow,
                    PublicatieDatum = DateTimeOffset.UtcNow,
                    PublicatieEinddatum = DateTimeOffset.UtcNow.AddYears(1),
                    Titel = "Mantelzorgcadeau volgende maand van start",
                    Inhoud = "<p>Volgende maand begint de aanvraag van het Mantelzorgcadeau. Mantelzorgers en zorgontvangers die dit niet online kunnen aanvragen, kunnen bellen naar het KCC. Vul met deze bellers het speciale KCC-formulier in, om het cadeau aan te vragen.</p>",
                    IsBelangrijk = false,
                    Type = "Werkinstructie"
                },
                new()
                {
                    DateCreated = DateTimeOffset.UtcNow,
                    PublicatieDatum = DateTimeOffset.UtcNow,
                    PublicatieEinddatum = DateTimeOffset.UtcNow.AddYears(1),
                    Titel = "Wijzig je wachtwoord",
                    Inhoud = "<p>Let op! Het Team ICT-Security heeft vastgesteld dat er enkel zwakke wachtwoorden in gebruik zijn. Iedereen is daarom verplicht uiterlijk 15 juni zijn wacht te wijzigen. Doe je dit niet op tijd, dan wordt je wachtwoord gereset, moet je opnieuw je inloggegevens opvragen.</p>",
                    IsBelangrijk = false,
                    Type = "Werkinstructie"
                },
                new()
                {
                    DateCreated = DateTimeOffset.UtcNow,
                    PublicatieDatum = DateTimeOffset.UtcNow,
                    PublicatieEinddatum = DateTimeOffset.UtcNow.AddYears(1),
                    Titel = "Nieuws en werkinstructies toevoegen",
                    Inhoud = "<p>Op de startpagina van KISS zie je als KCM nieuwsberichten en werkinstructies die belangrijk zijn voor je werk. Hier kunnen berichten staan over incidenten die plaatsvinden in de stad, zodat je weet waar burgers en bedrijven mogelijk extra contact over opzoeken.&nbsp;</p><ul><li>Nieuwsberichten en werkinstructies kunnen alleen door gebruikers toegevoegd worden, die bij Beheer kunnen</li><li>Hoe de berichten zijn georganiseerd lees je in <a href=\"https://kiss-klantinteractie-servicesysteem.readthedocs.io/nl/stable/manual/nieuws.html\">de documentatie van KISS</a>.&nbsp;</li><li>Hier staat ook een handleiding voor het toevoegen en bewerken van berichten.&nbsp;</li></ul>",
                    IsBelangrijk = true,
                    Type = "Werkinstructie"
                },
                new()
                {
                    DateCreated = DateTimeOffset.UtcNow,
                    PublicatieDatum = DateTimeOffset.UtcNow,
                    PublicatieEinddatum = DateTimeOffset.UtcNow.AddYears(1),
                    Titel = "KISS op de testomgeving",
                    Inhoud = "<p>Goed nieuws! Als je dit ziet, is KISS goed geïnstalleerd inclusief de demodata. KISS staat voor Klant Interactie Service Systeem. Het is een applicatie waarmee Klantcontactmedewerkers (KCM) optimaal worden ondersteund in hun werk: het informeren en helpen van burgers en ondernemers die contact opnemen met de gemeente.&nbsp;</p><p>KISS is een Open Source component in het Common Ground-landschap. Je leest meer informatie over KISS in <a href=\"https://github.com/Klantinteractie-Servicesysteem/\">Github</a>, en in de <a href=\"https://kiss-klantinteractie-servicesysteem.readthedocs.io/nl/stable/\">documentatie</a>.</p>",
                    IsBelangrijk = true,
                    Type = "Nieuws"
                }
            };
        }
    }
}
