namespace Kiss.Bff.Intern.Registry.Data
{
    public class KissConnectionsModel
    {
        public KissConnectionsModel() { }

        public string? AfdelingenBaseUrl { get; set; }

        public string? GroepenBaseUrl { get; set; }

        public string? VacObjectenBaseUrl { get; set; }
        
        public string? VacObjectTypeUrl { get; set; }

        public string? MedewerkerObjectenBaseUrl { get; set; }

        public string? MedewerkerObjectTypeUrl { get; set; }

        public List<RegistryModel> Registries { get; set; } = [];


    }
}
