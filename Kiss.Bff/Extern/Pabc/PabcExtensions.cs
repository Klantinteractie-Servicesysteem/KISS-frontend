namespace Kiss.Bff.Extern.Pabc
{
    public static class PabcExtensions
    {
        /// <summary>
        /// Registers the PABC service if the required configuration is present.
        /// Returns true if PABC is configured, false otherwise.
        /// </summary>
        public static bool AddPabcService(this IServiceCollection services, IConfiguration configuration)
        {
            var baseUrl = configuration["PABC_BASE_URL"];
            var apiKey = configuration["PABC_API_KEY"];

            if (string.IsNullOrWhiteSpace(baseUrl) || string.IsNullOrWhiteSpace(apiKey))
            {
                return false;
            }

            var config = new PabcConfig
            {
                BaseUrl = baseUrl.TrimEnd('/'),
                ApiKey = apiKey
            };

            services.AddSingleton(config);

            services.AddHttpClient<PabcService>(client =>
            {
                client.BaseAddress = new Uri(config.BaseUrl + "/");
                client.DefaultRequestHeaders.Add("X-API-KEY", config.ApiKey);
            });

            return true;
        }
    }
}
