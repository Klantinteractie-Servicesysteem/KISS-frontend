using System.Runtime.CompilerServices;
using System.Security.Claims;
using System.Text.Json;
using System.Text.Json.Nodes;

[assembly: InternalsVisibleTo("Kiss.Bff.Test")]
namespace Kiss.Bff.Extern.Elasticsearch
{
    public class ElasticsearchService
    {
        private readonly HttpClient _httpClient;
        private readonly IsKennisbank _isKennisbank;
        private readonly IsKcm _isKcm;
        private readonly ClaimsPrincipal _user;
        private readonly string[] _excludedFieldsForKennisbank;
        private readonly string[] _searchIndices;
        private readonly JsonObject _queryTemplate;

        private const int PageSize = 10;

        private static readonly JsonObject BronQuery = JsonNode.Parse("""
            {
              "size": 0,
              "aggs": {
                "bronnen": {
                  "terms": { "field": "object_bron.enum" },
                  "aggs": { "by_index": { "terms": { "field": "_index" } } }
                },
                "domains": {
                  "terms": { "field": "domains.enum" },
                  "aggs": { "by_index": { "terms": { "field": "_index" } } }
                }
              }
            }
            """)!.AsObject();

        public ElasticsearchService(HttpClient httpClient, IsKennisbank isKennisbank, IsKcm isKcm, ClaimsPrincipal user, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _isKennisbank = isKennisbank;
            _isKcm = isKcm;
            _user = user;

            var excludedFields = configuration["ELASTIC_EXCLUDED_FIELDS_KENNISBANK"];
            _excludedFieldsForKennisbank = string.IsNullOrWhiteSpace(excludedFields)
                ? []
                : excludedFields.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

            var indices = configuration["ELASTIC_SEARCH_INDICES"];
            _searchIndices = string.IsNullOrWhiteSpace(indices)
                ? []
                : indices.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

            var templatePath = Path.Combine(AppContext.BaseDirectory, "Extern", "Elasticsearch", "query-template.json");
            var templateJson = File.ReadAllText(templatePath);
            _queryTemplate = JsonNode.Parse(templateJson)!.AsObject();
        }

        public async Task<ElasticResponse?> GlobalSearch(SearchRequest request, CancellationToken cancellationToken)
        {
            var query = BuildGlobalSearchQuery(request);
            ApplyRequestTransform(query);

            var indices = request.Filters.Count > 0
                ? request.Filters.Select(f => f.Index).Distinct().OrderBy(x => x)
                : _searchIndices.AsEnumerable();

            var url = $"{string.Join(",", indices)}/_search";
            var esResponse = await _httpClient.PostAsJsonAsync(url, query, cancellationToken);

            if (!esResponse.IsSuccessStatusCode)
            {
                var errorBody = await esResponse.Content.ReadAsStringAsync(cancellationToken);
                throw new HttpRequestException($"Elasticsearch request failed: {errorBody}", null, esResponse.StatusCode);
            }

            var esResponseBody = await esResponse.Content.ReadFromJsonAsync<ElasticResponse>(cancellationToken);
            ApplyResponseTransform(esResponseBody);

            return esResponseBody;
        }

        public async Task<JsonObject?> GetSources(CancellationToken cancellationToken)
        {
            if (_searchIndices.Length == 0) return null;

            var url = $"{string.Join(",", _searchIndices)}/_search";
            var esResponse = await _httpClient.PostAsJsonAsync(url, BronQuery, cancellationToken);

            if (!esResponse.IsSuccessStatusCode)
            {
                var errorBody = await esResponse.Content.ReadAsStringAsync(cancellationToken);
                throw new HttpRequestException($"Elasticsearch request failed: {errorBody}", null, esResponse.StatusCode);
            }

            return await esResponse.Content.ReadFromJsonAsync<JsonObject>(cancellationToken);
        }

        public async Task<JsonObject?> SearchMedewerkers(MedewerkerSearchRequest request, CancellationToken cancellationToken)
        {
            var query = BuildMedewerkerQuery(request);
            var esResponse = await _httpClient.PostAsJsonAsync("search-smoelenboek/_search", query, cancellationToken);

            if (!esResponse.IsSuccessStatusCode)
            {
                var errorBody = await esResponse.Content.ReadAsStringAsync(cancellationToken);
                throw new HttpRequestException($"Elasticsearch request failed: {errorBody}", null, esResponse.StatusCode);
            }

            return await esResponse.Content.ReadFromJsonAsync<JsonObject>(cancellationToken);
        }

        private JsonObject BuildGlobalSearchQuery(SearchRequest request)
        {
            var templateJson = _queryTemplate.ToJsonString();
            var substituted = templateJson.Replace("{{query}}", JsonEncodedText.Encode(request.Query).ToString());
            var query = JsonNode.Parse(substituted)!.AsObject();

            var page = request.Page < 1 ? 1 : request.Page;
            query["from"] = (page - 1) * PageSize;
            query["size"] = PageSize;

            query["indices_boost"] = JsonNode.Parse("""[{"*": 10}]""");

            query["suggest"] = JsonNode.Parse($$"""
                {
                  "suggestions": {
                    "prefix": {{JsonSerializer.Serialize(request.Query)}},
                    "completion": {
                      "field": "_completion",
                      "skip_duplicates": true,
                      "fuzzy": {}
                    }
                  }
                }
                """);

            var filters = request.Filters ?? [];
            var domains = filters.Where(f => f.Name.StartsWith("http")).Select(f => f.Name).ToList();
            var bronnen = filters.Where(f => !f.Name.StartsWith("http")).Select(f => f.Name).ToList();

            var conditions = new JsonArray();
            if (domains.Count > 0)
                conditions.Add(JsonNode.Parse($$$"""{"terms":{"domains.enum":{{{JsonSerializer.Serialize(domains)}}}}}"""));
            if (bronnen.Count > 0)
                conditions.Add(JsonNode.Parse($$$"""{"terms":{"object_bron.enum":{{{JsonSerializer.Serialize(bronnen)}}}}}"""));

            if (conditions.Count > 0)
            {
                var originalQuery = query["query"]!.DeepClone();
                query["query"] = JsonNode.Parse("{}");
                query["query"]!["bool"] = JsonNode.Parse("{}");
                query["query"]!["bool"]!["must"] = new JsonArray(originalQuery);
                query["query"]!["bool"]!["filter"] = new JsonArray(
                    JsonNode.Parse($$$"""{"bool":{"should":{{{conditions.ToJsonString()}}}}}""")
                );
            }

            return query;
        }

        private static JsonObject BuildMedewerkerQuery(MedewerkerSearchRequest request)
        {
            var mustClauses = new JsonArray();

            if (!string.IsNullOrWhiteSpace(request.Search))
            {
                mustClauses.Add(JsonNode.Parse(
                    "{\"simple_query_string\":{\"query\":" + JsonSerializer.Serialize(request.Search) + ",\"default_operator\":\"and\"}}"));
            }

            if (!string.IsNullOrWhiteSpace(request.FilterField) && !string.IsNullOrWhiteSpace(request.FilterValue))
            {
                mustClauses.Add(JsonNode.Parse(
                    "{\"match\":{\"" + request.FilterField + ".enum\":" + JsonSerializer.Serialize(request.FilterValue) + "}}"));
            }

            return JsonNode.Parse(
                "{\"from\":0,\"size\":30,\"sort\":[{\"Smoelenboek.achternaam.enum\":{\"order\":\"asc\"}}],\"query\":{\"bool\":{\"must\":" + mustClauses.ToJsonString() + "}}}")!.AsObject();
        }

        internal void ApplyRequestTransform(JsonObject query)
        {
            if (IsOnlyKennisbank())
            {
                foreach (var excludedField in _excludedFieldsForKennisbank)
                {
                    RemoveMatchingFieldsRecursive(query, excludedField);
                }
            }
        }

        private static void RemoveMatchingFieldsRecursive(JsonNode? node, string fieldName)
        {
            if (node == null) return;

            if (node is JsonObject jsonObject)
            {
                foreach (var property in jsonObject)
                {
                    if (property.Value is JsonArray jsonArray)
                    {
                        var itemsToRemove = new List<JsonNode?>();

                        foreach (var item in jsonArray)
                        {
                            if (item is JsonValue itemValue)
                            {
                                var valueStr = itemValue.ToString();

                                if (valueStr.StartsWith(fieldName, StringComparison.OrdinalIgnoreCase))
                                {
                                    itemsToRemove.Add(item);
                                }
                            }
                        }

                        foreach (var item in itemsToRemove)
                        {
                            jsonArray.Remove(item);
                        }
                    }

                    RemoveMatchingFieldsRecursive(property.Value, fieldName);
                }
            }
            else if (node is JsonArray jsonArray)
            {
                foreach (var item in jsonArray)
                {
                    RemoveMatchingFieldsRecursive(item, fieldName);
                }
            }
        }

        internal void ApplyResponseTransform(ElasticResponse? responseBody)
        {
            if (IsOnlyKennisbank() && _excludedFieldsForKennisbank.Length > 0)
            {
                if (responseBody?.Hits?.Hits != null)
                {
                    foreach (var hit in responseBody.Hits.Hits)
                    {
                        if (hit.Source != null)
                        {
                            foreach (var fieldPath in _excludedFieldsForKennisbank)
                            {
                                var splitPath = fieldPath.Split('.');
                                RemoveFieldRecursively(hit.Source, splitPath);
                            }
                        }
                    }
                }
            }
        }

        private void RemoveFieldRecursively(JsonNode? node, string[] fieldPath)
        {
            if (node == null || fieldPath.Length == 0) return;

            if (fieldPath.Length == 1 && node is JsonObject fieldObject)
            {
                if (fieldObject.ContainsKey(fieldPath[0]))
                {
                    fieldObject.Remove(fieldPath[0]);
                }
                return;
            }

            var headOfPath = fieldPath[0];
            var tailOfPath = fieldPath[1..];

            if (node is JsonObject jsonObject)
            {
                if (jsonObject.ContainsKey(headOfPath))
                {
                    RemoveFieldRecursively(jsonObject[headOfPath], tailOfPath);
                }
                else
                {
                    foreach (var field in jsonObject)
                    {
                        RemoveFieldRecursively(field.Value, fieldPath);
                    }
                }
            }
            else if (node is JsonArray jsonArray)
            {
                foreach (var item in jsonArray)
                {
                    RemoveFieldRecursively(item, fieldPath);
                }
            }
        }

        private bool IsOnlyKennisbank()
        {
            return _isKennisbank(_user) && !_isKcm(_user);
        }
    }

    public record SearchRequest(string Query, int Page, List<SearchFilter> Filters);
    public record SearchFilter(string Name, string Index);
    public record MedewerkerSearchRequest(string? Search, string? FilterField, string? FilterValue);
}
