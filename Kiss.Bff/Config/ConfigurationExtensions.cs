namespace Kiss.Bff.Config
{
    public static class ConfigurationExtensions
    {
        extension(IConfiguration configuration)
        {
            public string GetRequired(string key)
            {
                var value = configuration[key];
                return string.IsNullOrWhiteSpace(value) ? throw new InvalidOperationException($"Required configuration value '{key}' is missing or empty.") : value;
            }

            public string GetOrEmpty(string key)
            {
                return configuration[key] ?? string.Empty;
            }
        }
    }
}
