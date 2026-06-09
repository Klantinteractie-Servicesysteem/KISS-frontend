using System.Net.Http.Json;
using System.Text.Json;
using Kiss.Bff.Extern.Elasticsearch;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
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

            var service = new ElasticsearchService(
                httpClient,
                new MemoryCache(new MemoryCacheOptions()),
                _ => isKennisbank,
                _ => isKcm,
                null!,
                configuration.Object);

            return (service, http);
        }

        private static void RespondToFieldCaps(MockHttpMessageHandler http, params string[] indices) =>
            RespondToFieldCaps(http, indices, new[] { "title", "body_content" });

        private static void RespondToFieldCaps(MockHttpMessageHandler http, string[] indices, string[] fieldNames)
        {
            var fields = fieldNames.ToDictionary(f => f, _ => new Dictionary<string, object> { ["text"] = new { } });
            http.When(HttpMethod.Get, "https://elasticsearch.example.com/search-*/_field_caps*")
                .Respond("application/json", JsonSerializer.Serialize(new { indices, fields }));
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
        public async Task GetSources_CallsElasticsearch()
        {
            var (service, http) = BuildService();
            RespondToFieldCaps(http, "search-kennisbank");

            var called = false;
            http.When(HttpMethod.Post, "https://elasticsearch.example.com/*/_search")
                .With(_ => { called = true; return true; })
                .Respond("application/json", JsonSerializer.Serialize(new
                {
                    aggregations = new
                    {
                        bronnen = new { buckets = Array.Empty<object>() },
                        domains = new { buckets = Array.Empty<object>() },
                    }
                }));

            var result = await service.GetSources(CancellationToken.None);

            Assert.IsTrue(called);
            Assert.IsNotNull(result);
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

        [TestMethod]
        public async Task GlobalSearch_KennisbankOnlyUser_ExcludesConfiguredFieldsFromQueryAndSource()
        {
            var (service, http) = BuildService(
                isKennisbank: true,
                isKcm: false,
                excludedFields: "VAC.toelichting,Kennisbank.vertalingen.deskMemo");

            RespondToFieldCaps(http,
                ["search-kennisbank", "search-vac"],
                ["title", "VAC.toelichting", "VAC.toelichting.stem", "Kennisbank.vertalingen.deskMemo", "Kennisbank.naam"]);

            string? capturedBody = null;
            http.When(HttpMethod.Post, "https://elasticsearch.example.com/*/_search")
                .With(req =>
                {
                    capturedBody = req.Content!.ReadAsStringAsync().GetAwaiter().GetResult();
                    return true;
                })
                .Respond("application/json", JsonSerializer.Serialize(new
                {
                    hits = new { total = new { value = 0 }, hits = Array.Empty<object>() }
                }));

            await service.GlobalSearch(new SearchRequest("test", 1, []), CancellationToken.None);

            Assert.IsNotNull(capturedBody);
            using var doc = JsonDocument.Parse(capturedBody);

            // Excluded fields must not appear in multi_match fields
            var fields = doc.RootElement
                .GetProperty("query").GetProperty("bool").GetProperty("must").GetProperty("bool")
                .GetProperty("must")[0].GetProperty("bool").GetProperty("should")[0]
                .GetProperty("multi_match").GetProperty("fields");
            var fieldList = fields.EnumerateArray().Select(f => f.GetString()!).ToList();
            Assert.IsFalse(fieldList.Any(f => f.StartsWith("VAC.toelichting")), "VAC.toelichting should be excluded from multi_match fields");
            Assert.IsFalse(fieldList.Any(f => f.StartsWith("Kennisbank.vertalingen.deskMemo")), "Kennisbank.vertalingen.deskMemo should be excluded from multi_match fields");
            Assert.IsTrue(fieldList.Any(f => f.StartsWith("title")), "title should remain in multi_match fields");
            Assert.IsTrue(fieldList.Any(f => f.StartsWith("Kennisbank.naam")), "Kennisbank.naam should remain in multi_match fields");

            // Excluded fields must appear in _source.excludes
            var excludes = doc.RootElement.GetProperty("_source").GetProperty("excludes");
            var excludeList = excludes.EnumerateArray().Select(f => f.GetString()!).ToList();
            CollectionAssert.Contains(excludeList, "VAC.toelichting");
            CollectionAssert.Contains(excludeList, "Kennisbank.vertalingen.deskMemo");
        }

        [TestMethod]
        public async Task GlobalSearch_KcmUser_DoesNotExcludeFields()
        {
            var (service, http) = BuildService(
                isKennisbank: false,
                isKcm: true,
                excludedFields: "VAC.toelichting,Kennisbank.vertalingen.deskMemo");

            RespondToFieldCaps(http,
                ["search-kennisbank", "search-vac"],
                ["title", "VAC.toelichting", "Kennisbank.vertalingen.deskMemo"]);

            string? capturedBody = null;
            http.When(HttpMethod.Post, "https://elasticsearch.example.com/*/_search")
                .With(req =>
                {
                    capturedBody = req.Content!.ReadAsStringAsync().GetAwaiter().GetResult();
                    return true;
                })
                .Respond("application/json", JsonSerializer.Serialize(new
                {
                    hits = new { total = new { value = 0 }, hits = Array.Empty<object>() }
                }));

            await service.GlobalSearch(new SearchRequest("test", 1, []), CancellationToken.None);

            Assert.IsNotNull(capturedBody);
            using var doc = JsonDocument.Parse(capturedBody);

            var fields = doc.RootElement
                .GetProperty("query").GetProperty("bool").GetProperty("must").GetProperty("bool")
                .GetProperty("must")[0].GetProperty("bool").GetProperty("should")[0]
                .GetProperty("multi_match").GetProperty("fields");
            var fieldList = fields.EnumerateArray().Select(f => f.GetString()!).ToList();
            Assert.IsTrue(fieldList.Any(f => f.StartsWith("VAC.toelichting")), "VAC.toelichting should NOT be excluded for KCM users");
            Assert.IsTrue(fieldList.Any(f => f.StartsWith("Kennisbank.vertalingen.deskMemo")), "Kennisbank.vertalingen.deskMemo should NOT be excluded for KCM users");

            var excludes = doc.RootElement.GetProperty("_source").GetProperty("excludes");
            Assert.AreEqual(0, excludes.GetArrayLength(), "_source.excludes should be empty for KCM users");
        }

        [TestMethod]
        public async Task GlobalSearch_UserWithBothRoles_DoesNotExcludeFields()
        {
            var (service, http) = BuildService(
                isKennisbank: true,
                isKcm: true,
                excludedFields: "VAC.toelichting");

            RespondToFieldCaps(http,
                ["search-kennisbank"],
                ["title", "VAC.toelichting"]);

            string? capturedBody = null;
            http.When(HttpMethod.Post, "https://elasticsearch.example.com/*/_search")
                .With(req =>
                {
                    capturedBody = req.Content!.ReadAsStringAsync().GetAwaiter().GetResult();
                    return true;
                })
                .Respond("application/json", JsonSerializer.Serialize(new
                {
                    hits = new { total = new { value = 0 }, hits = Array.Empty<object>() }
                }));

            await service.GlobalSearch(new SearchRequest("test", 1, []), CancellationToken.None);

            Assert.IsNotNull(capturedBody);
            using var doc = JsonDocument.Parse(capturedBody);

            var fields = doc.RootElement
                .GetProperty("query").GetProperty("bool").GetProperty("must").GetProperty("bool")
                .GetProperty("must")[0].GetProperty("bool").GetProperty("should")[0]
                .GetProperty("multi_match").GetProperty("fields");
            var fieldList = fields.EnumerateArray().Select(f => f.GetString()!).ToList();
            Assert.IsTrue(fieldList.Any(f => f.StartsWith("VAC.toelichting")), "VAC.toelichting should NOT be excluded when user has both roles");

            var excludes = doc.RootElement.GetProperty("_source").GetProperty("excludes");
            Assert.AreEqual(0, excludes.GetArrayLength(), "_source.excludes should be empty when user has both roles");
        }
    }
}
