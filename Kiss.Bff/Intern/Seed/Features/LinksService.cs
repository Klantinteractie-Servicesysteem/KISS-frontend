using Kiss.Bff.Beheer.Links.Data.Entities;

namespace Kiss.Bff.Intern.Seed.Features
{
    public class LinksService
    {
        public List<Link> GenerateLinks()
        {
            return new List<Link>
            {
                new()
                {
                    DateCreated = DateTimeOffset.UtcNow,
                    Titel = "De repository van KISS, op Github",
                    Url = "https://github.com/Klantinteractie-Servicesysteem/",
                    Categorie = "KISS"
                },
                new()
                {
                    DateCreated = DateTimeOffset.UtcNow,
                    Titel = "Beheerhandleiding KISS",
                    Url = "https://kiss-klantinteractie-servicesysteem.readthedocs.io/en/stable/manual/manual.html",
                    Categorie = "KISS"
                },
                new()
                {
                    DateCreated = DateTimeOffset.UtcNow,
                    Titel = "Documentatie KISS",
                    Url = "https://kiss-klantinteractie-servicesysteem.readthedocs.io/",
                    Categorie = "KISS"
                },
                new()
                {
                    DateCreated = DateTimeOffset.UtcNow,
                    Titel = "Common Ground op Pleio",
                    Url = "https://commonground.nl/",
                    Categorie = "Common Ground"
                },
                new()
                {
                    DateCreated = DateTimeOffset.UtcNow,
                    Titel = "Programma Common Ground",
                    Url = "https://vng.nl/projecten/programma-common-ground",
                    Categorie = "Common Ground"
                }
            };
        }
    }
}
