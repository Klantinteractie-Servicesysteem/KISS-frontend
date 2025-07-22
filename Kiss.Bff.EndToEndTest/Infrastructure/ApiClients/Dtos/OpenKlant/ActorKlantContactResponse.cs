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
        public string Uuid { get; set; }

        [JsonPropertyName("url")]
        public string Url { get; set; }

        [JsonPropertyName("actor")]
        public ActorResponse Actor { get; set; }

        [JsonPropertyName("klantcontact")]
        public KlantContactResponse KlantContact { get; set; }
    }
}
