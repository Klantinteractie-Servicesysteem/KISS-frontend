using System.ComponentModel.DataAnnotations;

namespace Kiss.Bff.ZaakGerichtWerken.Contactverzoeken
{
    public class ContactVerzoekVragenSet
    {
        [Required]
        public int Id { get; set; } = default!;
        [Required]
        public string Naam { get; set; } = default!;
        public string? JsonVragen { get; set; }
        public string? AfdelingId { get; set; }
    }
}
