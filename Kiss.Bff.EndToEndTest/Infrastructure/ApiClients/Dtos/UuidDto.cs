using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Kiss.Bff.EndToEndTest.Infrastructure.ApiClients.Dtos
{
    internal class UuidDto
    {
        [JsonPropertyName("uuid")]
        public required string Value { get; set; }
    }
}
