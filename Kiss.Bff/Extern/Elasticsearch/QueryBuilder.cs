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

        public static object BuildGlobalSearchQuery(SearchRequest request, string[] fields, string[] excludedSourceFields)
        {
            var page = Math.Max(1, request.Page);

            var multiMatch = new
            {
                multi_match = new
                {
                    query = request.Query,
                    minimum_should_match = "2<75%",
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

            return new
            {
                from = (page - 1) * PageSize,
                size = PageSize,
                _source = new { excludes = excludedSourceFields },
                indices_boost = new[] { new Dictionary<string, int> { ["*"] = 10 } },
                query = baseQuery,
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
                // TODO(PC-2385 follow-up): FilterField comes from the request body and is
                // interpolated directly into the ES field name. An authenticated user can pass
                // any field name and probe the smoelenboek index. Constrain to an allowlist of
                // fields the frontend actually uses (e.g. afdelingen, groepen).
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

    }
}
