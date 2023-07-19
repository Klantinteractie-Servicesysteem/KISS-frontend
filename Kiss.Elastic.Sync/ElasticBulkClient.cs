using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.Json.Nodes;
using Kiss.Elastic.Sync.Mapping;

namespace Kiss.Elastic.Sync
{
    internal sealed class ElasticBulkClient : IDisposable
    {
        private readonly HttpClient _httpClient;

        public ElasticBulkClient(Uri baseUri, string username, string password)
        {
            var handler = new HttpClientHandler();
            handler.ClientCertificateOptions = ClientCertificateOption.Manual;
            handler.ServerCertificateCustomValidationCallback =
                (httpRequestMessage, cert, cetChain, policyErrors) =>
                {
                    return true;
                };
            _httpClient = new HttpClient(handler);
            _httpClient.BaseAddress = baseUri;
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", Helpers.EncodeCredential(username, password));
        }

        public void Dispose() => _httpClient?.Dispose();



        public static ElasticBulkClient Create()
        {
            var elasticBaseUrl = Helpers.GetEnvironmentVariable("ELASTIC_BASE_URL");
            var username = Helpers.GetEnvironmentVariable("ELASTIC_USERNAME");
            var password = Helpers.GetEnvironmentVariable("ELASTIC_PASSWORD");

            if (!Uri.TryCreate(elasticBaseUrl, UriKind.Absolute, out var elasticBaseUri))
            {
                throw new Exception("elastic base url is niet valide: " + elasticBaseUrl);
            }

            return new ElasticBulkClient(elasticBaseUri, username, password);
        }

        public async Task<string> IndexBulk(IAsyncEnumerable<KissEnvelope> envelopes, string bron, CompletionMapping mapping, CancellationToken token)
        {
            const string Prefix = "search-";
            var indexName = string.Create(bron.Length + Prefix.Length, bron, (a, b) =>
            {
                Prefix.CopyTo(a);
                b.AsSpan().ToLowerInvariant(a[Prefix.Length..]);
            });

            if (!await EnsureIndex(indexName, mapping, token)) return indexName;
            await using var enumerator = envelopes.GetAsyncEnumerator(token);
            var hasNext = await enumerator.MoveNextAsync();
            const long MaxLength = 50 * 1000 * 1000;
            const byte NewLine = (byte)'\n';
            while (hasNext)
            {
                long written = 0;
                using var content = new PushStreamContent(async (stream) =>
                {
                    using var writer = new Utf8JsonWriter(stream);

                    while (hasNext && written < MaxLength)
                    {
                        writer.WriteStartObject();
                        writer.WritePropertyName("index");
                        writer.WriteStartObject();
                        writer.WriteString("_index", indexName);
                        writer.WriteString("_id", enumerator.Current.Id);
                        writer.WriteEndObject();
                        writer.WriteEndObject();
                        await writer.FlushAsync(token);
                        written += writer.BytesCommitted;
                        writer.Reset();
                        stream.WriteByte(NewLine);
                        written++;

                        enumerator.Current.WriteTo(writer, bron);
                        await writer.FlushAsync(token);
                        written += writer.BytesCommitted;
                        writer.Reset();
                        stream.WriteByte(NewLine);
                        written++;

                        hasNext = await enumerator.MoveNextAsync();
                    }
                });
                content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                using var request = new HttpRequestMessage(HttpMethod.Post, "_bulk")
                {
                    Content = content,
                };
                using var response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, token);
                await Helpers.LogResponse(response, token);
            }

            return indexName;
        }

        private async Task<bool> EnsureIndex(string indexName, CompletionMapping mapping, CancellationToken token)
        {
            using var existsRequest = new HttpRequestMessage(HttpMethod.Head, indexName);
            using var existsResponse = await _httpClient.SendAsync(existsRequest, HttpCompletionOption.ResponseHeadersRead, token);

            if (existsResponse.IsSuccessStatusCode) return true;

            using var bodyStream = Helpers.GetEmbedded("mapping.json") ?? Stream.Null;
            var putBody = JsonNode.Parse(bodyStream);
            var properties = putBody?["mappings"]?["properties"];
            var sourceMappings = mapping?.ToJsonObject()?["properties"]?.AsObject();

            if (properties != null && sourceMappings != null)
            {
                var targetMappings = new JsonObject();
                properties["object"] = new JsonObject
                {
                    ["properties"] = targetMappings,
                    ["type"] = "object"
                };

                foreach (var (key, value) in sourceMappings)
                {
                    targetMappings[key] = value.Deserialize<JsonNode>();
                }
            }

            using var putResponse = await _httpClient.SendJsonAsync(HttpMethod.Put, indexName, putBody!, token);

            await Helpers.LogResponse(putResponse, token);

            return putResponse.IsSuccessStatusCode;
        }
    }
}
