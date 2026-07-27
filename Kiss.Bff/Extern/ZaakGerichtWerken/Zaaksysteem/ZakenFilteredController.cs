using System.Security.Claims;
using System.Text.Json;
using Kiss.Bff.Extern.Pabc;
using Microsoft.AspNetCore.Mvc;

namespace Kiss.Bff.Extern.ZaakGerichtWerken.Zaaksysteem
{
    [ApiController]
    public class ZakenFilteredController : ControllerBase
    {
        private readonly RegistryConfig _registryConfig;
        private readonly PabcService? _pabcService;
        private readonly ILogger<ZakenFilteredController> _logger;

        public ZakenFilteredController(
            RegistryConfig registryConfig,
            ILogger<ZakenFilteredController> logger,
            PabcService? pabcService = null)
        {
            _registryConfig = registryConfig;
            _logger = logger;
            _pabcService = pabcService;
        }

        /// <summary>
        /// Returns the set of allowed zaaktype identifiers for the current user.
        /// If PABC is not configured, returns null (meaning: no filtering needed).
        /// If PABC is configured but the user has no access, returns an empty array.
        /// </summary>
        [HttpGet("api/pabc/allowed-zaaktypen")]
        public async Task<IActionResult> GetAllowedZaaktypen(CancellationToken cancellationToken)
        {
            if (_pabcService == null)
            {
                return Ok(new { isFiltered = false, zaaktypen = (string[]?)null });
            }

            var allowedZaaktypen = await _pabcService.GetAllowedZaaktypenAsync(User, cancellationToken);

            return Ok(new
            {
                isFiltered = true,
                zaaktypen = allowedZaaktypen?.ToArray() ?? Array.Empty<string>()
            });
        }
    }
}
