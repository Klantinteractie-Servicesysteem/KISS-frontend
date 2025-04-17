using System.Text.Json.Nodes;
using Kiss.Bff.Extern;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Win32;

namespace Kiss.Bff.ZaakGerichtWerken.Contactmomenten
{
    [Route("api/postcontactmomenten")]
    [ApiController]
    public class PostContactmomentenCustomProxy : ControllerBase
    {
        private readonly RegistryConfig _configuration;
        private readonly GetMedewerkerIdentificatie _getMedewerkerIdentificatie;

        public PostContactmomentenCustomProxy(RegistryConfig configuration, GetMedewerkerIdentificatie getMedewerkerIdentificatie)
        {
            _configuration = configuration;
            _getMedewerkerIdentificatie = getMedewerkerIdentificatie;
        }

        [HttpPost]
        public IActionResult Post([FromBody] JsonObject parsedModel, [FromHeader(Name = "systemIdentifier")] string systemIdentifier)
        {
            var config = _configuration.Systemen.FirstOrDefault(x => x.Identifier == systemIdentifier)?.ContactmomentRegistry;

            if (config == null) return BadRequest($"Geen Contactmomentenregister gevonden voor deze systemIdentifier: '{systemIdentifier ?? "null"}'");

            var email = User?.GetEmail();
            var userRepresentation = User?.Identity?.Name;
            if (parsedModel != null)
            {
                parsedModel["medewerkerIdentificatie"] = _getMedewerkerIdentificatie();
            }

            var url = config.BaseUrl.TrimEnd('/') + "/contactmomenten/api/v1/contactmomenten";

            return new ProxyResult(() =>
            {
                var request = new HttpRequestMessage(HttpMethod.Post, url)
                {
                    Content = JsonContent.Create(parsedModel)
                };

                config.ApplyHeaders(request.Headers, User!);

                return request;
            });
        }
    }
}
