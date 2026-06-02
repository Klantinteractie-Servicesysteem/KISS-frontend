using System.Text.Json.Serialization;

namespace Kiss.Bff.Extern.Elasticsearch
{
    public class ElasticsearchMetadataCache(HttpClient httpClient, ILogger<ElasticsearchMetadataCache> logger)
    {
        private const string TextType = "text";

        private static readonly Dictionary<string, double> s_boostBySuffix = new()
        {
            { "",           1.00 },
            { ".stem",      0.95 },
            { ".joined",    0.75 },
            { ".delimiter", 0.40 },
            { ".prefix",    0.10 },
        };

        private static readonly HashSet<string> s_excludedSuffixes = [".enum", ".date", ".float", ".location"];

        private Task<Metadata>? _metadataTask;
        private readonly object _lock = new();

        public Task<Metadata> GetMetadata(CancellationToken cancellationToken)
        {
            lock (_lock)
            {
                if (_metadataTask == null || _metadataTask.IsFaulted)
                    _metadataTask = FetchMetadata(cancellationToken);
                return _metadataTask;
            }
        }

        private async Task<Metadata> FetchMetadata(CancellationToken cancellationToken)
        {
            var response = await httpClient.GetFromJsonAsync<FieldCapsResponse>(
                "search-*/_field_caps?fields=*", cancellationToken)
                ?? throw new InvalidOperationException("Empty field caps response");

            var indices = response.Indices
                .OrderBy(i => i)
                .ToArray();

            var fields = response.Fields
                .Where(kv => IsSearchableTextField(kv.Key, kv.Value))
                .Select(kv => $"{kv.Key}^{BoostFor(kv.Key)}")
                .ToArray();

            logger.LogInformation("Discovered {IndexCount} search indices, {FieldCount} searchable fields",
                indices.Length, fields.Length);

            return new Metadata(indices, fields);
        }

        private static bool IsSearchableTextField(string fieldName, Dictionary<string, FieldType> types)
        {
            if (fieldName.StartsWith('_')) return false;
            if (s_excludedSuffixes.Any(s => fieldName.EndsWith(s, StringComparison.OrdinalIgnoreCase))) return false;
            if (!types.ContainsKey(TextType)) return false;

            return true;
        }

        private static double BoostFor(string fieldName) =>
            s_boostBySuffix
                .Where(kv => kv.Key.Length == 0 || fieldName.EndsWith(kv.Key, StringComparison.OrdinalIgnoreCase))
                .OrderByDescending(kv => kv.Key.Length)
                .Select(kv => kv.Value)
                .FirstOrDefault(1.0);

        public record Metadata(string[] Indices, string[] Fields);

        private record FieldCapsResponse(
            [property: JsonPropertyName("indices")] string[] Indices,
            [property: JsonPropertyName("fields")] Dictionary<string, Dictionary<string, FieldType>> Fields);

        private record FieldType(
            [property: JsonPropertyName("indices")] string[]? Indices);
    }
}
