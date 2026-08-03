namespace Kiss.Bff.Extern.Pabc
{
    public static class PabcExtensions
    {
        /// <summary>
        /// Registers the PABC service if the required configuration is present.
        /// Returns true if PABC is configured, false otherwise.
        /// </summary>
        public static bool AddPabcClient(this IServiceCollection services, IConfiguration configuration)
        {
            var baseUrl = configuration["PABC_BASE_URL"];
            var apiKey = configuration["PABC_API_KEY"];

            if (string.IsNullOrWhiteSpace(baseUrl) || string.IsNullOrWhiteSpace(apiKey))
            {
                return false;
            }

            var trimmedBaseUrl = baseUrl.TrimEnd('/');

            services.AddHttpClient<PabcClient>(client =>
            {
                client.BaseAddress = new Uri(trimmedBaseUrl + "/");
                client.DefaultRequestHeaders.Add("X-API-KEY", apiKey);
            });

            return true;
        }
    }
}
