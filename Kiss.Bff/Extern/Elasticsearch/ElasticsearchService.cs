using System.Runtime.CompilerServices;
using System.Security.Claims;
using System.Text.Json.Nodes;

[assembly: InternalsVisibleTo("Kiss.Bff.Test")]
namespace Kiss.Bff.Extern.Elasticsearch
{
    public class ElasticsearchService(
        HttpClient httpClient,
        ElasticsearchMetadataCache metadataCache,
        IsKennisbank isKennisbank,
        IsKcm isKcm,
        ClaimsPrincipal user,
        IConfiguration configuration)
    {
        private const string SmoelenboekIndex = "search-smoelenboek";

        private readonly string[] _excludedFieldsForKennisbank =
            (configuration["ELASTIC_EXCLUDED_FIELDS_KENNISBANK"] ?? "")
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        public async Task<ElasticResponse?> GlobalSearch(SearchRequest request, CancellationToken cancellationToken)
        {
            var metadata = await metadataCache.GetMetadata(cancellationToken);

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
            var response = await PostSearch<ElasticResponse>(indices, query, cancellationToken);
            NormalizeLegacyBodyField(response);
            return response;
        }

        // Enterprise Search docs use `body_content`; Open Crawler docs use `body`. Rename so
        // the frontend only needs to read one field. Drop once all legacy docs are reindexed.
        private static void NormalizeLegacyBodyField(ElasticResponse? response)
        {
            if (response?.Hits?.Hits is not { } hits) return;
            foreach (var hit in hits)
            {
                if (hit.Source is not JsonObject src) continue;
                if (src.ContainsKey("body") || !src.ContainsKey("body_content")) continue;
                var value = src["body_content"];
                src.Remove("body_content");
                src["body"] = value?.DeepClone();
            }
        }

        public async Task<Source[]> GetSources(CancellationToken cancellationToken)
        {
            var metadata = await metadataCache.GetMetadata(cancellationToken);
            return metadata.Indices
                .Select(i => new Source(i, DisplayNameFor(i)))
                .ToArray();
        }

        private static string DisplayNameFor(string index) =>
            index.StartsWith("search-", StringComparison.Ordinal) ? index["search-".Length..] : index;

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
    }

    public record SearchRequest(string Query, int Page, List<SearchFilter> Filters);
    public record SearchFilter(string Name, string Index);
    public record Source(string Index, string Name);
    public record MedewerkerSearchRequest(string? Search, string? FilterField, string? FilterValue);
}
