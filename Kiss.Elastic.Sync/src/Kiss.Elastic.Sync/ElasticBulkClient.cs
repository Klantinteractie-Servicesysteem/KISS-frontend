using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.Json.Nodes;
using Elastic.Clients.Elasticsearch;
using Elastic.Transport;
using HttpMethod = System.Net.Http.HttpMethod;

namespace Kiss.Elastic.Sync
{
    public sealed class ElasticBulkClient : IDisposable
    {
        const long MaxBytesForBulk = 50_000_000;
        const byte NewLine = (byte)'\n';
        const int MaxPageSizeForScrolling = 10_000;

        private static JsonElement GetFieldMapping()
        {
            using var str = Helpers.GetEmbedded("field.json") ?? Stream.Null;
            using var doc = JsonDocument.Parse(str);
            return doc.RootElement.Clone();
        }

        private static readonly JsonElement s_fieldMapping = GetFieldMapping();

        private readonly HttpClient _httpClient;
        private readonly ElasticsearchClient _elasticsearchClient;
        private readonly int _scrollPageSize;

        public ElasticBulkClient(Uri baseUri, string username, string password, int scrollPageSize = MaxPageSizeForScrolling)
        {
            var handler = new HttpClientHandler();
            handler.ClientCertificateOptions = ClientCertificateOption.Manual;
            // skip checking the certificate because we run Elastic internally, with a local certificate
            handler.ServerCertificateCustomValidationCallback = (a, b, c, d) => true;
            _httpClient = new HttpClient(handler);
            _httpClient.BaseAddress = baseUri;
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", Helpers.EncodeCredential(username, password));
            var clientSettings = new ElasticsearchClientSettings(baseUri)
                .Authentication(new BasicAuthentication(username, password))
                // skip checking the certificate because we run Elastic internally, with a local certificate
                .ServerCertificateValidationCallback((a, b, c, d) => true);

            _elasticsearchClient = new ElasticsearchClient(clientSettings);
            _scrollPageSize = scrollPageSize;
        }

        public void Dispose() => _httpClient?.Dispose();

        public static ElasticBulkClient Create()
        {
            var elasticBaseUrl = Helpers.GetRequiredEnvironmentVariable("ELASTIC_BASE_URL");
            var username = Helpers.GetRequiredEnvironmentVariable("ELASTIC_USERNAME");
            var password = Helpers.GetRequiredEnvironmentVariable("ELASTIC_PASSWORD");

            if (!Uri.TryCreate(elasticBaseUrl, UriKind.Absolute, out var elasticBaseUri))
            {
                throw new Exception("elastic base url is niet valide: " + elasticBaseUrl);
            }

            return new ElasticBulkClient(elasticBaseUri, username, password);
        }



        public async Task IndexBulk(IAsyncEnumerable<KissEnvelope> envelopes, string bron, string indexName, IReadOnlyList<string> completionFields, CancellationToken token)
        {
            if (!await EnsureIndex(bron, indexName, completionFields, token)) return;

            var existingIds = await GetExistingIds(indexName, token);

            await using var enumerator = envelopes.GetAsyncEnumerator(token);
            var hasNext = await enumerator.MoveNextAsync();
            
            // we need to continue sending requests to elasticsearch until:
            // - there are no more records from the source left to process
            // - there are no more records to delete
            // we enter this loop multiple times if there are so much records in the source,
            // that the total size of the request would become too large if we send it to Elasticsearch in one go.
            while (hasNext || existingIds.Count > 0)
            {
                long written = 0;
                using var content = new PushStreamContent(async (stream) =>
                {
                    using var writer = new Utf8JsonWriter(stream);

                    // here we loop through the records from the source and add them to the index,
                    // as long as the request size is not too large
                    while (hasNext && written < MaxBytesForBulk)
                    {
                        existingIds.Remove(enumerator.Current.Id);
                        written += await WriteIndexStatement(writer, stream, indexName, bron, enumerator.Current, token);
                        hasNext = await enumerator.MoveNextAsync();
                    }

                    // once there are no more records from the source left to process,
                    // and there is still room in the request to add delete statements
                    // write a delete statement for all ids that are no longer in the source
                    while (!hasNext && existingIds.Count > 0 && written < MaxBytesForBulk)
                    {
                        var existingId = existingIds.First();
                        existingIds.Remove(existingId);
                        written += await WriteDeleteStatement(stream, writer, indexName, existingId, token);
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
        }

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

        private async Task<bool> EnsureIndex(string bron, string indexName, IReadOnlyList<string> completionFields, CancellationToken token)
        {
            using var existsRequest = new HttpRequestMessage(HttpMethod.Head, indexName);
            using var existsResponse = await _httpClient.SendAsync(existsRequest, HttpCompletionOption.ResponseHeadersRead, token);

            if (existsResponse.IsSuccessStatusCode) return true;

            using var bodyStream = Helpers.GetEmbedded("mapping.json") ?? Stream.Null;
            var putBody = JsonNode.Parse(bodyStream);
            var properties = putBody?["mappings"]?["properties"];
            var sourceMappings = MapCompletionFields(completionFields)?["properties"] as JsonObject;

            if (properties != null && sourceMappings != null)
            {
                var targetMappings = new JsonObject();
                properties[bron] = new JsonObject
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

        private async Task<HashSet<string>> GetExistingIds(string indexName, CancellationToken token)
        {
            // 1 minute is used in the elasticsearch examples
            var scrollDuration = TimeSpan.FromMinutes(1);
            var result = new HashSet<string>();

            var searchResponse = await _elasticsearchClient.SearchAsync<object>(x => x
                    .Index(indexName)
                    // we don't need any stored fields
                    .StoredFields(Array.Empty<string>())
                    // we don't need the source documents
                    .Source(new(false))
                    .Size(_scrollPageSize)
                    // scrolling is the most efficient way to loop through big result sets
                    .Scroll(scrollDuration),
                token);

            if (!searchResponse.IsSuccess())
            {
                throw new Exception("search failed: " + searchResponse.ToString());
            }

            var scrollId = searchResponse.ScrollId;
            var hits = searchResponse.Hits;

            while (scrollId is not null && hits.Count > 0)
            {
                foreach (var id in hits.Select(x=> x.Id).OfType<string>())
                {
                    result.Add(id);
                }

                // get the next result set by specifying the scrollId we got previously
                var scrollResponse = await _elasticsearchClient.ScrollAsync<object>(new ScrollRequest { 
                    ScrollId = scrollId, 
                    Scroll = scrollDuration,
                }, token);

                if (!scrollResponse.IsSuccess())
                {
                    throw new Exception("scroll failed: " + scrollResponse.ToString());
                }

                scrollId = scrollResponse.ScrollId;
                hits = scrollResponse.Hits;
            }

            if (scrollId is not null)
            {
                // it's best practice to clear the active scroll when you are done
                await _elasticsearchClient.ClearScrollAsync(x => x.ScrollId(searchResponse.ScrollId!), token);
            }

            return result;
        }

        private static JsonObject MapCompletionFields(IReadOnlyList<string> completionFields)
        {
            if (!completionFields.Any()) return JsonObject.Create(s_fieldMapping)!;
            var split = completionFields.Select(x => x.Split("."));
            var groupedByFirst = split.GroupBy(x => x[0]);

            var properties = new JsonObject();
            var result = new JsonObject
            {
                ["properties"] = properties
            };

            foreach (var group in groupedByFirst)
            {
                var fields = new List<string>();
                foreach (var item in group)
                {
                    var rest = item.Skip(1);
                    var str = string.Join(".", rest);
                    if (!string.IsNullOrWhiteSpace(str))
                    {
                        fields.Add(str);
                    }
                }
                var value = MapCompletionFields(fields);
                properties[group.Key] = value;
            }

            return result;
        }

        private static async Task<long> WriteIndexStatement(Utf8JsonWriter writer, Stream stream, string indexName, string bron, KissEnvelope envelope, CancellationToken token)
        {
            long written = 0;
            writer.WriteStartObject();
            writer.WritePropertyName("index"u8);
            writer.WriteStartObject();
            writer.WriteString("_index"u8, indexName);
            writer.WriteString("_id"u8, envelope.Id);
            writer.WriteEndObject();
            writer.WriteEndObject();
            await writer.FlushAsync(token);
            written += writer.BytesCommitted;
            writer.Reset();
            stream.WriteByte(NewLine);
            written++;

            envelope.WriteTo(writer, bron);
            await writer.FlushAsync(token);
            written += writer.BytesCommitted;
            writer.Reset();
            stream.WriteByte(NewLine);
            written++;
            return written;
        }

        private static async Task<long> WriteDeleteStatement(Stream stream, Utf8JsonWriter writer, string indexName, string existingId, CancellationToken token)
        {
            writer.WriteStartObject();
            writer.WritePropertyName("delete"u8);
            writer.WriteStartObject();
            writer.WriteString("_index"u8, indexName);
            writer.WriteString("_id"u8, existingId);
            writer.WriteEndObject();
            writer.WriteEndObject();
            await writer.FlushAsync(token);
            var written = writer.BytesCommitted;
            writer.Reset();
            stream.WriteByte(NewLine);
            written++;
            return written;
        }
    }
}
