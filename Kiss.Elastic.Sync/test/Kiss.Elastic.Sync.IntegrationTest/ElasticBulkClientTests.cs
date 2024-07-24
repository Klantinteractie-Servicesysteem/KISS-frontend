
using System.Text.Json;
using Elastic.Clients.Elasticsearch;
using Kiss.Elastic.Sync.IntegrationTest.Infrastructure;
using Testcontainers.Elasticsearch;

namespace Kiss.Elastic.Sync.IntegrationTest
{
    public class ElasticBulkClientTests(ElasticFixture fixture) : IClassFixture<ElasticFixture>
    {
        [Fact]
        public async Task Bulk_insert_works_for_inserts_updates_and_deletes()
        {
            const string IndexWithoutPrefix = "my_index";
            const string IndexWithPrefix = $"search-{IndexWithoutPrefix}";

            using var bulkClient = new ElasticBulkClient(
                fixture.BaseUri,
                ElasticsearchBuilder.DefaultUsername,
                ElasticsearchBuilder.DefaultPassword,
                1
            );

            var elastic = new ElasticsearchClient(fixture.BaseUri);

            var expectedInitalRecords = new Dictionary<string, string>
            {
                ["1"] = "first record to be deleted",
                ["2"] = "second record to be updated",
                ["3"] = "third record to remain the same",
            };

            var expectedUpdatedRecords = new Dictionary<string, string>
            {
                ["2"] = "second record with update",
                ["3"] = "third record to remain the same",
                ["4"] = "fourth record which is new",
            };

            var initialEnvelopes = expectedInitalRecords
                .Select(Map)
                .AsAsyncEnumerable();

            var updatedEnvelopes = expectedUpdatedRecords
                .Select(Map)
                .AsAsyncEnumerable();

            // Bulk index the initial values
            await bulkClient.IndexBulk(initialEnvelopes, IndexWithoutPrefix, [], default);

            Assert.True((await elastic.Indices.RefreshAsync(IndexWithPrefix)).IsSuccess());

            var firstSearchResponse = await elastic.SearchAsync<KissEnvelope>(IndexWithPrefix);
            Assert.True(firstSearchResponse.IsSuccess());

            var actualInitialRecords = firstSearchResponse.Hits.ToDictionary(x => x.Id!, x => x.Source.Title!);

            Assert.Equivalent(expectedInitalRecords, actualInitialRecords);


            // Bulk index the updated values
            await bulkClient.IndexBulk(updatedEnvelopes, IndexWithoutPrefix, [], default);

            Assert.True((await elastic.Indices.RefreshAsync(IndexWithPrefix)).IsSuccess());

            var secondSearchResponse = await elastic.SearchAsync<KissEnvelope>(IndexWithPrefix);
            Assert.True(secondSearchResponse.IsSuccess());

            var actualUpdatedRecords = secondSearchResponse.Hits.ToDictionary(x => x.Id!, x => x.Source.Title!);

            Assert.Equal(expectedUpdatedRecords, actualUpdatedRecords);
        }

        private static KissEnvelope Map(KeyValuePair<string, string> x) 
            => new(JsonDocument.Parse(JsonSerializer.Serialize(x)).RootElement, x.Value, null, x.Key);
    }
}
