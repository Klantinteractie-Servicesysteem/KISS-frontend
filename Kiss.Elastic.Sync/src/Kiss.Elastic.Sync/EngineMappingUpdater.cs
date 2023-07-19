using System.Net.Http.Headers;

namespace Kiss.Elastic.Sync
{
    public sealed class EngineMappingUpdater : IDisposable
    {
        public static EngineMappingUpdater Create()
        {
            var elasticBaseUrl = Helpers.GetEnvironmentVariable("ELASTIC_BASE_URL");
            var username = Helpers.GetEnvironmentVariable("ELASTIC_USERNAME");
            var password = Helpers.GetEnvironmentVariable("ELASTIC_PASSWORD");

            if (!Uri.TryCreate(elasticBaseUrl, UriKind.Absolute, out var elasticBaseUri))
            {
                throw new Exception("elastic base url is niet valide: " + elasticBaseUrl);
            }

            return new EngineMappingUpdater(elasticBaseUri, username, password);
        }

        private readonly HttpClient _httpClient;

        public EngineMappingUpdater(Uri baseUri, string username, string password)
        {
            var handler = new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = (_, _, _, _) => true
            };

            _httpClient = new HttpClient(handler);
            _httpClient.BaseAddress = baseUri;
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", Helpers.EncodeCredential(username, password));
        }

        public void Dispose() => _httpClient.Dispose();

        public async Task<bool> UpdateMappingForCrawlEngine(CancellationToken token)
        {
            var indexName = ".ent-search-engine-documents-" + Helpers.CrawlEngineName;
            using var existsResponse = await _httpClient.HeadAsync(indexName, token);

            if (!existsResponse.IsSuccessStatusCode) return false;

            using var body = Helpers.GetEmbedded("engine.json");
            using var putResponse = await _httpClient.SendJsonAsync(HttpMethod.Put, indexName + "/_mapping", body, token);

            await Helpers.LogResponse(putResponse, token);

            return putResponse.IsSuccessStatusCode;
        }
    }
}
