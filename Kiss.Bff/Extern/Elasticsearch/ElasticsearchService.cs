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
            var indices = request.Filters.Count > 0
                ? request.Filters.Select(f => f.Index).Where(knownIndices.Contains).Distinct().OrderBy(x => x).ToArray()
                : metadata.Indices;
            var query = QueryBuilder.BuildGlobalSearchQuery(request, fields, excludes);
            return await PostSearch<ElasticResponse>(indices, query, cancellationToken);
        }

        public async Task<JsonObject?> GetSources(CancellationToken cancellationToken)
        {
            var metadata = await metadataCache.GetMetadata(cancellationToken);
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
    }

    public record SearchRequest(string Query, int Page, List<SearchFilter> Filters);
    public record SearchFilter(string Name, string Index);
    public record MedewerkerSearchRequest(string? Search, string? FilterField, string? FilterValue);
}
