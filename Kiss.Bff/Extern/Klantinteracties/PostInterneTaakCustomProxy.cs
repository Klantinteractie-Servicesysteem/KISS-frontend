using Microsoft.AspNetCore.Mvc;
using System.Text.Json.Nodes;

namespace Kiss.Bff.Extern.Klantinteracties
{
    /// <summary>
    /// Since multiple combinations of registers can be used, 
    /// it must be determined in real time which register is used for storing contactverzoeken (interne taak).
    /// A systemIdentifier header is sent from the client for this purpose.
    /// This endpoint is used for registries that support the the klantineracties API (openklant 2 or higher).
    /// </summary>
    public class PostInterneTaakCustomProxy(RegistryConfig registryConfig) : ControllerBase
    {
        [HttpPost("api/postinternetaak")]
        public IActionResult PostInterneTaak([FromBody] JsonObject parsedModel, [FromHeader(Name = "systemIdentifier")] string systemIdentifier)
        {
            var config = registryConfig.Systemen.FirstOrDefault(x => x.Identifier == systemIdentifier)?.KlantinteractieRegistry;

            if (config == null) return BadRequest($"Geen Internetaakregister gevonden voor deze systemIdentifier: '{systemIdentifier ?? "null"}'");

            var url = config.BaseUrl.TrimEnd('/') + "/api/v1/internetaken";

            return new ProxyResult(() =>
            {
                var request = new HttpRequestMessage(HttpMethod.Post, url) { Content = JsonContent.Create(parsedModel) };
                config.ApplyHeaders(request.Headers, User);
                return request;
            });
        }
    }
}
