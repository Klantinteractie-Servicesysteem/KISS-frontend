using Kiss.Bff.Extern.Pabc;
using Microsoft.AspNetCore.Mvc;

namespace Kiss.Bff.Extern.ZaakGerichtWerken.Zaaksysteem
{
    [ApiController]
    public class ZaaksysteemProxy(RegistryConfig registryConfig, ILogger<ZaaksysteemProxy> logger, PabcClient? pabcClient = null) : ControllerBase
    {
        /// <summary>
        /// Proxyt zaken API calls naar het juiste zaaksysteem endpoint.
        /// Verrijkt altijd met zaaktype details. Wanneer PABC geconfigureerd is,
        /// worden zaken ook gefilterd op basis van toegestane zaaktypes.
        /// </summary>
        [HttpGet("api/zaken/{**path}")]
        public IActionResult GetZaken(string path, [FromHeader(Name = "systemIdentifier")] string systemIdentifier)
        {
            if (!path.StartsWith("zaken"))
            {
                // Non-zaak paths (rollen, statussen, etc.) — proxy without enrichment
                return ProxyToEndpoint(path, systemIdentifier, "zaken");
            }

            return ProxyToEndpointWithEnrichment(path, systemIdentifier);
        }

        /// <summary>
        /// Proxyt catalogi API calls naar het juiste zaaksysteem endpoint
        /// </summary>
        [HttpGet("api/catalogi/{**path}")]
        public IActionResult GetCatalogi(string path, [FromHeader(Name = "systemIdentifier")] string systemIdentifier)
            => ProxyToEndpoint(path, systemIdentifier, "catalogi");

        /// <summary>
        /// Proxyt documenten API calls naar het juiste zaaksysteem endpoint
        /// </summary>
        [HttpGet("api/documenten/{**path}")]
        public IActionResult GetDocumenten(string path, [FromHeader(Name = "systemIdentifier")] string systemIdentifier)
            => ProxyToEndpoint(path, systemIdentifier, "documenten");

        private IActionResult ProxyToEndpointWithEnrichment(string path, string systemIdentifier)
        {
            var config = registryConfig.GetRegistrySystem(systemIdentifier)?.ZaaksysteemRegistry;

            if (config == null)
            {
                return LogAndReturnConfigError(systemIdentifier);
            }

            var baseUrl = config.ZakenBaseUrl;

            return new ZaaktypeEnrichedProxyResult(
                () =>
                {
                    var url = $"{baseUrl.AsSpan().TrimEnd('/')}/{path}{Request?.QueryString}";
                    var message = new HttpRequestMessage(HttpMethod.Get, url);
                    config.ApplyHeaders(message.Headers, User);
                    return message;
                },
                User,
                config.CatalogiBaseUrl,
                config,
                pabcClient);
        }

        /// <summary>
        /// Generieke proxy methode die routeert naar het juiste zaaksysteem endpoint gebaseerd op API type
        /// Ondersteunt zowel OpenZaak formaat (enkele BaseUrl) als Rx.Mission formaat (aparte endpoints)
        /// </summary>
        private IActionResult ProxyToEndpoint(string path, string systemIdentifier, string apiType)
        {
            var config = registryConfig.GetRegistrySystem(systemIdentifier)?.ZaaksysteemRegistry;

            if (config == null)
            {
                return LogAndReturnConfigError(systemIdentifier);
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

        private IActionResult LogAndReturnConfigError(string systemIdentifier)
        {
            var sanitizedSystemIdentifier = systemIdentifier.Replace("\n", "").Replace("\r", "").Replace("\t", "");
            logger.LogError("Geen zaaksysteem gevonden voor ZaaksysteemId {ZaaksysteemId}",
                sanitizedSystemIdentifier[..(sanitizedSystemIdentifier.Length < 15 ? sanitizedSystemIdentifier.Length - 1 : 15)] + "...");
            return Problem(
                title: "Configuratieprobleem",
                detail: "Geen zaaksysteem gevonden voor ZaaksysteemId " + systemIdentifier,
                statusCode: 500
            );
        }

        /// <summary>
        /// Bepaalt de base URL voor het opgegeven API type.
        /// </summary>
        private static string GetBaseUrlForApiType(ZaaksysteemRegistry config, string apiType)
        {
            return apiType switch
            {
                "zaken" => config.ZakenBaseUrl,
                "catalogi" => config.CatalogiBaseUrl,
                "documenten" => config.DocumentenBaseUrl,
                _ => throw new ArgumentException($"Unknown API type: {apiType}", nameof(apiType))
            };
        }
    }
}
