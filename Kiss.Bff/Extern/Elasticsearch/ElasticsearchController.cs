using System.Text.Json.Nodes;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Kiss.Bff.Extern.Elasticsearch
{
    /// <summary>
    /// Elasticsearch proxy controller with request and response body transformation
    /// </summary>
    [Route("api/elasticsearch")]
    [ApiController]
    public class ElasticsearchController : ControllerBase
    {
        private readonly ElasticsearchService _elasticsearchService;
        private readonly ILogger<ElasticsearchController> _logger;

        /// <summary>
        /// Initializes a new instance of the ElasticsearchController
        /// </summary>
        /// <param name="elasticsearchService">Service for handling Elasticsearch operations</param>
        /// <param name="logger">Logger instance for logging errors and information</param>
        public ElasticsearchController(
            ElasticsearchService elasticsearchService,
            ILogger<ElasticsearchController> logger)
        {
            _elasticsearchService = elasticsearchService;
            _logger = logger;
        }

        /// <summary>
        /// Performs an Elasticsearch search on the specified index with role-based transformations
        /// </summary>
        /// <param name="index">The Elasticsearch index to search</param>
        /// <param name="elasticQuery">The Elasticsearch JSON query object</param>
        /// <returns>Search results from Elasticsearch</returns>
        [HttpPost("{index}/_search")]
        [Authorize(Policy = Policies.KcmOrKennisbankPolicy)]
        public async Task<IActionResult> Search([FromRoute] string index, [FromBody] JsonObject elasticQuery, CancellationToken cancellationToken)
        {
            try
            {
                var searchUrl = $"{index}/_search";
                var responseBody = await _elasticsearchService.Search(searchUrl, elasticQuery, cancellationToken);
                return Ok(responseBody);
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex.Message);
                return StatusCode((int?)ex.StatusCode ?? StatusCodes.Status500InternalServerError);
            }
        }

    }
}



