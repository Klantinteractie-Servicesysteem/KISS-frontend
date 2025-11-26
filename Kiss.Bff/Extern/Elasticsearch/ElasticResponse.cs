using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace Kiss.Bff.Extern.ElasticSearch
{
    /// <summary>
    /// Elasticsearch response object containing only the essential structure
    /// for accessing hits and their _source properties
    /// </summary>
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
        public JsonObject? Source { get; set; }
        [JsonExtensionData]
        public Dictionary<string, JsonElement>? HitsExtensionData { get; set; }
    }
}
