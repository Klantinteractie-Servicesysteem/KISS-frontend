using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;

namespace Kiss.Bff.Extern.InterneTaak
{

    /// <summary>
    /// Since multiple combinations of registers can be used, 
    /// it must be determined in real time which register should be used for interne taken.
    /// A systemIdentifier header is sent from the client for this purpose.
    /// This interne taak endpoint is only used for Openklant 1 registers.
    /// </summary>
    [ApiController]
    public class GetInterneTaakObjectsCustomProxy(RegistryConfig registryConfig) : ControllerBase
    {
        [HttpGet("api/internetaak/api/{version}/objects")]
        public IActionResult Get([FromRoute] string version, [FromHeader(Name = "systemIdentifier")] string? systemIdentifier)
        {
            var registry = registryConfig.GetRegistrySystem(systemIdentifier)?.InterneTaakRegistry;

            if (registry == null) return BadRequest($"Geen Interne Taakregister gevonden voor deze systemIdentifier: '{systemIdentifier ?? "null"}'");

            return new ProxyResult(() =>
            {
                var message = new HttpRequestMessage(new HttpMethod(Request.Method), GetUri(registry, $"api/{version}/objects"))
                {
                    Content = new StreamContent(Request.Body)
                };
                if (!string.IsNullOrWhiteSpace(Request.ContentType))
                {
                    message.Content.Headers.ContentType = new(Request.ContentType);
                }
                message.Content.Headers.ContentLength = Request.ContentLength;
                registry.ApplyHeaders(message.Headers, User);
                return message;
            });
        }

        private Uri GetUri(InternetaakRegistry config, string path)
        {
            var url = $"{config.BaseUrl.AsSpan().TrimEnd('/')}/{path}{Request.QueryString}";
            url = QueryHelpers.AddQueryString(url, "type", config.ObjectTypeUrl);

            return new Uri(url);
        }

    }
}
