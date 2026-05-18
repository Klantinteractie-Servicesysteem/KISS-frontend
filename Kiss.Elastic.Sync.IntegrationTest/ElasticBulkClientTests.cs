
using System.Text.Json;
using Elastic.Clients.Elasticsearch;
using Kiss.Elastic.Sync.IntegrationTest.Infrastructure;
using Testcontainers.Elasticsearch;

namespace Kiss.Elastic.Sync.IntegrationTest
{
    public class ElasticBulkClientTests(ElasticFixture fixture) : IClassFixture<ElasticFixture>
    {
        const string SourceName = "my_index";
        static readonly string s_indexName = Helpers.GenerateValidIndexName(SourceName);

        [Fact]
        public async Task Bulk_insert_works_for_inserts_updates_and_deletes()
        {
            using var bulkClient = new ElasticBulkClient(
                fixture.BaseUri,
                ElasticsearchBuilder.DefaultUsername,
                ElasticsearchBuilder.DefaultPassword,
                1 // page size of 1 so we also test if scrolling works correctly
            );

            var elastic = new ElasticsearchClient(fixture.BaseUri);

            // index some records and assert if we get the same records back from elasticsearch
            await BulkIndexRecordsAndAssertOutput(bulkClient, elastic, new ()
            {
                ["1"] = "first record to be deleted",
                ["2"] = "second record to be updated",
                ["3"] = "third record to remain the same",
            });

            // index a new set of records, with:
            // - an excluded record from the first set
            // - an update
            // - an unchanged record 
            // - a new record
            // and assert if we get that exact same records back from elasticsearch
            await BulkIndexRecordsAndAssertOutput(bulkClient, elastic, new()
            {
                ["2"] = "second record with update",
                ["3"] = "third record to remain the same",
                ["4"] = "fourth record which is new",
            });
        }

        private static async Task BulkIndexRecordsAndAssertOutput(ElasticBulkClient bulkClient, ElasticsearchClient elastic, Dictionary<string, string> expectedRecords)
        {
            var envelopes = expectedRecords
                .Select(Map)
                .AsAsyncEnumerable();

            await bulkClient.IndexBulk(envelopes, SourceName, s_indexName, [], default);

            var refreshResponse = await elastic.Indices.RefreshAsync(s_indexName);
            Assert.True(refreshResponse.IsSuccess());

            var searchResponse = await elastic.SearchAsync<KissEnvelope>(s_indexName);
            Assert.True(searchResponse.IsSuccess());

            var actualRecords = searchResponse.Hits.ToDictionary(x => x.Id!, x => x.Source.Title!);

            Assert.Equal(expectedRecords, actualRecords);
        }

        private static KissEnvelope Map(KeyValuePair<string, string> x) 
            => new(JsonDocument.Parse(JsonSerializer.Serialize(x)).RootElement, x.Value, null, x.Key);
    }
}
