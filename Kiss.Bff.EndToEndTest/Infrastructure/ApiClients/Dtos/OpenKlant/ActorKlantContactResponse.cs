using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Kiss.Bff.EndToEndTest.Infrastructure.ApiClients.OpenKlant.Dtos;

namespace Kiss.Bff.EndToEndTest.Infrastructure.ApiClients.Dtos.OpenKlant
{
    public class ActorKlantContactResponse
    {
        [JsonPropertyName("uuid")]
        public required string Uuid { get; set; }

        [JsonPropertyName("url")]
        public required string Url { get; set; }

        [JsonPropertyName("actor")]
        public required ActorResponse Actor { get; set; }

        [JsonPropertyName("klantcontact")]
        public required KlantContactResponse KlantContact { get; set; }
    }
}
