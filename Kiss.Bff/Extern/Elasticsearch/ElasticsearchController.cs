using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Kiss.Bff.Extern.ElasticSearch
{
    /// <summary>
    /// Elasticsearch proxy controller with request and response body transformation
    /// </summary>
    [Route("api/elasticsearch")]
    [ApiController]
    public class ElasticsearchController : ControllerBase
    {
        private readonly HttpClient _httpClient;
        private readonly string _elasticsearchBaseUrl;
        private readonly string _elasticsearchUsername;
        private readonly string _elasticsearchPassword;
        private readonly string[] _excludedFieldsForKennisbank;
        private readonly ILogger<ElasticsearchController> _logger;
        private readonly IsKennisbank _isKennisbank;

        public ElasticsearchController(
            HttpClient httpClient,
            IConfiguration configuration,
            ILogger<ElasticsearchController> logger,
            IsKennisbank isKennisbank)
        {
            // Load configuration
            _elasticsearchBaseUrl = configuration["ELASTIC_BASE_URL"] ?? throw new InvalidOperationException("ELASTIC_BASE_URL not configured");
            _elasticsearchUsername = configuration["ELASTIC_USERNAME"] ?? throw new InvalidOperationException("ELASTIC_USERNAME not configured");
            _elasticsearchPassword = configuration["ELASTIC_PASSWORD"] ?? throw new InvalidOperationException("ELASTIC_PASSWORD not configured");

            _httpClient = httpClient;
            _httpClient.BaseAddress = new Uri(_elasticsearchBaseUrl);
            _logger = logger;
            _isKennisbank = isKennisbank;

            var excludedFields = configuration["ELASTICSEARCH_KENNISBANK_EXCLUDED_FIELDS"];
            _excludedFieldsForKennisbank = string.IsNullOrWhiteSpace(excludedFields)
                ? []
                : excludedFields.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        }

        [HttpPost("{index}/_search")]
        [Authorize(Policy = Policies.KcmOrKennisbankPolicy)]
        public async Task<IActionResult> Search([FromRoute] string index)
        {
            var cancellationToken = Request.HttpContext.RequestAborted;

            try
            {
                // Read and parse request body
                string requestBody;
                using (var reader = new StreamReader(Request.Body, Encoding.UTF8))
                {
                    requestBody = await reader.ReadToEndAsync();
                }

                if (string.IsNullOrWhiteSpace(requestBody))
                {
                    return BadRequest(new { error = "Request body cannot be empty" });
                }

                JsonObject? elasticqQuery;
                try
                {
                    var queryNode = JsonNode.Parse(requestBody);
                    elasticqQuery = queryNode?.AsObject();

                    if (elasticqQuery == null)
                    {
                        return BadRequest(new { error = "Invalid query format: expected JSON object" });
                    }
                }
                catch (JsonException ex)
                {
                    _logger.LogError(ex, "Invalid JSON in request body");
                    return BadRequest(new { error = "Invalid JSON in request body" });
                }

                // Transform request (apply role-based filtering)
                ApplyRequestTransform(elasticqQuery);

                // Forward request to Elasticsearch
                var modifiedRequestBody = elasticqQuery.ToJsonString();

                var httpRequest = new HttpRequestMessage(HttpMethod.Post, $"{index}/_search")
                {
                    Content = new StringContent(modifiedRequestBody, Encoding.UTF8, "application/json")
                };

                ApplyAuthenticationHeaders(httpRequest.Headers);

                // Send request
                var esResponse = await _httpClient.SendAsync(httpRequest, HttpCompletionOption.ResponseContentRead, cancellationToken);

                // Catch any invalid statuscode here in order to add responseBody as reference to logger.
                if (!esResponse.IsSuccessStatusCode)
                {
                    var errorBody = await esResponse.Content.ReadAsStringAsync(cancellationToken);
                    _logger.LogError("Elasticsearch returned error. Status: {statusCode}, Elastcsearch error response: {Body}", esResponse.StatusCode, errorBody);
                    return StatusCode(502, new { error = errorBody, statusCode = (int)esResponse.StatusCode });
                }

                // Read response body
                var responseBody = await esResponse.Content.ReadFromJsonAsync<ElasticResponse>(cancellationToken);

                // Transform response (if needed)
                var transformedResponse = ApplyResponseTransform(responseBody);

                // Return transformed response
                return new ContentResult
                {
                    Content = transformedResponse,
                    ContentType = "application/json",
                    StatusCode = (int)esResponse.StatusCode
                };
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Error connecting to search service");
                return StatusCode(502, new { error = "Error connecting to search service" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unexpected error occurred");
                return StatusCode(500, new { error = "An unexpected error occurred" });
            }
        }

        /// <summary>
        /// Transform the request query based on user role
        /// Applies field exclusions for Kennisbank users
        /// </summary>
        private void ApplyRequestTransform(JsonObject query)
        {
            if (!_isKennisbank(User) || _excludedFieldsForKennisbank.Length == 0)
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
        private string ApplyResponseTransform(ElasticResponse? responseBody)
        {
            // User has Kennisbank role and there are fields to exclude
            if (_isKennisbank(User) && _excludedFieldsForKennisbank.Length > 0)
            {
                try
                {
                    if (responseBody?.Hits?.Hits != null)
                    {
                        foreach (var hit in responseBody.Hits.Hits)
                        {
                            if (hit.Source != null)
                            {
                                RemoveExcludedFields(hit.Source, _excludedFieldsForKennisbank);
                            }
                        }
                    }
                }
                catch (JsonException ex)
                {
                    _logger.LogError(ex, "Failed to transform Elasticsearch response");
                    return JsonSerializer.Serialize(responseBody);
                }
            }
            return JsonSerializer.Serialize(responseBody);
        }

        /// <summary>
        /// Remove excluded fields from source data by finding the source and removing any specified labels.
        /// Example: excluded field "VAC.toelichting" removes the field from VAC.toelichting, but not from Kennisartikel.toelichting.
        /// </summary>
        private void RemoveExcludedFields(SourceData obj, string[] excludedFields)
        {
            if (obj.SourceExtensionData == null) return;

            foreach (var fieldName in excludedFields)
            {
                var sourceAndFieldName = splitExcludedFieldNameFromObjectSource(fieldName);

                if (obj.ObjectBron?.Equals(sourceAndFieldName.Key) ?? false)
                {
                    // Try to find source element by name in the source data. 
                    if (obj.SourceExtensionData.TryGetValue(sourceAndFieldName.Key, out var sourceElement)
                        && sourceElement.ValueKind == JsonValueKind.Object)
                    {
                        // The source element needs to be deserialized into dictionary for easy removal of the excluded field.
                        var dict = sourceElement.Deserialize<Dictionary<string, JsonElement>>();
                        if (dict != null && dict.Remove(sourceAndFieldName.Value))
                        {
                            // After successful removal the new filtered source element can be returned into the source data.
                            obj.SourceExtensionData[sourceAndFieldName.Key] =
                                JsonSerializer.SerializeToElement(dict);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Apply Basic Authentication headers to the request
        /// </summary>
        private void ApplyAuthenticationHeaders(HttpRequestHeaders headers)
        {
            var authValue = Convert.ToBase64String(
                Encoding.ASCII.GetBytes($"{_elasticsearchUsername}:{_elasticsearchPassword}")
            );
            headers.Authorization = new AuthenticationHeaderValue("Basic", authValue);
        }

        /// <summary>
        /// Splits a given source.fieldname string on the '.' symbol.
        /// Returns a key-value pair of strings: (source, fieldname).
        /// </summary>
        private KeyValuePair<string, string> splitExcludedFieldNameFromObjectSource(string stringToSplit)
        {
            var splittedString = stringToSplit.Split('.', StringSplitOptions.TrimEntries);
            return new KeyValuePair<string, string>(splittedString.ElementAtOrDefault(0) ?? "", splittedString.LastOrDefault() ?? "");
        }
    }
}



