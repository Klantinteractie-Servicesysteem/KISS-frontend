using System.Net.Http.Json;
using System.Text.Json;
using Kiss.Bff.Extern.Elasticsearch;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using RichardSzalay.MockHttp;

namespace Kiss.Bff.Test
{
    [TestClass]
    public class ElasticsearchControllerTests
    {
        private static (ElasticsearchService service, MockHttpMessageHandler http) BuildService(
            bool isKennisbank = false,
            bool isKcm = true,
            string excludedFields = "")
        {
            var configuration = new Mock<IConfiguration>();
            configuration.Setup(c => c["ELASTIC_EXCLUDED_FIELDS_KENNISBANK"]).Returns(excludedFields);

            var http = new MockHttpMessageHandler();
            var httpClient = http.ToHttpClient();
            httpClient.BaseAddress = new Uri("https://elasticsearch.example.com");

            var factory = new Mock<IHttpClientFactory>();
            factory.Setup(f => f.CreateClient(It.IsAny<string>())).Returns(() =>
            {
                var c = http.ToHttpClient();
                c.BaseAddress = new Uri("https://elasticsearch.example.com");
                return c;
            });
            var cache = new ElasticsearchMetadataCache(factory.Object, NullLogger<ElasticsearchMetadataCache>.Instance);

            var service = new ElasticsearchService(
                httpClient,
                cache,
                _ => isKennisbank,
                _ => isKcm,
                null!,
                configuration.Object);

            return (service, http);
        }

        private static void RespondToFieldCaps(MockHttpMessageHandler http, params string[] indices)
        {
            http.When(HttpMethod.Get, "https://elasticsearch.example.com/search-*/_field_caps*")
                .Respond("application/json", JsonSerializer.Serialize(new
                {
                    indices,
                    fields = new Dictionary<string, Dictionary<string, object>>
                    {
                        ["title"] = new() { ["text"] = new { } },
                        ["body_content"] = new() { ["text"] = new { } },
                    }
                }));
        }

        [TestMethod]
        public async Task GlobalSearch_CallsElasticsearch()
        {
            var (service, http) = BuildService();
            RespondToFieldCaps(http, "search-kennisbank", "search-vac");

            var called = false;
            http.When(HttpMethod.Post, "https://elasticsearch.example.com/*/_search")
                .With(_ => { called = true; return true; })
                .Respond("application/json", JsonSerializer.Serialize(new
                {
                    hits = new { total = new { value = 0 }, hits = Array.Empty<object>() }
                }));

            var result = await service.GlobalSearch(new SearchRequest("hello", 1, []), CancellationToken.None);

            Assert.IsTrue(called);
            Assert.IsNotNull(result);
        }

        [TestMethod]
        public async Task GetSources_ReturnsDiscoveredIndices()
        {
            var (service, http) = BuildService();
            RespondToFieldCaps(http, "search-kennisbank", "search-website");

            var result = await service.GetSources(CancellationToken.None);

            Assert.IsNotNull(result);
            CollectionAssert.AreEquivalent(
                new[] { "search-kennisbank", "search-website" },
                result.Select(s => s.Index).ToArray());
            CollectionAssert.AreEquivalent(
                new[] { "kennisbank", "website" },
                result.Select(s => s.Name).ToArray());
        }

        [TestMethod]
        public async Task SearchMedewerkers_CallsSmoelenboekIndex()
        {
            var (service, http) = BuildService();

            var called = false;
            http.When(HttpMethod.Post, "https://elasticsearch.example.com/search-smoelenboek/_search")
                .With(_ => { called = true; return true; })
                .Respond("application/json", JsonSerializer.Serialize(new
                {
                    hits = new { hits = Array.Empty<object>() }
                }));

            var result = await service.SearchMedewerkers(
                new MedewerkerSearchRequest("jan", null, null),
                CancellationToken.None);

            Assert.IsTrue(called);
            Assert.IsNotNull(result);
        }
    }
}
