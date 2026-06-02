namespace Kiss.Bff.Extern.Elasticsearch
{
    /// <summary>
    /// Builds Elasticsearch query DSL using anonymous objects.
    /// Anonymous objects serialize to JSON exactly as written, with camelCase property names
    /// preserved (System.Text.Json default), making the shape easy to read against ES docs.
    /// </summary>
    internal static class QueryBuilder
    {
        private const int PageSize = 10;

        public static object BronnenAggregation { get; } = new
        {
            size = 0,
            aggs = new
            {
                bronnen = new
                {
                    terms = new { field = "object_bron.enum" },
                    aggs = new { by_index = new { terms = new { field = "_index" } } }
                },
                domains = new
                {
                    terms = new { field = "domains.enum" },
                    aggs = new { by_index = new { terms = new { field = "_index" } } }
                },
            }
        };

        public static object BuildGlobalSearchQuery(SearchRequest request, string[] fields, string[] excludedSourceFields)
        {
            var page = Math.Max(1, request.Page);

            var multiMatch = new
            {
                multi_match = new
                {
                    query = request.Query,
                    minimum_should_match = "100%",
                    type = "best_fields",
                    fields,
                }
            };

            // matches the original App Search template shape:
            //   bool.must.bool.must.[bool.should.[multi_match]]
            var baseQuery = new
            {
                @bool = new
                {
                    must = new
                    {
                        @bool = new
                        {
                            must = new[]
                            {
                                new
                                {
                                    @bool = new { should = new[] { multiMatch } }
                                }
                            }
                        }
                    }
                }
            };

            var filterConditions = BuildFilterConditions(request.Filters);

            object query = filterConditions.Count == 0
                ? baseQuery
                : new
                {
                    @bool = new
                    {
                        must = new object[] { baseQuery },
                        filter = new object[] { new { @bool = new { should = filterConditions } } }
                    }
                };

            return new
            {
                from = (page - 1) * PageSize,
                size = PageSize,
                _source = new { excludes = excludedSourceFields },
                indices_boost = new[] { new Dictionary<string, int> { ["*"] = 10 } },
                suggest = new
                {
                    suggestions = new
                    {
                        prefix = request.Query,
                        completion = new
                        {
                            field = "_completion",
                            skip_duplicates = true,
                            fuzzy = new { },
                        }
                    }
                },
                query,
            };
        }

        public static object BuildMedewerkerQuery(MedewerkerSearchRequest request)
        {
            var mustClauses = new List<object>();

            if (!string.IsNullOrWhiteSpace(request.Search))
                mustClauses.Add(new
                {
                    simple_query_string = new { query = request.Search, default_operator = "and" }
                });

            if (!string.IsNullOrWhiteSpace(request.FilterField) && !string.IsNullOrWhiteSpace(request.FilterValue))
                mustClauses.Add(new Dictionary<string, object>
                {
                    ["match"] = new Dictionary<string, string> { [$"{request.FilterField}.enum"] = request.FilterValue }
                });

            return new
            {
                from = 0,
                size = 30,
                sort = new[]
                {
                    new Dictionary<string, object> { ["Smoelenboek.achternaam.enum"] = new { order = "asc" } }
                },
                query = new { @bool = new { must = mustClauses } }
            };
        }

        private static List<object> BuildFilterConditions(IReadOnlyList<SearchFilter>? filters)
        {
            var conditions = new List<object>();
            if (filters == null || filters.Count == 0) return conditions;

            var domains = filters.Where(f => f.Name.StartsWith("http")).Select(f => f.Name).ToList();
            var bronnen = filters.Where(f => !f.Name.StartsWith("http")).Select(f => f.Name).ToList();

            if (domains.Count > 0)
                conditions.Add(new { terms = new Dictionary<string, List<string>> { ["domains.enum"] = domains } });
            if (bronnen.Count > 0)
                conditions.Add(new { terms = new Dictionary<string, List<string>> { ["object_bron.enum"] = bronnen } });

            return conditions;
        }
    }
}
