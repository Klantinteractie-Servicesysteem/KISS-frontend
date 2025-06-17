using Microsoft.AspNetCore.Mvc;

namespace Kiss.Bff.Extern.ZaakGerichtWerken.Zaaksysteem
{
    [ApiController]
    public class ZaaksysteemProxy(RegistryConfig registryConfig, ILogger<ZaaksysteemProxy> logger) : ControllerBase
    {
        /// <summary>
        /// Proxyt zaken API calls naar het juiste zaaksysteem endpoint
        /// </summary>
        [HttpGet("api/zaken/{**path}")]
        public IActionResult GetZaken(string path, [FromHeader(Name = "systemIdentifier")] string systemIdentifier)
            => ProxyToEndpoint(path, systemIdentifier, "zaken");

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

        /// <summary>
        /// Generieke proxy methode die routeert naar het juiste zaaksysteem endpoint gebaseerd op API type
        /// Ondersteunt zowel OpenZaak formaat (enkele BaseUrl) als Rx.Mission formaat (aparte endpoints)
        /// </summary>
        /// <param name="path">Het API pad om te proxyen</param>
        /// <param name="systemIdentifier">De systeem identifier uit de header</param>
        /// <param name="apiType">Het type API (zaken, catalogi, documenten)</param>
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
        /// Bepaalt de base URL voor het opgegeven API type.
        /// Valt terug naar OpenZaak formaat als specifieke URLs niet zijn geconfigureerd.
        /// </summary>
        /// <param name="config">De zaaksysteem registry configuratie</param>
        /// <param name="apiType">Het API type (zaken, catalogi, documenten)</param>
        /// <returns>De base URL voor het API type</returns>
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
