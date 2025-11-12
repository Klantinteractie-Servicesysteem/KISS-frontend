using System.Text;
using System.Text.Json.Nodes;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Kiss.Bff.Extern.ElasticSearch
{
    [Route("api/elasticsearch")]
    [ApiController]
    public class ElasticsearchController : ControllerBase
    {
        private readonly string _elasticsearchBaseUrl;
        private readonly string _kennisbankRole;
        private readonly IHttpClientFactory _httpClientFactory;
        public ElasticsearchController(IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {
            _httpClientFactory = httpClientFactory;
            _elasticsearchBaseUrl = configuration["ELASTIC_BASE_URL"] ?? "";
            _kennisbankRole = configuration["OIDC_KENNISBANK_ROLE"] ?? "Kennisbank";
        }


        [HttpPost("{index}/_search")]
        [Authorize(Policy = Policies.KcmOrKennisbankPolicy)]
        public async Task<ActionResult> Search([FromRoute] string index)
        {
            try
            {   
                // Get the body from the request and change it to JSON.

                string requestBody;
                var cancellationToken = Request.HttpContext.RequestAborted;
                
                using (var reader = new StreamReader(Request.Body, Encoding.UTF8))
                {
                    requestBody = await reader.ReadToEndAsync();
                }

                JsonObject? query;
                try
                {
                    var queryNode = JsonNode.Parse(requestBody);
                    query = queryNode as JsonObject;
                    // do some refactoring to the JSON body
                }
                catch (Exception error)
                {
                    //do something
                }

                // Create an HTTPClient that will be used to forward the request.

                var client = _httpClientFactory.CreateClient("elasticsearch");
                var esResponse = await client.PostAsync($"{_elasticsearchBaseUrl}/{index}/_search", new StringContent(requestBody, Encoding.UTF8, "application/json"), cancellationToken);

                // Now setup the response that goes back to the frontend to be similar to the Elasticsearch response.

                Response.StatusCode = (int)esResponse.StatusCode;

                // Copy RESPONSE headers
                foreach (var header in esResponse.Headers)
                {
                    // Skip transfer-encoding header as it indicates the content will be 'chunked',
                    // and that does not lineup with the way we create our response.
                    if (header.Key.Equals("transfer-encoding", StringComparison.OrdinalIgnoreCase)) continue;

                    Response.Headers[header.Key] = header.Value.ToArray();
                }

                // Copy CONTENT headers
                foreach (var header in esResponse.Content.Headers)
                {
                    Response.Headers[header.Key] = header.Value.ToArray();
                }

                // Stream response back to client
                await esResponse.Content.CopyToAsync(Response.Body, cancellationToken);

                return new EmptyResult();
            }
            catch (Exception ex)
            {
                // do something
                return StatusCode(502, new { error = "Error connecting to search service" });
            }
        }
    }
}



