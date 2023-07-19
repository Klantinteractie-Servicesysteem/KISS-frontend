using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json.Nodes;

namespace Kiss.Elastic.Sync
{
    public sealed class ElasticEnterpriseSearchClient : IDisposable
    {
        const string EnginesUrl = "/api/as/v0/engines/";

        private readonly HttpClient _httpClient;
        private readonly string _metaEngine;

        public ElasticEnterpriseSearchClient(Uri baseUri, string apiKey, string metaEngine)
        {
            // necessary because enterprise search has a local cert in our cluster
            var handler = new HttpClientHandler();
            handler.ClientCertificateOptions = ClientCertificateOption.Manual;
            handler.ServerCertificateCustomValidationCallback =
                (httpRequestMessage, cert, cetChain, policyErrors) =>
                {
                    return true;
                };

            _httpClient = new HttpClient(handler);
            _httpClient.BaseAddress = baseUri;
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
            _metaEngine = metaEngine;
        }

        public static ElasticEnterpriseSearchClient Create()
        {
            var elasticBaseUrl = Helpers.GetEnvironmentVariable("ENTERPRISE_SEARCH_BASE_URL");
            var elasticApiKey = Helpers.GetEnvironmentVariable("ENTERPRISE_SEARCH_PRIVATE_API_KEY");
            var elasticEngine = Helpers.GetEnvironmentVariable("ENTERPRISE_SEARCH_ENGINE");

            if (!Uri.TryCreate(elasticBaseUrl, UriKind.Absolute, out var elasticBaseUri))
            {
                throw new Exception("elastic base url is niet valide: " + elasticBaseUrl);
            }

            return new ElasticEnterpriseSearchClient(elasticBaseUri, elasticApiKey, elasticEngine);
        }

        public async Task<bool> AddEngineAsync(string indexName, CancellationToken token)
        {
            var engineName = $"engine-{indexName}";

            if (!await EnsureEngineAsync(engineName, indexName, token)) return false;
            if (!await EnsureMetaEngineAsync(engineName, token)) return false;

            var url = $"/api/as/v1/engines/{_metaEngine}/source_engines";
            var body = new JsonArray(engineName);
            using var content = new StringContent(body.ToJsonString(), Encoding.UTF8, "application/json");
            using var request = new HttpRequestMessage(HttpMethod.Post, url) { Content = content };

            using var postResponse = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, token);

            await Helpers.LogResponse(postResponse, token);

            return postResponse.IsSuccessStatusCode;
        }

        private async Task<bool> EnsureMetaEngineAsync(string firstEngineName, CancellationToken token)
        {
            if (await EngineExistsAsync(_metaEngine, token)) return true;

            var body = new JsonObject
            {
                ["name"] = _metaEngine,
                ["type"] = "meta",
                ["source_engines"] = new JsonArray(firstEngineName)
            };

            var str = body.ToJsonString();
            using var content = new StringContent(str, Encoding.UTF8, "application/json");
            using var request = new HttpRequestMessage(HttpMethod.Post, "/api/as/v1/engines/")
            {
                Content = content
            };

            using var postResponse = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, token);

            await Helpers.LogResponse(postResponse, token);

            return postResponse.IsSuccessStatusCode;
        }

        private async Task<bool> EnsureEngineAsync(string engineName, string indexName, CancellationToken token)
        {
            if (await EngineExistsAsync(engineName, token)) return true;

            var body = new JsonObject
            {
                ["name"] = engineName,
                ["search_index"] = new JsonObject
                {
                    ["type"] = "elasticsearch",
                    ["index_name"] = indexName
                },
                ["language"] = "nl"
            };

            var str = body.ToJsonString();
            using var content = new StringContent(str, Encoding.UTF8, "application/json");
            using var request = new HttpRequestMessage(HttpMethod.Post, EnginesUrl)
            {
                Content = content
            };
            using var postResponse = await _httpClient.SendAsync(request, token);

            await Helpers.LogResponse(postResponse, token);

            return postResponse.IsSuccessStatusCode;
        }

        private async Task<bool> EngineExistsAsync(string engineName, CancellationToken token)
        {
            using var headRequest = new HttpRequestMessage(HttpMethod.Head, "/api/as/v1/engines/" + engineName);
            using var headResponse = await _httpClient.SendAsync(headRequest, HttpCompletionOption.ResponseHeadersRead, token);
            await Helpers.LogResponse(headResponse, token);
            return headResponse.IsSuccessStatusCode;
        }

        public void Dispose() => _httpClient.Dispose();
    }
}
