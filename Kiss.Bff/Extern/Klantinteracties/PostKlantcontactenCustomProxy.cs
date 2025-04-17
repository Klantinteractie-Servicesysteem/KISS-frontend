using System.Text.Json.Nodes;
using Microsoft.AspNetCore.Mvc;

namespace Kiss.Bff.Extern.Klantinteracties
{
    /// <summary>
    /// Since multiple combinations of registers can be used, 
    /// it must be determined in real time which register is used for storing contactmomenten (klantcontacten).
    /// A systemIdentifier header is sent from the client for this purpose.
    /// This endpoint is used for registries that support the the klantineracties API (openklant 2 or higher).
    /// A dedicated endpoint is used for contactmomenten instead of the KlantinteractiesProxy because in this case information about the logged in user must be added server side
    /// </summary>
    [Route("api")]
    [ApiController]
    public class PostKlantContactenCustomProxy : ControllerBase
    {
        private readonly GetMedewerkerIdentificatie _getMedewerkerIdentificatie;
        private readonly HttpClient _httpClient;
        private readonly RegistryConfig _registryConfig;

        public PostKlantContactenCustomProxy(GetMedewerkerIdentificatie getMedewerkerIdentificatie, HttpClient httpClient, RegistryConfig registryConfig)
        {
            _getMedewerkerIdentificatie = getMedewerkerIdentificatie;
            _httpClient = httpClient;
            _registryConfig = registryConfig;
        }

        [HttpPost("postklantcontacten")]
        public async Task<IActionResult> PostKlantContacten([FromBody] JsonObject parsedModel, [FromHeader(Name = "systemIdentifier")] string systemIdentifier)
        {
            var registry = _registryConfig.GetRegistrySystem(systemIdentifier)?.KlantinteractieRegistry;

            if (registry == null) return BadRequest($"Geen Contactmomentregister gevonden voor deze systemIdentifier: '{systemIdentifier ?? "null"}'");

            var email = User?.GetEmail();
            var userRepresentation = User?.Identity?.Name;

            var actorUuid = await GetActorId(registry, email);

            if (actorUuid == null)
            {
                actorUuid = await PostActoren(registry);

                if (actorUuid == null)
                {
                    return BadRequest(new { errorMessage = "Failed to create actor." });
                }
            }

            var klantcontact = await PostKlantContact(registry, parsedModel);

            var linkSuccessful = await LinkActorWithKlantContact(registry, actorUuid, klantcontact["uuid"].ToString());

            if (!linkSuccessful)
            {
                return BadRequest(new { errorMessage = "Failed to link actor with klantcontact." });
            }

            return Ok(klantcontact);
        }

        [NonAction]
        public async Task<JsonObject?> PostKlantContact(KlantinteractieRegistry registry, JsonObject parsedModel)
        {
            var url = registry.BaseUrl.TrimEnd('/') + "/api/v1/klantcontacten";

            var request = new HttpRequestMessage(HttpMethod.Post, url)
            {
                Content = JsonContent.Create(parsedModel)
            };

            registry.ApplyHeaders(request.Headers, User);

            using var response = await _httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();

            var jsonResponse = await response.Content.ReadFromJsonAsync<JsonObject>();

            return jsonResponse;
        }

        [NonAction]
        public async Task<string> PostActoren(KlantinteractieRegistry registry)
        {
            var email = User?.GetEmail();
            var firstName = User?.GetFirstName();
            var lastName = User?.GetLastName();
            var fullName = $"{firstName} {lastName}".Trim();

            var parsedModel = new JsonObject
            {
                ["naam"] = fullName,
                ["soortActor"] = "medewerker",
                ["indicatieActief"] = true,
                ["actoridentificator"] = new JsonObject
                {
                    ["objectId"] = email,
                    ["codeObjecttype"] = "mdw",
                    ["codeRegister"] = "msei",
                    ["codeSoortObjectId"] = "email"
                }
            };

            parsedModel["medewerkerIdentificatie"] = _getMedewerkerIdentificatie();

            var url = registry.BaseUrl.TrimEnd('/') + "/api/v1/actoren";

            var request = new HttpRequestMessage(HttpMethod.Post, url)
            {
                Content = JsonContent.Create(parsedModel)
            };

            registry.ApplyHeaders(request.Headers, User);

            using var response = await _httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
            var jsonResponse = JsonNode.Parse(content);

            var actorUuid = jsonResponse?["uuid"]?.ToString();

            return actorUuid ?? throw new Exception("Failed to retrieve actor UUID.");
        }

        [NonAction]
        public async Task<string?> GetActorId(KlantinteractieRegistry registry, string email)
        {
            var url = $"{registry.BaseUrl.TrimEnd('/')}/api/v1/actoren?actoridentificatorObjectId={email}";

            var request = new HttpRequestMessage(HttpMethod.Get, url);

            registry.ApplyHeaders(request.Headers, User);

            using var response = await _httpClient.SendAsync(request);

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var jsonResponse = JsonNode.Parse(content);

                var results = jsonResponse?["results"]?.AsArray();
                if (results != null && results.Count > 0)
                {
                    return results[0]?["uuid"]?.ToString();
                }
            }
            return null;
        }

        [NonAction]
        public async Task<bool> LinkActorWithKlantContact(KlantinteractieRegistry registry, string actorUuid, string klantcontactUuid)
        {
            var url = registry.BaseUrl.TrimEnd('/') + "/api/v1/actorklantcontacten";

            var payload = new JsonObject
            {
                ["actor"] = new JsonObject { ["uuid"] = actorUuid },
                ["klantcontact"] = new JsonObject { ["uuid"] = klantcontactUuid }
            };

            var request = new HttpRequestMessage(HttpMethod.Post, url)
            {
                Content = JsonContent.Create(payload)
            };

            registry.ApplyHeaders(request.Headers, User);

            using var response = await _httpClient.SendAsync(request);

            return response.IsSuccessStatusCode;
        }
    }
}
