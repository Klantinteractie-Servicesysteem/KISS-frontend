using System.Runtime.CompilerServices;
using System.Security.Claims;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Caching.Memory;

[assembly: InternalsVisibleTo("Kiss.Bff.Test")]
namespace Kiss.Bff.Extern.Elasticsearch
{
    public class ElasticsearchService(
        HttpClient httpClient,
        IMemoryCache memoryCache,
        IsKennisbank isKennisbank,
        IsKcm isKcm,
        ClaimsPrincipal user,
        IConfiguration configuration)
    {
        private const string SmoelenboekIndex = "search-smoelenboek";
        private const string MetadataCacheKey = "elasticsearch_metadata";
        private static readonly TimeSpan s_metadataCacheExpiry = TimeSpan.FromHours(1);

        private static readonly Dictionary<string, double> s_boostBySuffix = new()
        {
            { "",           1.00 },
            { ".stem",      0.95 },
            { ".joined",    0.75 },
            { ".delimiter", 0.40 },
            { ".prefix",    0.10 },
        };

        private static readonly HashSet<string> s_excludedSuffixes =
            [".enum", ".date", ".float", ".location"];

        private readonly string[] _excludedFieldsForKennisbank =
            (configuration["ELASTIC_EXCLUDED_FIELDS_KENNISBANK"] ?? "")
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        private async Task<Metadata> GetMetadata(CancellationToken cancellationToken)
        {
            if (memoryCache.TryGetValue(MetadataCacheKey, out Metadata? cached) && cached != null)
                return cached;

            var response = await httpClient.GetFromJsonAsync<FieldCapsResponse>(
                "search-*/_field_caps?fields=*", cancellationToken)
                ?? throw new InvalidOperationException("Empty field caps response");

            var indices = response.Indices.OrderBy(i => i).ToArray();
            var fields = response.Fields
                .Where(kv => IsSearchableTextField(kv.Key, kv.Value))
                .Select(kv => $"{kv.Key}^{BoostFor(kv.Key)}")
                .ToArray();

            var metadata = new Metadata(indices, fields);
            memoryCache.Set(MetadataCacheKey, metadata, s_metadataCacheExpiry);
            return metadata;
        }

        private static bool IsSearchableTextField(string fieldName, Dictionary<string, object> types)
        {
            if (fieldName.StartsWith('_')) return false;
            if (s_excludedSuffixes.Any(s => fieldName.EndsWith(s, StringComparison.OrdinalIgnoreCase))) return false;
            if (!types.ContainsKey("text")) return false;
            return true;
        }

        private static double BoostFor(string fieldName) =>
            s_boostBySuffix
                .Where(kv => kv.Key.Length == 0 || fieldName.EndsWith(kv.Key, StringComparison.OrdinalIgnoreCase))
                .OrderByDescending(kv => kv.Key.Length)
                .Select(kv => kv.Value)
                .FirstOrDefault(1.0);

        public async Task<ElasticResponse?> GlobalSearch(SearchRequest request, CancellationToken cancellationToken)
        {
            var metadata = await GetMetadata(cancellationToken);

            var excludes = GetExcludedFieldsForUser();
            var fields = metadata.Fields
                .Where(f => !excludes.Any(ex => f.StartsWith(ex, StringComparison.OrdinalIgnoreCase)))
                .ToArray();
            // Constrain to known indices: filter.Index comes from the browser and is
            // concatenated into the ES URL, so without this an authenticated user could
            // probe arbitrary indices.
            var knownIndices = metadata.Indices.ToHashSet(StringComparer.Ordinal);
            var indices = request.Filters is { Count: > 0 } filters
                ? filters.Select(f => f.Index).Where(knownIndices.Contains).Distinct().OrderBy(x => x).ToArray()
                : metadata.Indices;
            var query = QueryBuilder.BuildGlobalSearchQuery(request, fields, excludes);
            return await PostSearch<ElasticResponse>(indices, query, cancellationToken);
        }

        public async Task<JsonObject?> GetSources(CancellationToken cancellationToken)
        {
            var metadata = await GetMetadata(cancellationToken);
            if (metadata.Indices.Length == 0) return null;

            return await PostSearch<JsonObject>(metadata.Indices, QueryBuilder.BronnenAggregation, cancellationToken);
        }

        public async Task<JsonObject?> SearchMedewerkers(MedewerkerSearchRequest request, CancellationToken cancellationToken)
        {
            var query = QueryBuilder.BuildMedewerkerQuery(request);
            return await PostSearch<JsonObject>([SmoelenboekIndex], query, cancellationToken);
        }

        private async Task<T?> PostSearch<T>(string[] indices, object query, CancellationToken cancellationToken)
        {
            var url = $"{string.Join(",", indices)}/_search";
            var response = await httpClient.PostAsJsonAsync(url, query, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                var errorBody = await response.Content.ReadAsStringAsync(cancellationToken);
                throw new HttpRequestException($"Elasticsearch request failed: {errorBody}", null, response.StatusCode);
            }

            return await response.Content.ReadFromJsonAsync<T>(cancellationToken);
        }

        private string[] GetExcludedFieldsForUser() =>
            IsOnlyKennisbank() ? _excludedFieldsForKennisbank : [];

        private bool IsOnlyKennisbank() => isKennisbank(user) && !isKcm(user);

        private record Metadata(string[] Indices, string[] Fields);

        private record FieldCapsResponse(
            [property: JsonPropertyName("indices")] string[] Indices,
            [property: JsonPropertyName("fields")] Dictionary<string, Dictionary<string, object>> Fields);
    }

    public record SearchRequest(string Query, int Page, List<SearchFilter> Filters);
    public record SearchFilter(string Name, string Index);
    public record MedewerkerSearchRequest(string? Search, string? FilterField, string? FilterValue);
}
