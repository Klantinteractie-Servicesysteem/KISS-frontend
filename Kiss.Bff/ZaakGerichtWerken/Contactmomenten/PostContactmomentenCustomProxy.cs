using System.Net.Http.Headers;
using System.Text.Json.Nodes;
using Microsoft.AspNetCore.Mvc;

namespace Kiss.Bff.ZaakGerichtWerken.Contactmomenten
{
    [Route("api/postcontactmomenten")]
    [ApiController]
    public class PostContactmomentenCustomProxy : ControllerBase
    {
        private const int IdentificatieMaxLength = 24;
        private readonly HttpClient _defaultClient;
        private readonly ZgwTokenProvider _tokenProvider;
        private readonly string _destination;

        public PostContactmomentenCustomProxy(IConfiguration configuration, IHttpClientFactory factory)
        {
            _destination = configuration["CONTACTMOMENTEN_BASE_URL"];
            var clientId = configuration["CONTACTMOMENTEN_API_CLIENT_ID"];
            var apiKey = configuration["CONTACTMOMENTEN_API_KEY"];

            _defaultClient = factory.CreateClient("default");
            _tokenProvider = new ZgwTokenProvider(apiKey, clientId);
        }

        [HttpPost]
        public async Task Post([FromBody] JsonObject parsedModel, CancellationToken token)
        {
            var email = Request.HttpContext.User?.GetEmail();
            var userRepresentation = Request.HttpContext.User?.Identity?.Name;
            // identificatie mag maximaal 24 tekens zijn
            var identificatie = email != null && email.Length > IdentificatieMaxLength
                ? email[..IdentificatieMaxLength]
                : email;
            if (parsedModel != null)
            {
                //claims zijn niet standaard. configuratie mogelijkheid vereist voor juiste vulling 
                parsedModel["medewerkerIdentificatie"] = new JsonObject
                {
                    ["achternaam"] = userRepresentation,
                    ["identificatie"] = identificatie,
                    ["voorletters"] = "",
                    ["voorvoegselAchternaam"] = "",

                };
            }

            var accessToken = _tokenProvider.GenerateToken(email, userRepresentation);

            var url = _destination.TrimEnd('/') + "/contactmomenten/api/v1/contactmomenten";

            _defaultClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            using var response = await _defaultClient.PostAsJsonAsync(url, parsedModel, cancellationToken: token);

            Response.StatusCode = (int)response.StatusCode;

            foreach (var item in response.Headers)
            {
                Response.Headers[item.Key] = new(item.Value.ToArray());
            }

            foreach (var item in response.Content.Headers)
            {
                Response.Headers[item.Key] = new(item.Value.ToArray());
            }

            await using var stream = await response.Content.ReadAsStreamAsync(token);
            await stream.CopyToAsync(Response.Body, token);
        }
    }
}
