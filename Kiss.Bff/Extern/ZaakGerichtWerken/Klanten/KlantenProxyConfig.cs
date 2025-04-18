using Kiss.Bff.Extern;
using Microsoft.AspNetCore.Mvc;

namespace Kiss.Bff.ZaakGerichtWerken.Klanten
{
    [ApiController]
    [Route("/api/klanten/{**path}")]
    public class KlantenProxy(RegistryConfig registryConfig) : ControllerBase
    {
        private readonly RegistryConfig _registryConfig = registryConfig;

        [HttpPost]
        [HttpGet]
        public IActionResult Handle([FromRoute] string path, [FromHeader(Name = "systemIdentifier")] string? systemIdentifier)
        {
            var registry = _registryConfig.Systemen.FirstOrDefault(x => x.Identifier == systemIdentifier)?.KlantRegistry;

            if (registry == null)
            {
                return BadRequest($"FOUT: Geen klantregistratie gevonden voor systemIdentifier '{systemIdentifier ?? "null"}'");
            }

            return new ProxyResult(() =>
            {
                var message = new HttpRequestMessage(new HttpMethod(Request.Method), GetUri(registry, path))
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
        private Uri GetUri(KlantRegistry config, string path) => new Uri($"{config.BaseUrl.AsSpan().TrimEnd('/')}/{path}?{Request.QueryString.Value?.TrimStart('?')}");
    }
}
