using System.Text.Json;
using System.Text.Json.Serialization;

namespace Kiss.Bff.Extern.ElasticSearch
{
    public class ElasticResponse
    {
        [JsonPropertyName("hits")]
        public HitsWrapper? Hits { get; set; }
        [JsonExtensionData]
        public Dictionary<string, JsonElement>? ResponseExtensionData { get; set; }
    }

    public class HitsWrapper
    {

        [JsonPropertyName("hits")]
        public List<Hit>? Hits { get; set; }
        [JsonExtensionData]
        public Dictionary<string, JsonElement>? HitsWrapperExtensionData { get; set; }
    }

    public class Hit
    {
        [JsonPropertyName("_source")]
        public SourceData? Source { get; set; }
        [JsonExtensionData]
        public Dictionary<string, JsonElement>? HitsExtensionData { get; set; }
    }

    public class SourceData
    {
        [JsonPropertyName("object_bron")]
        public string? ObjectBron { get; set; }

        [JsonExtensionData]
        public Dictionary<string, JsonElement>? SourceExtensionData { get; set; }
    }
}
