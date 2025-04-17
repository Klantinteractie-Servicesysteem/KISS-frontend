using Microsoft.AspNetCore.Mvc;

namespace Kiss.Bff.Extern.ZaakGerichtWerken.Contactmomenten
{
    [ApiController]
    [Route("/api/contactmomenten/{**path}")]
    public class ContactmomentenProxy(RegistryConfig registryConfig) : ControllerBase
    {
        private readonly RegistryConfig _registryConfig = registryConfig;

        [HttpPost]
        [HttpGet]
        public IActionResult Handle([FromRoute] string path, [FromHeader(Name = "systemIdentifier")] string? systemIdentifier)
        {
            var registry = _registryConfig.GetRegistrySystem(systemIdentifier)?.ContactmomentRegistry;

            if (registry == null) return BadRequest($"Geen Contactmomentenregister gevonden voor deze systemIdentifier: '{systemIdentifier ?? "null"}'");


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

        private Uri GetUri(ContactmomentRegistry config, string path) => new Uri($"{config.BaseUrl.AsSpan().TrimEnd('/')}/{path}?{Request.QueryString}");
    }
}
