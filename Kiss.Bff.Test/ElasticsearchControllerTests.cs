using System.Net;
using System.Security.Claims;
using System.Text.Json;
using System.Text.Json.Nodes;
using Kiss.Bff.Extern.Elasticsearch;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using RichardSzalay.MockHttp;

namespace Kiss.Bff.Test
{
    [TestClass]
    public class ElasticsearchControllerTests
    {
        private Mock<IConfiguration> _configurationMock = null!;
        private Mock<ILogger<ElasticsearchController>> _loggerMock = null!;
        private MockHttpMessageHandler _mockHttp = null!;
        private ElasticsearchController _controller = null!;
        private DefaultHttpContext _httpContext = null!;
        private IsKennisbank _isKennisbank = null!;
        private IsKcm _isKcm = null!;
        private IsRedacteur _isRedacteur = null!;

        [TestInitialize]
        public void Setup()
        {
            _configurationMock = new Mock<IConfiguration>();
            _loggerMock = new Mock<ILogger<ElasticsearchController>>();
            _mockHttp = new MockHttpMessageHandler();
            _isKennisbank = (user) => user?.IsInRole("Kennisbank") ?? false;
            _isKcm = (user) => user?.IsInRole("Kcm") ?? false;
            _isRedacteur = (user) => user?.IsInRole("Redacteur") ?? false;

            // Setup default configuration
            _configurationMock.Setup(c => c["ELASTIC_BASE_URL"]).Returns("https://elasticsearch.example.com");
            _configurationMock.Setup(c => c["ELASTIC_USERNAME"]).Returns("testuser");
            _configurationMock.Setup(c => c["ELASTIC_PASSWORD"]).Returns("testpass");
            _configurationMock.Setup(c => c["ELASTIC_EXCLUDED_FIELDS_KENNISBANK"]).Returns("VAC.toelichting,Kennisbank.vertalingen.deskMemo");

            _httpContext = new DefaultHttpContext
            {
                User = new ClaimsPrincipal(new ClaimsIdentity(
                [
                    new Claim(ClaimTypes.Email, "test@example.com"),
                    new Claim(ClaimTypes.Role, "Kennisbank")
                ], "TestAuth"))
            };

            CreateController();
        }

        private void CreateController()
        {
            var httpClient = _mockHttp.ToHttpClient();
            httpClient.BaseAddress = new Uri("https://elasticsearch.example.com");

            var elasticsearchService = new ElasticsearchService(
                httpClient,
                _isKennisbank,
                _isRedacteur,
                _isKcm,
                _httpContext.User,
                _configurationMock.Object
            );

            _controller = new ElasticsearchController(
                elasticsearchService,
                _loggerMock.Object
            );
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = _httpContext
            };
        }

        #region Request Transformation Tests (Kennisbank Users)

        [TestMethod]
        public async Task Search_KennisbankUser_RemovesExcludedFieldsFromRequest()
        {
            var elasticQuery = new JsonObject
            {
                ["query"] = new JsonObject
                {
                    ["multi_match"] = new JsonObject
                    {
                        ["query"] = "test",
                        ["fields"] = new JsonArray(
                            "VAC.toelichting^1.0",
                            "VAC.toelichting.stem^0.95",
                            "VAC.status^1.0",
                            "Kennisbank.vertalingen.deskMemo^1.0",
                            "Kennisbank.vertalingen.deskMemo.stem^0.95",
                            "Kennisbank.naam^1.0",
                            "title^1.0"
                        )
                    }
                }
            };

            var capturedRequest = "";
            _mockHttp.When(HttpMethod.Post, "https://elasticsearch.example.com/test-index/_search")
                .With(req =>
                {
                    capturedRequest = req.Content!.ReadAsStringAsync().Result;
                    return true;
                })
                .Respond("application/json", JsonSerializer.Serialize(new
                {
                    hits = new { hits = Array.Empty<object>() }
                }));

            await _controller.Search("test-index", elasticQuery, CancellationToken.None);

            var parsedRequest = JsonNode.Parse(capturedRequest);
            Assert.IsNotNull(parsedRequest);
            var fields = parsedRequest["query"]?["multi_match"]?["fields"]?.AsArray();
            Assert.IsNotNull(fields);

            // Should only have 3 fields remaining (removed VAC.toelichting and Kennisbank.vertalingen.deskMemo)
            Assert.AreEqual(3, fields.Count);
            Assert.IsFalse(fields.Any(x => x?.ToString().StartsWith("VAC.toelichting") ?? false));
            Assert.IsFalse(fields.Any(x => x?.ToString().StartsWith("Kennisbank.vertalingen.deskMemo") ?? false));
            Assert.IsTrue(fields.Any(x => x?.ToString() == "VAC.status^1.0"));
            Assert.IsTrue(fields.Any(x => x?.ToString() == "Kennisbank.naam^1.0"));
            Assert.IsTrue(fields.Any(x => x?.ToString() == "title^1.0"));
        }

        #endregion

        #region Response Transformation Tests (Kennisbank Users)

        [TestMethod]
        public async Task Search_KennisbankUser_RemovesExcludedFieldsFromResponse()
        {
            var elasticQuery = new JsonObject
            {
                ["query"] = new JsonObject { ["match_all"] = new JsonObject() }
            };

            var elasticResponse = "{\"hits\":{\"hits\":[{\"_source\":{\"title\":\"First item\",\"object_meta\":null,\"object_bron\":\"VAC\",\"VAC\":{\"allowedField\":\"This should remain\",\"status\":\"actief\",\"toelichting\":\"This has to be removed\"}}},{\"_source\":{\"title\":\"Second item\",\"object_bron\":\"Kennisbank\",\"Kennisbank\":{\"existingField\":\"This has to stay\",\"vertalingen\":{\"deskMemo\":\"This has to be removed\"}}}}]}}";

            _mockHttp.When(HttpMethod.Post, "https://elasticsearch.example.com/test-index/_search")
                .Respond("application/json", elasticResponse);

            var result = await _controller.Search("test-index", elasticQuery, CancellationToken.None);

            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);
            var responseBody = okResult.Value as ElasticResponse;
            Assert.IsNotNull(responseBody);

            var sourceVAC = responseBody.Hits?.Hits?[0].Source?.AsObject();

            Assert.IsNotNull(sourceVAC);
            var vacObject = sourceVAC["VAC"]?.AsObject();
            Assert.IsNotNull(vacObject);
            Assert.IsFalse(vacObject.ContainsKey("toelichting"));
            Assert.IsTrue(vacObject.ContainsKey("allowedField"));
            Assert.AreEqual("This should remain", vacObject["allowedField"]?.ToString());
        }

        [TestMethod]
        public async Task Search_KennisbankUser_RemovesExcludedFieldsFromNestedObjects()
        {
            var elasticQuery = new JsonObject
            {
                ["query"] = new JsonObject { ["match_all"] = new JsonObject() }
            };

            var elasticResponse = "{\"hits\":{\"hits\":[{\"_source\":{\"title\":\"First item\",\"object_meta\":null,\"object_bron\":\"VAC\",\"VAC\":{\"allowedField\":\"This should remain\",\"status\":\"actief\",\"toelichting\":\"This has to be removed\"}}},{\"_source\":{\"title\":\"Second item\",\"object_bron\":\"Kennisbank\",\"Kennisbank\":{\"existingField\":\"This has to stay\",\"vertalingen\":{\"deskMemo\":\"This has to be removed\"}}}}]}}";

            _mockHttp.When(HttpMethod.Post, "https://elasticsearch.example.com/test-index/_search")
                .Respond("application/json", elasticResponse);

            var result = await _controller.Search("test-index", elasticQuery, CancellationToken.None);

            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);
            var responseBody = okResult.Value as ElasticResponse;
            Assert.IsNotNull(responseBody);

            var sourceKennisbank = responseBody.Hits?.Hits?[1].Source?["Kennisbank"]?.AsObject();

            Assert.IsNotNull(sourceKennisbank);
            Assert.IsFalse(sourceKennisbank["vertalingen"]?.AsObject().ContainsKey("deskMemo"));
            Assert.IsTrue(sourceKennisbank.ContainsKey("existingField"));
            Assert.AreEqual("This has to stay", sourceKennisbank["existingField"]?.ToString());
        }

        [TestMethod]
        public async Task Search_KennisbankUser_RemovesExcludedFieldsFromArrays()
        {
            var elasticQuery = new JsonObject
            {
                ["query"] = new JsonObject { ["match_all"] = new JsonObject() }
            };

            var elasticResponse = "{\"hits\":{\"hits\":[{\"_source\":{\"title\":\"First item\",\"object_meta\":null,\"object_bron\":\"VAC\",\"VAC\":[{\"allowedField\":\"This should remain\",\"status\":\"actief\",\"toelichting\":\"This has to be removed\"}]}},{\"_source\":{\"title\":\"Second item\",\"object_bron\":\"Kennisbank\",\"Kennisbank\":{\"existingField\":\"This has to stay\",\"vertalingen\":{\"deskMemo\":\"This has to be removed\"}}}}]}}";

            _mockHttp.When(HttpMethod.Post, "https://elasticsearch.example.com/test-index/_search")
                .Respond("application/json", elasticResponse);

            var result = await _controller.Search("test-index", elasticQuery, CancellationToken.None);

            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);
            var responseBody = okResult.Value as ElasticResponse;
            Assert.IsNotNull(responseBody);

            var sourceVAC = responseBody.Hits?.Hits?[0].Source?["VAC"]?[0]?.AsObject();

            Assert.IsNotNull(sourceVAC);
            Assert.IsFalse(sourceVAC.ContainsKey("toelichting"));
            Assert.IsTrue(sourceVAC.ContainsKey("allowedField"));
            Assert.AreEqual("This should remain", sourceVAC["allowedField"]?.ToString());
        }

        #endregion

        #region Only Kennisbank Users Are Affected Tests

        [TestMethod]
        public async Task Search_NonKennisbankUser_DoesNotModifyRequest()
        {
            // Create a non-Kennisbank user
            _httpContext.User = new ClaimsPrincipal(new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.Email, "test@example.com"),
                new Claim(ClaimTypes.Role, "OtherRole")
            }, "TestAuth"));

            CreateController(); // Recreate with new user

            var elasticQuery = new JsonObject
            {
                ["query"] = new JsonObject
                {
                    ["multi_match"] = new JsonObject
                    {
                        ["query"] = "test",
                        ["fields"] = new JsonArray(
                            "VAC.toelichting^1.0",
                            "Kennisbank.vertalingen.deskMemo^1.0",
                            "title^1.0"
                        )
                    }
                },
                ["size"] = 10
            };

            var capturedRequest = "";
            _mockHttp.When(HttpMethod.Post, "https://elasticsearch.example.com/test-index/_search")
                .With(req =>
                {
                    capturedRequest = req.Content!.ReadAsStringAsync().Result;
                    return true;
                })
                .Respond("application/json", JsonSerializer.Serialize(new
                {
                    hits = new { hits = Array.Empty<object>() }
                }));

            await _controller.Search("test-index", elasticQuery, CancellationToken.None);

            var parsedRequest = JsonNode.Parse(capturedRequest);
            Assert.IsNotNull(parsedRequest);

            // Fields should NOT be removed
            var fields = parsedRequest["query"]?["multi_match"]?["fields"]?.AsArray();
            Assert.IsNotNull(fields);
            Assert.AreEqual(3, fields.Count);
            Assert.IsTrue(fields.Any(x => x?.ToString() == "VAC.toelichting^1.0"));
            Assert.IsTrue(fields.Any(x => x?.ToString() == "Kennisbank.vertalingen.deskMemo^1.0"));
            Assert.IsTrue(fields.Any(x => x?.ToString() == "title^1.0"));

            // Original query should be preserved
            Assert.IsNotNull(parsedRequest["query"]);
            Assert.AreEqual(10, parsedRequest["size"]?.GetValue<int>());
        }

        [TestMethod]
        public async Task Search_NonKennisbankUser_DoesNotModifyResponse()
        {
            // Create a non-Kennisbank user
            _httpContext.User = new ClaimsPrincipal(new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.Email, "test@example.com"),
                new Claim(ClaimTypes.Role, "OtherRole")
            }, "TestAuth"));

            CreateController(); // Recreate with new user

            var elasticQuery = new JsonObject
            {
                ["query"] = new JsonObject { ["match_all"] = new JsonObject() }
            };

            var elasticResponse = new
            {
                hits = new
                {
                    hits = new[]
                    {
                        new
                        {
                            _source = new
                            {
                                title = "Test Document",
                                toelichting = "This should remain for non-Kennisbank users",
                                internalField = "This should also remain"
                            }
                        }
                    }
                }
            };

            _mockHttp.When(HttpMethod.Post, "https://elasticsearch.example.com/test-index/_search")
                .Respond("application/json", JsonSerializer.Serialize(elasticResponse));

            var result = await _controller.Search("test-index", elasticQuery, CancellationToken.None);

            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);
            var responseBody = okResult.Value as ElasticResponse;
            Assert.IsNotNull(responseBody);

            var source = responseBody.Hits?.Hits?[0].Source?.AsObject();

            Assert.IsNotNull(source);
            Assert.IsTrue(source.ContainsKey("toelichting"));
            Assert.IsTrue(source.ContainsKey("internalField"));
            Assert.AreEqual("This should remain for non-Kennisbank users", source["toelichting"]?.ToString());
        }

        [TestMethod]
        public async Task Search_KennisbankUser_WithNoEnvironmentVariableConfigured_DoesNotModifyRequestOrResponse()
        {
            _configurationMock.Setup(c => c["ELASTIC_EXCLUDED_FIELDS_KENNISBANK"]).Returns("");
            CreateController(); // Recreate with empty excluded fields

            var elasticQuery = new JsonObject
            {
                ["query"] = new JsonObject
                {
                    ["multi_match"] = new JsonObject
                    {
                        ["query"] = "test",
                        ["fields"] = new JsonArray(
                            "VAC.toelichting^1.0",
                            "title^1.0"
                        )
                    }
                }
            };

            var capturedRequest = "";
            var elasticResponse = new
            {
                hits = new
                {
                    hits = new[]
                    {
                        new { _source = new { toelichting = "Should remain" } }
                    }
                }
            };

            _mockHttp.When(HttpMethod.Post, "https://elasticsearch.example.com/test-index/_search")
                .With(req =>
                {
                    capturedRequest = req.Content!.ReadAsStringAsync().Result;
                    return true;
                })
                .Respond("application/json", JsonSerializer.Serialize(elasticResponse));

            var result = await _controller.Search("test-index", elasticQuery, CancellationToken.None);

            // Assert - Request should not be modified
            var parsedRequest = JsonNode.Parse(capturedRequest);
            var fields = parsedRequest!["query"]?["multi_match"]?["fields"]?.AsArray();
            Assert.IsNotNull(fields);
            Assert.AreEqual(2, fields.Count);
            Assert.IsTrue(fields.Any(x => x?.ToString() == "VAC.toelichting^1.0"));
            Assert.IsTrue(fields.Any(x => x?.ToString() == "title^1.0"));

            // Assert - Response should not be modified
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);
            var responseBody = okResult.Value as ElasticResponse;
            Assert.IsNotNull(responseBody);
            var source = responseBody.Hits?.Hits?[0].Source?.AsObject();
            Assert.IsNotNull(source);
            Assert.IsTrue(source.ContainsKey("toelichting"));
        }

        [TestMethod]
        public async Task Search_UserWithKennisbankAndKcmRoles_DoesNotRemoveFieldsFromRequest()
        {
            // Create a user with both Kennisbank and Kcm roles
            _httpContext.User = new ClaimsPrincipal(new ClaimsIdentity(
            [
                new Claim(ClaimTypes.Email, "test@example.com"),
                new Claim(ClaimTypes.Role, "Kennisbank"),
                new Claim(ClaimTypes.Role, "Kcm")
            ], "TestAuth"));

            CreateController(); // Recreate with new user

            var elasticQuery = new JsonObject
            {
                ["query"] = new JsonObject
                {
                    ["multi_match"] = new JsonObject
                    {
                        ["query"] = "test",
                        ["fields"] = new JsonArray(
                            "VAC.toelichting^1.0",
                            "Kennisbank.vertalingen.deskMemo^1.0",
                            "title^1.0"
                        )
                    }
                }
            };

            var capturedRequest = "";
            _mockHttp.When(HttpMethod.Post, "https://elasticsearch.example.com/test-index/_search")
                .With(req =>
                {
                    capturedRequest = req.Content!.ReadAsStringAsync().Result;
                    return true;
                })
                .Respond("application/json", JsonSerializer.Serialize(new
                {
                    hits = new { hits = Array.Empty<object>() }
                }));

            await _controller.Search("test-index", elasticQuery, CancellationToken.None);

            // Assert - Fields should NOT be removed from request
            var parsedRequest = JsonNode.Parse(capturedRequest);
            Assert.IsNotNull(parsedRequest);
            var fields = parsedRequest["query"]?["multi_match"]?["fields"]?.AsArray();
            Assert.IsNotNull(fields);
            Assert.AreEqual(3, fields.Count);
            Assert.IsTrue(fields.Any(x => x?.ToString() == "VAC.toelichting^1.0"));
            Assert.IsTrue(fields.Any(x => x?.ToString() == "Kennisbank.vertalingen.deskMemo^1.0"));
            Assert.IsTrue(fields.Any(x => x?.ToString() == "title^1.0"));
        }

        [TestMethod]
        public async Task Search_UserWithKennisbankAndKcmRoles_DoesNotRemoveExcludedFieldsFromResponse()
        {
            // Create a user with both Kennisbank and Kcm roles
            _httpContext.User = new ClaimsPrincipal(new ClaimsIdentity(
            [
                new Claim(ClaimTypes.Email, "test@example.com"),
                new Claim(ClaimTypes.Role, "Kennisbank"),
                new Claim(ClaimTypes.Role, "Kcm")
            ], "TestAuth"));

            CreateController(); // Recreate with new user

            var elasticQuery = new JsonObject
            {
                ["query"] = new JsonObject { ["match_all"] = new JsonObject() }
            };

            var elasticResponse = "{\"hits\":{\"hits\":[{\"_source\":{\"title\":\"Test item\",\"VAC\":{\"allowedField\":\"This should remain\",\"toelichting\":\"This has to be removed\"}}},{\"_source\":{\"title\":\"Second item\",\"Kennisbank\":{\"existingField\":\"This has to stay\",\"vertalingen\":{\"deskMemo\":\"This has to be removed\"}}}}]}}";

            _mockHttp.When(HttpMethod.Post, "https://elasticsearch.example.com/test-index/_search")
                .Respond("application/json", elasticResponse);

            var result = await _controller.Search("test-index", elasticQuery, CancellationToken.None);

            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);
            var responseBody = okResult.Value as ElasticResponse;
            Assert.IsNotNull(responseBody);

            // Verify VAC.toelichting is NOT removed
            var sourceVAC = responseBody.Hits?.Hits?[0].Source?["VAC"]?.AsObject();
            Assert.IsNotNull(sourceVAC);
            Assert.IsTrue(sourceVAC.ContainsKey("toelichting"));
            Assert.IsTrue(sourceVAC.ContainsKey("allowedField"));

            // Verify Kennisbank.vertalingen.deskMemo is NOT removed
            var sourceKennisbank = responseBody.Hits?.Hits?[1].Source?["Kennisbank"]?.AsObject();
            Assert.IsNotNull(sourceKennisbank);
            Assert.IsTrue(sourceKennisbank["vertalingen"]?.AsObject().ContainsKey("deskMemo"));
            Assert.IsTrue(sourceKennisbank.ContainsKey("existingField"));
        }

        #endregion

        #region Proxy Functionality Tests

        [TestMethod]
        public async Task Search_ForwardsRequestToElasticsearch()
        {
            var elasticQuery = new JsonObject
            {
                ["query"] = new JsonObject
                {
                    ["term"] = new JsonObject { ["field"] = "value" }
                },
                ["size"] = 20
            };

            var wasCalled = false;
            _mockHttp.When(HttpMethod.Post, "https://elasticsearch.example.com/my-index/_search")
                .With(req =>
                {
                    wasCalled = true;
                    return true;
                })
                .Respond("application/json", JsonSerializer.Serialize(new
                {
                    hits = new { hits = Array.Empty<object>() }
                }));

            await _controller.Search("my-index", elasticQuery, CancellationToken.None);

            Assert.IsTrue(wasCalled);
        }

        #endregion
    }
}
