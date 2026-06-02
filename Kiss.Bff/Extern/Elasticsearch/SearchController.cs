using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Kiss.Bff.Extern.Elasticsearch
{
    [Route("api/elasticsearch")]
    [ApiController]
    public class ElasticsearchController : ControllerBase
    {
        private readonly ElasticsearchService _elasticsearchService;
        private readonly ILogger<ElasticsearchController> _logger;

        public ElasticsearchController(
            ElasticsearchService elasticsearchService,
            ILogger<ElasticsearchController> logger)
        {
            _elasticsearchService = elasticsearchService;
            _logger = logger;
        }

        [HttpPost("/api/search")]
        [Authorize(Policy = Policies.KcmOrKennisbankPolicy)]
        public async Task<IActionResult> GlobalSearch([FromBody] SearchRequest request, CancellationToken cancellationToken)
        {
            try
            {
                var responseBody = await _elasticsearchService.GlobalSearch(request, cancellationToken);
                 return Ok(responseBody);
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError("{Message}", ex.Message);
                return StatusCode((int?)ex.StatusCode ?? StatusCodes.Status500InternalServerError);
            }
        }

        [HttpGet("/api/search/sources")]
        [Authorize(Policy = Policies.KcmOrKennisbankPolicy)]
        public async Task<IActionResult> GetSources(CancellationToken cancellationToken)
        {
            try
            {
                var responseBody = await _elasticsearchService.GetSources(cancellationToken);
                return Ok(responseBody);
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError("{Message}", ex.Message);
                return StatusCode((int?)ex.StatusCode ?? StatusCodes.Status500InternalServerError);
            }
        }

        [HttpPost("/api/search/medewerkers")]
        [Authorize(Policy = Policies.KcmOrKennisbankPolicy)]
        public async Task<IActionResult> SearchMedewerkers([FromBody] MedewerkerSearchRequest request, CancellationToken cancellationToken)
        {
            try
            {
                var responseBody = await _elasticsearchService.SearchMedewerkers(request, cancellationToken);
                return Ok(responseBody);
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError("{Message}", ex.Message);
                return StatusCode((int?)ex.StatusCode ?? StatusCodes.Status500InternalServerError);
            }
        }
    }
}
