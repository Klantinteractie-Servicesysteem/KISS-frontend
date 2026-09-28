using System.ComponentModel.DataAnnotations;

namespace Kiss.Bff.Intern.ContactmomentDetails.Data.Entities
{
    public class ContactmomentDetails
    {
        public string Id { get; set; } = default!;
        public DateTimeOffset Startdatum { get; set; }
        public DateTimeOffset Einddatum { get; set; }
        public string? Gespreksresultaat { get; set; }
        public string? Vraag { get; set; }
        public string? SpecifiekeVraag { get; set; }
        public string? EmailadresKcm { get; set; }
        public string? VerantwoordelijkeAfdeling { get; set; }
        public string? GespreksId { get; set; }

        public List<ContactmomentDetailsBron> Bronnen { get; set; } = new();
    }
}
