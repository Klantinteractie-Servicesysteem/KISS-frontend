using System.Security.Claims;
using System.Text.Json.Nodes;

namespace Kiss.Bff.Extern.ElasticSearch
{
    public class ElasticsearchService
    {
        private readonly HttpClient _httpClient;
        private readonly IsKennisbank _isKennisbank;
        private readonly ClaimsPrincipal _user;
        private readonly string[] _excludedFieldsForKennisbank;

        public ElasticsearchService(HttpClient httpClient, IsKennisbank isKennisbank, ClaimsPrincipal user)
        {
            _httpClient = httpClient;
            _isKennisbank = isKennisbank;
            _user = user;
            _excludedFieldsForKennisbank = ["Kennisbank.vertalingen.deskMemo"];
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
        /// Transform the request query based on user role
        /// Applies field exclusions for Kennisbank users
        /// </summary>
        private void ApplyRequestTransform(JsonObject query)
        {
            if (!_isKennisbank(_user) || _excludedFieldsForKennisbank.Length == 0)
            {
                return;
            }

            // Add _source.excludes to query
            if (!query.ContainsKey("_source"))
            {
                query["_source"] = new JsonObject
                {
                    ["excludes"] = new JsonArray([.. _excludedFieldsForKennisbank.Select(fieldName => JsonValue.Create(fieldName))])
                };
            }
            else if (query["_source"] is JsonObject sourceObj)
            {
                if (!sourceObj.ContainsKey("excludes"))
                {
                    sourceObj["excludes"] = new JsonArray([.. _excludedFieldsForKennisbank.Select(fieldName => JsonValue.Create(fieldName))]);
                }
                else if (sourceObj["excludes"] is JsonArray existingExcludes)
                {
                    // Merge with existing excludes
                    foreach (var field in _excludedFieldsForKennisbank)
                    {
                        if (!existingExcludes.Any(existingField => existingField?.ToString() == field))
                        {
                            existingExcludes.Add(JsonValue.Create(field));
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Transform the response body by removing excluded fields from the search results
        /// Filters out any restricted fields for Kennisbank users
        /// </summary>
        private ElasticResponse? ApplyResponseTransform(ElasticResponse? responseBody)
        {
            // User has Kennisbank role and there are fields to exclude
            if (_isKennisbank(_user) && _excludedFieldsForKennisbank.Length > 0)
            {
                if (responseBody?.Hits?.Hits != null)
                {
                    foreach (var hit in responseBody.Hits.Hits)
                    {
                        if (hit.Source != null)
                            // Remove excluded fields from _source
                            RemoveExcludedFields(hit.Source, _excludedFieldsForKennisbank);
                    }
                }
            }
            return responseBody;
        }

        /// <summary>
        /// Recursively remove excluded fields from a JSON object
        /// Example: "toelichting" removes the field from VAC.toelichting, Kennisartikel.vertalingen.toelichting, etc.
        /// </summary>
        private void RemoveExcludedFields(JsonNode obj, string[] excludedFields)
        {
            foreach (var fieldPath in excludedFields)
            {
                var splitPath = fieldPath.Split('.');
                RemoveFieldRecursively(obj, splitPath);
            }
        }

        /// <summary>
        /// Recursively moves down the field path and removes the field object if it is found.
        /// </summary>
        private void RemoveFieldRecursively(JsonNode? node, string[] fieldPath)
        {
            // Base cases
            if (node == null || fieldPath.Length == 0) return;

            if (fieldPath.Length == 1 && node is JsonObject fieldObject)
            {
                fieldObject.Remove(fieldPath[0]);
                return;
            }

            var headOfPath = fieldPath[0];
            var tailOfPath = fieldPath[1..];

            // Check if the current head of the path is in the given node object.
            if (node is JsonObject jsonObject && jsonObject.ContainsKey(headOfPath))
            {
                RemoveFieldRecursively(jsonObject[headOfPath], tailOfPath);
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
    }
}