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
        private readonly IHttpClientFactory _httpClientFactory;

        private readonly string _elasticsearchBaseUrl;
        private readonly string _elasticsearchUsername;
        private readonly string _elasticsearchPassword;

        public ElasticsearchController(
            IHttpClientFactory httpClientFactory,
            IConfiguration configuration)
        {
            _httpClientFactory = httpClientFactory;

            // Load configuration
            _elasticsearchBaseUrl = configuration["ELASTIC_BASE_URL"] ?? throw new InvalidOperationException("ELASTIC_BASE_URL not configured");
            _elasticsearchUsername = configuration["ELASTIC_USERNAME"] ?? throw new InvalidOperationException("ELASTIC_USERNAME not configured");
            _elasticsearchPassword = configuration["ELASTIC_PASSWORD"] ?? throw new InvalidOperationException("ELASTIC_PASSWORD not configured");
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
                    elasticqQuery = queryNode as JsonObject;

                    if (elasticqQuery == null)
                    {
                        return BadRequest(new { error = "Invalid query format: expected JSON object" });
                    }
                }
                catch (JsonException ex)
                {
                    return BadRequest(new { error = "Invalid JSON in request body" });
                }

                // Transform request (apply role-based filtering)

                // Forward request to Elasticsearch
                var modifiedRequestBody = elasticqQuery.ToJsonString();

                var httpRequest = new HttpRequestMessage(HttpMethod.Post, $"{_elasticsearchBaseUrl}/{index}/_search")
                {
                    Content = new StringContent(modifiedRequestBody, Encoding.UTF8, "application/json")
                };

                ApplyAuthenticationHeaders(httpRequest.Headers);

                // Send request
                var client = _httpClientFactory.CreateClient();
                var esResponse = await client.SendAsync(httpRequest, HttpCompletionOption.ResponseContentRead, cancellationToken);

                // Read response body
                var responseBody = await esResponse.Content.ReadAsStringAsync(cancellationToken);

                // Transform response (if needed)

                // Return transformed response
                return new ContentResult
                {
                    Content = "transformedResponse",
                    ContentType = "application/json",
                    StatusCode = (int)esResponse.StatusCode
                };
            }
            catch (HttpRequestException ex)
            {
                return StatusCode(502, new { error = "Error connecting to search service" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "An unexpected error occurred" });
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



