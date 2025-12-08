using System.Runtime.CompilerServices;
using System.Security.Claims;
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

        public ElasticsearchService(HttpClient httpClient, IsKennisbank isKennisbank, IsKcm isKcm, ClaimsPrincipal user, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _isKennisbank = isKennisbank;
            _isKcm = isKcm;
            _user = user;
            var excludedFields = configuration["ELASTIC_EXCLUDED_FIELDS_KENNISBANK"];
            _excludedFieldsForKennisbank = string.IsNullOrWhiteSpace(excludedFields) ? [] : excludedFields.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        }

        /// <summary>
        /// Executes an Elasticsearch search request with request/response transformations
        /// </summary>
        /// <param name="url">The Elasticsearch endpoint URL</param>
        /// <param name="elasticQuery">The JSON query object to send to Elasticsearch</param>
        /// <returns>Transformed Elasticsearch response</returns>
        public async Task<ElasticResponse?> Search(string url, JsonObject elasticQuery, CancellationToken cancellationToken)
        {
            ApplyRequestTransform(elasticQuery);
            var esResponse = await _httpClient.PostAsJsonAsync(url, elasticQuery, cancellationToken);

            if (!esResponse.IsSuccessStatusCode)
            {
                var errorBody = await esResponse.Content.ReadAsStringAsync(cancellationToken);
                throw new HttpRequestException($"Elasticsearch request failed: {errorBody}", null, esResponse.StatusCode);
            }

            var esResponseBody = await esResponse.Content.ReadFromJsonAsync<ElasticResponse>(cancellationToken);
            ApplyResponseTransform(esResponseBody);

            return esResponseBody;
        }


        /// <summary>
        /// Transform the request query based on user role.
        /// Removes fields that should not be searched from the query.
        /// </summary>
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

        /// <summary>
        /// Recursively traverses JSON structure and removes values starting with fieldName from arrays and objects
        /// </summary>
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

        /// <summary>
        /// Transform the response body by removing excluded fields from the search results
        /// Filters out any restricted fields for Kennisbank users
        /// </summary>
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

        /// <summary>
        /// Recursively moves down the field path and removes the field object if it is found.
        /// </summary>
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

            // Check if the current head of the path is in the given node object.
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
                    // If the node is an array, don't skip the field path until an object is encountered.
                    RemoveFieldRecursively(item, fieldPath);
                }
            }
        }
        /// <summary>
        /// Checks if the user only has the Kennisbank role.
        /// </summary>
        private bool IsOnlyKennisbank()
        {
            return _isKennisbank(_user) && !_isKcm(_user);
        }
    }
}