using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Kiss.Bff.Extern.Elasticsearch
{
    [Route("api/search")]
    [ApiController]
    [Authorize(Policy = Policies.KcmOrKennisbankPolicy)]
    public class SearchController : ControllerBase
    {
        private readonly ElasticsearchService _elasticsearchService;
        private readonly ILogger<SearchController> _logger;

        public SearchController(
            ElasticsearchService elasticsearchService,
            ILogger<SearchController> logger)
        {
            _elasticsearchService = elasticsearchService;
            _logger = logger;
        }

        [HttpPost]
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

        [HttpGet("sources")]
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

        [HttpPost("medewerkers")]
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
