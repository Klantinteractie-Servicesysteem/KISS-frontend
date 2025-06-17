using Microsoft.AspNetCore.Mvc;

namespace Kiss.Bff.Extern.ZaakGerichtWerken.Zaaksysteem
{
    [ApiController]
    public class ZaaksysteemProxy(RegistryConfig registryConfig, ILogger<ZaaksysteemProxy> logger) : ControllerBase
    {
        /// <summary>
        /// Proxies zaken API calls to the appropriate zaaksysteem endpoint
        /// </summary>
        [HttpGet("api/zaken/{**path}")]
        public IActionResult GetZaken(string path, [FromHeader(Name = "systemIdentifier")] string systemIdentifier)
            => ProxyToEndpoint(path, systemIdentifier, "zaken");

        /// <summary>
        /// Proxies catalogi API calls to the appropriate zaaksysteem endpoint
        /// </summary>
        [HttpGet("api/catalogi/{**path}")]
        public IActionResult GetCatalogi(string path, [FromHeader(Name = "systemIdentifier")] string systemIdentifier)
            => ProxyToEndpoint(path, systemIdentifier, "catalogi");

        /// <summary>
        /// Proxies documenten API calls to the appropriate zaaksysteem endpoint
        /// </summary>
        [HttpGet("api/documenten/{**path}")]
        public IActionResult GetDocumenten(string path, [FromHeader(Name = "systemIdentifier")] string systemIdentifier)
            => ProxyToEndpoint(path, systemIdentifier, "documenten");

        /// <summary>
        /// Generic proxy method that routes to the correct zaaksysteem endpoint based on API type
        /// Supports both OpenZaak format (single BaseUrl) and Rx.Mission format (separate endpoints)
        /// </summary>
        /// <param name="path">The API path to proxy</param>
        /// <param name="systemIdentifier">The system identifier from the header</param>
        /// <param name="apiType">The type of API (zaken, catalogi, documenten)</param>
        /// <returns></returns>
        private IActionResult ProxyToEndpoint(string path, string systemIdentifier, string apiType)
        {
            var config = registryConfig.GetRegistrySystem(systemIdentifier)?.ZaaksysteemRegistry;

            if (config == null)
            {
                logger.LogError("Geen zaaksysteem gevonden voor ZaaksysteemId {ZaaksysteemId}", 
                    systemIdentifier[..(systemIdentifier.Length < 15 ? systemIdentifier.Length - 1 : 15)] + "...");
                return Problem(
                    title: "Configuratieprobleem",
                    detail: "Geen zaaksysteem gevonden voor ZaaksysteemId " + systemIdentifier,
                    statusCode: 500
                );
            }

            var baseUrl = GetBaseUrlForApiType(config, apiType);

            return new ProxyResult(() =>
            {
                var url = $"{baseUrl.AsSpan().TrimEnd('/')}/{path}{Request?.QueryString}";
                var message = new HttpRequestMessage(HttpMethod.Get, url);
                config.ApplyHeaders(message.Headers, User);
                return message;
            });
        }

        /// <summary>
        /// Determines the base URL for the specified API type.
        /// Falls back to legacy OpenZaak format if specific URLs are not configured.
        /// </summary>
        /// <param name="config">The zaaksysteem registry configuration</param>
        /// <param name="apiType">The API type (zaken, catalogi, documenten)</param>
        /// <returns>The base URL for the API type</returns>
        private string GetBaseUrlForApiType(ZaaksysteemRegistry config, string apiType)
        {
            return apiType switch
            {
                "zaken" => config.ZakenBaseUrl ?? $"{config.BaseUrl?.TrimEnd('/')}/zaken/api/v1",
                "catalogi" => config.CatalogiBaseUrl ?? $"{config.BaseUrl?.TrimEnd('/')}/catalogi/api/v1",
                "documenten" => config.DocumentenBaseUrl ?? $"{config.BaseUrl?.TrimEnd('/')}/documenten/api/v1",
                _ => throw new ArgumentException($"Unknown API type: {apiType}", nameof(apiType))
            };
        }
    }
}
