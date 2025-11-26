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
        private readonly string _kennisbankRole;
        private readonly ILogger<ElasticsearchController> _logger;

        public ElasticsearchController(
            HttpClient httpClient,
            IConfiguration configuration,
            ILogger<ElasticsearchController> logger)
        {
            // Load configuration
            _elasticsearchBaseUrl = configuration["ELASTIC_BASE_URL"] ?? throw new InvalidOperationException("ELASTIC_BASE_URL not configured");
            _elasticsearchUsername = configuration["ELASTIC_USERNAME"] ?? throw new InvalidOperationException("ELASTIC_USERNAME not configured");
            _elasticsearchPassword = configuration["ELASTIC_PASSWORD"] ?? throw new InvalidOperationException("ELASTIC_PASSWORD not configured");

            _httpClient = httpClient;
            _httpClient.BaseAddress = new Uri(_elasticsearchBaseUrl);
            _logger = logger;
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

                // Read response body
                // var responseBody = await esResponse.Content.ReadAsStringAsync(cancellationToken);
                var responseBody = await esResponse.Content.ReadFromJsonAsync<ElasticResponse>(cancellationToken);

                // Catch any invalid statuscode here in order to add responseBody as reference to logger.
                if (!esResponse.IsSuccessStatusCode)
                {
                    _logger.LogError("Elasticsearch returned error. Status: {statusCode}, Elastcsearch error response: {Body}", esResponse.StatusCode, responseBody);
                    return StatusCode(502, new { error = responseBody });
                }

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
            // Check if user is Kennisbank
            var isKennisbank = User?.IsInRole(_kennisbankRole) ?? false;

            if (!isKennisbank || _excludedFieldsForKennisbank.Length == 0)
            {
                return; // No transformation needed
            }

            // Add _source.excludes to query
            if (!query.ContainsKey("_source"))
            {
                query["_source"] = new JsonObject
                {
                    ["excludes"] = new JsonArray([.. _excludedFieldsForKennisbank.Select(fieldName => JsonValue.Create("*." + fieldName))])
                };
            }
            else if (query["_source"] is JsonObject sourceObj)
            {
                if (!sourceObj.ContainsKey("excludes"))
                {
                    sourceObj["excludes"] = new JsonArray([.. _excludedFieldsForKennisbank.Select(fieldName => JsonValue.Create("*." + fieldName))]);
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
            // TODO: implement code to remove any fields that are not allowed for the role the current user has.
            // For now, just pass through
            return JsonSerializer.Serialize(responseBody);
            // Check if user is Kennisbank
            var isKennisbank = User?.IsInRole(_kennisbankRole) ?? false;

            if (!isKennisbank || _excludedFieldsForKennisbank.Length == 0)
            {
                return responseBody; // No transformation needed
            }

            try
            {
                // Parse response JSON
                var responseNode = JsonNode.Parse(responseBody);
                if (responseNode is not JsonObject response)
                {
                    return responseBody; // Invalid format, return as-is
                }

                // Navigate to hits.hits array
                if (response["hits"]?["hits"] is not JsonArray hitsArray)
                {
                    return responseBody; // No hits to filter
                }

                // Process each hit
                foreach (var hitNode in hitsArray)
                {
                    if (hitNode is not JsonObject hit)
                        continue;

                    // Get the _source object
                    if (hit["_source"] is not JsonObject source)
                        continue;

                    // Remove excluded fields from _source
                    RemoveExcludedFields(source, _excludedFieldsForKennisbank);

                }

                // Serialize back to JSON
                return response.ToJsonString(new JsonSerializerOptions { WriteIndented = false });
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "Failed to transform Elasticsearch response");
                return responseBody; // Return original on error
            }
        }

        /// <summary>
        /// Recursively remove excluded fields from a JSON object
        /// Example: "toelichting" removes the field from VAC.toelichting, Kennisartikel.toelichting, etc.
        /// </summary>
        private static void RemoveExcludedFields(JsonObject obj, string[] excludedFields)
        {
            foreach (var fieldName in excludedFields)
            {
                // Remove this field recursively from all objects
                RemoveFieldRecursively(obj, fieldName);
            }
        }

        /// <summary>
        /// Recursively remove a field with the given name from all objects
        /// </summary>
        private static void RemoveFieldRecursively(JsonNode? node, string fieldName)
        {
            if (node is JsonObject obj)
            {
                // Remove the field if it exists
                obj.Remove(fieldName);

                // Recursively process all child objects
                foreach (var property in obj.ToList()) // ToList to avoid modification during iteration
                {
                    RemoveFieldRecursively(property.Value, fieldName);
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
    }
}



