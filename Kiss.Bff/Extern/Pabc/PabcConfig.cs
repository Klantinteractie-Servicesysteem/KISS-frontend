namespace Kiss.Bff.Extern.Pabc
{
    public class PabcConfig
    {
        public required string BaseUrl { get; init; }
        public required string ApiKey { get; init; }
        public string ApplicationName => "kiss";
        public string ApplicationRole => "klantcontactmedewerker";
    }
}
