namespace Kiss.Bff.Intern.Registry.Data
{
    public class RegistryModel
    {
        public RegistryModel() { }

        public bool IsDefault { get; set; }
        public string? RegistryVersion { get; set; }
        public string? KlantinteractieRegistry { get; set; }
        public string? ZaaksysteemRegistry { get; set; }
        public string? KlantRegistry { get; set; }
        public string? InterneTaakRegistry { get; set; }
        public string? ContactmomentRegistry { get; set; }
    }
}
