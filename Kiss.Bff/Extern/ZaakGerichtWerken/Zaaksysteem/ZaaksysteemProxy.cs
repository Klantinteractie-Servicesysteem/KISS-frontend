using Microsoft.AspNetCore.Mvc;

namespace Kiss.Bff.Extern.ZaakGerichtWerken.Zaaksysteem
{
    [ApiController]
    public class ZaaksysteemProxy(RegistryConfig registryConfig, ILogger<ZaaksysteemProxy> logger) : ControllerBase
    {
        /// <summary>
        /// Wordt gebruikt voor het proxien van alle zaaksysteem calls waarbij we al weten in welk zaaksysteem de gegevens zitten
        /// gebruik wanneer de bron niet bekend is een custom functie volgens het stramien van Kiss.Bff.Extern.ZaakGerichtWerken.Zaaksysteem.GetZaken
        /// </summary>
        /// <param name="path"></param>
        /// <param name="zaaksysteemId"></param>
        /// <returns></returns>
        [HttpGet("api/zaken/{**path}")]
        [HttpGet("api/documenten/{**path}")]
        public IActionResult Get(
            string path, [FromHeader(Name = "systemIdentifier")] string systemIdentifier)
        {
            var config = registryConfig.GetRegistrySystem(systemIdentifier)?.ZaaksysteemRegistry;

            if (config == null)
            {
                logger.LogError("Geen zaaksysteem gevonden voor ZaaksysteemId {ZaaksysteemId}", systemIdentifier[..(systemIdentifier.Length < 15 ? systemIdentifier.Length - 1 : 15)] + "..." );
                return Problem(
                    title: "Configuratieprobleem",
                    detail: "Geen zaaksysteem gevonden voor ZaaksysteemId " + systemIdentifier,
                    statusCode: 500
                );
            }

            return new ProxyResult(() =>
            {
                var url = $"{config.BaseUrl.AsSpan().TrimEnd('/')}/{path}{Request?.QueryString}";
                var message = new HttpRequestMessage(HttpMethod.Get, url);
                config.ApplyHeaders(message.Headers, User);
                return message;
            });
        }
    }
}
