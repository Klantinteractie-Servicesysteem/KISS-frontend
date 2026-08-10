using System.Text.Json;
using System.Text.Json.Serialization;

namespace Kiss.Bff.Extern.ZaakGerichtWerken.Zaaksysteem
{
    /// <summary>
    /// Gepagineerd antwoord van het zaaksysteem.
    /// </summary>
    public class ZakenPaginatedResponse
    {
        [JsonPropertyName("count")]
        public int? Count { get; set; }

        [JsonPropertyName("next")]
        public string? Next { get; set; }

        [JsonPropertyName("previous")]
        public string? Previous { get; set; }

        [JsonPropertyName("results")]
        public List<ZaakResource> Results { get; set; } = new();

        [JsonExtensionData]
        public Dictionary<string, JsonElement>? AdditionalProperties { get; set; }
    }

    /// <summary>
    /// Een zaak uit het zaaksysteem. Alleen de velden die we nodig hebben zijn gemodelleerd;
    /// de rest wordt transparant doorgegeven via JsonExtensionData.
    /// </summary>
    public class ZaakResource
    {
        [JsonPropertyName("zaaktype")]
        public string? Zaaktype { get; set; }

        [JsonPropertyName("_zaaktype")]
        public ZaaktypeResource? ZaaktypeDetails { get; set; }

        [JsonExtensionData]
        public Dictionary<string, JsonElement>? AdditionalProperties { get; set; }
    }

    /// <summary>
    /// Zaaktype uit de catalogi API. Alleen de velden die we nodig hebben;
    /// de rest gaat transparant mee naar de frontend.
    /// </summary>
    public class ZaaktypeResource
    {
        [JsonPropertyName("omschrijving")]
        public string? Omschrijving { get; set; }

        [JsonExtensionData]
        public Dictionary<string, JsonElement>? AdditionalProperties { get; set; }
    }
}
