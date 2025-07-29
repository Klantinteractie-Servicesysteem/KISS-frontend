using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Nodes;
using Kiss.Bff.EndToEndTest.Infrastructure.ApiClients.Dtos.OpenKlant;

namespace Kiss.Bff.EndToEndTest.Infrastructure.ApiClients
{
     public class OpenKlantApiClient : ApiClientBase
    {
        public OpenKlantApiClient(string baseUrl, string token) : base(baseUrl, token)
        {
        }

        public async Task<bool> DeleteActorKlantContact(string actorKlantContactUuid)
        {
            var endpoint = $"actorklantcontacten/{actorKlantContactUuid}";

            var request = CreateRequest(HttpMethod.Delete, endpoint);

            using var response = await HttpClient.SendAsync(request);

            return response.IsSuccessStatusCode;
        }

        public async Task<bool> DeleteKlantContact(string klantContactUuid)
        {
            var endpoint = $"klantcontacten/{klantContactUuid}";

            var request = CreateRequest(HttpMethod.Delete, endpoint);

            using var response = await HttpClient.SendAsync(request);

            return response.IsSuccessStatusCode;
        }

        public async Task<bool> DeleteActor(string actorUuid)
        {
            var endpoint = $"actoren/{actorUuid}";

            var request = CreateRequest(HttpMethod.Delete, endpoint);

            using var response = await HttpClient.SendAsync(request);

            return response.IsSuccessStatusCode;
        }

        public async Task<bool> DeleteInterneTaak(string interneTaakUuid)
        {
            var endpoint = $"internetaken/{interneTaakUuid}";

            var request = CreateRequest(HttpMethod.Delete, endpoint);

            using var response = await HttpClient.SendAsync(request);

            return response.IsSuccessStatusCode;
        }

        public async Task<ActorKlantContactResponse> GetActorKlantContact(string klantContactUuid)
        {
            var endpoint = $"actorklantcontacten?klantcontact__uuid={klantContactUuid}";

            var request = CreateRequest(HttpMethod.Get, endpoint);

            using var response = await HttpClient.SendAsync(request);
            
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();  
                var jsonResponse = JsonNode.Parse(content);

                var results = jsonResponse?["results"]?.AsArray();

                if (results == null || results.Count == 0)
                {
                    throw new Exception($"No actor klant contact found for klant contact UUID: {klantContactUuid}");
                }

                return results[0].Deserialize<ActorKlantContactResponse>();
            } else
            {
                throw new Exception($"Failed to retrieve actor klant contact: {response.Content}");
            }
        }
    }
}
