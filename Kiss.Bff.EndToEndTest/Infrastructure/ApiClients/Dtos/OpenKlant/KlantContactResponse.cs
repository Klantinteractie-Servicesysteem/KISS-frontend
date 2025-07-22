using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Kiss.Bff.EndToEndTest.Infrastructure.ApiClients.OpenKlant.Dtos
{
    public class KlantContactResponse
    {
        [JsonPropertyName("uuid")]
        public string Uuid { get; set; }
    }
}
