using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace Kiss.Elastic.Sync
{
    public sealed class ElasticEnterpriseSearchClient : IDisposable
    {
        const string EnginesV0Url = "/api/as/v0/engines/";
        const string EnginesV1Url = "/api/as/v1/engines/";
        const string CrawlEngineDomainUrl = $"{EnginesV1Url}{Helpers.CrawlEngineName}/crawler/domains";

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

        public async Task<bool> AddDomain(Uri domainUri, CancellationToken token)
        {
            if (!await AddCrawlEngine(token)) return false;
            if(await DomainExists(domainUri, token)) return true;
            var body = new JsonObject
            {
                ["name"] = domainUri.ToString().TrimEnd('/'),
            };
            using var response = await _httpClient.SendJsonAsync(HttpMethod.Post, CrawlEngineDomainUrl, body, token);
            await Helpers.LogResponse(response, token);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> AddIndexEngineAsync(string indexName, CancellationToken token)
        {
            var engineName = $"engine-{indexName}";

            if (!await EnsureIndexEngineAsync(engineName, indexName, token)) return false;
            if (!await EnsureMetaEngineAsync(engineName, token)) return false;

            var url = $"{EnginesV1Url}{_metaEngine}/source_engines";
            var body = new JsonArray(engineName);

            using var postResponse = await _httpClient.SendJsonAsync(HttpMethod.Post, url, body, token);

            await Helpers.LogResponse(postResponse, token);

            return postResponse.IsSuccessStatusCode;
        }

        private async Task<bool> DomainExists(Uri domainUri, CancellationToken token)
        {
            using var request = new HttpRequestMessage(HttpMethod.Get, CrawlEngineDomainUrl);
            using var response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, token);
            if (!response.IsSuccessStatusCode)
            {
                await Helpers.LogResponse(response, token);
                return false;
            }
            using var stream = await response.Content.ReadAsStreamAsync(token);
            using var doc = await JsonDocument.ParseAsync(stream, cancellationToken: token);
            return UriExists(doc, domainUri);
        }

        private static bool UriExists(JsonDocument doc, Uri uri)
        {
            var domainToMatch = uri.ToString().AsSpan().TrimEnd('/');
            if (!doc.RootElement.TryGetProperty("results", out var resultsProp) ||
                resultsProp.ValueKind != JsonValueKind.Array) return false;

            foreach (var x in resultsProp.EnumerateArray())
            {
                if (x.ValueKind == JsonValueKind.Object &&
                    x.TryGetProperty("name", out var nameProp) &&
                    nameProp.ValueEquals(domainToMatch)) 
                    return true;
            }

            return false;
        }

        private async Task<bool> AddCrawlEngine(CancellationToken token)
        {
            if (!await EnsureCrawlEngineAsync(token)) return false;
            if (!await EnsureMetaEngineAsync(Helpers.CrawlEngineName, token)) return false;

            var url = $"{EnginesV1Url}{_metaEngine}/source_engines";
            var body = new JsonArray(Helpers.CrawlEngineName);

            using var postResponse = await _httpClient.SendJsonAsync(HttpMethod.Post, url, body, token);

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

            using var postResponse = await _httpClient.SendJsonAsync(HttpMethod.Post, EnginesV1Url, body, token);

            await Helpers.LogResponse(postResponse, token);

            return postResponse.IsSuccessStatusCode;
        }

        private async Task<bool> EnsureCrawlEngineAsync(CancellationToken token)
        {
            if (await EngineExistsAsync(Helpers.CrawlEngineName, token)) return true;

            var body = new JsonObject
            {
                ["name"] = Helpers.CrawlEngineName,
                ["language"] = "nl"
            };

            using var postResponse = await _httpClient.SendJsonAsync(HttpMethod.Post, EnginesV1Url, body, token);

            await Helpers.LogResponse(postResponse, token);

            return postResponse.IsSuccessStatusCode;
        }

        private async Task<bool> EnsureIndexEngineAsync(string engineName, string indexName, CancellationToken token)
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


            using var postResponse = await _httpClient.SendJsonAsync(HttpMethod.Post, EnginesV0Url, body, token);

            await Helpers.LogResponse(postResponse, token);

            return postResponse.IsSuccessStatusCode;
        }

        private async Task<bool> EngineExistsAsync(string engineName, CancellationToken token)
        {
            using var headResponse = await _httpClient.HeadAsync(EnginesV1Url + engineName, token);
            await Helpers.LogResponse(headResponse, token);
            return headResponse.IsSuccessStatusCode;
        }

        public void Dispose() => _httpClient.Dispose();
    }
}
