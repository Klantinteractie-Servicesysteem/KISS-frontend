using System.Net;
using System.Security.Claims;
using System.Text.Json;
using System.Text.Json.Nodes;
using Kiss.Bff.Extern.ElasticSearch;
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
            _configurationMock.Setup(c => c["ELASTICSEARCH_EXCLUDED_FIELDS_KENNISBANK"]).Returns("VAC.toelichting,Kennisbank.vertalingen.deskMemo");

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
        public async Task Search_KennisbankUser_AddsExcludedFieldsToRequest_WhenNoSourceInQuery()
        {
            var elasticQuery = new JsonObject
            {
                ["query"] = new JsonObject { ["match_all"] = new JsonObject() }
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
                    hits = new
                    {
                        hits = Array.Empty<object>()
                    }
                }));

            await _controller.Search("test-index", elasticQuery, CancellationToken.None);

            var parsedRequest = JsonNode.Parse(capturedRequest);
            Assert.IsNotNull(parsedRequest);
            var sourceExcludes = parsedRequest["_source"]?["excludes"]?.AsArray();
            Assert.IsNotNull(sourceExcludes);
            Assert.AreEqual(2, sourceExcludes.Count);
            Assert.IsTrue(sourceExcludes.Any(x => x?.ToString() == "VAC.toelichting"));
            Assert.IsTrue(sourceExcludes.Any(x => x?.ToString() == "Kennisbank.vertalingen.deskMemo"));
        }

        [TestMethod]
        public async Task Search_KennisbankUser_AddsExcludedFieldsToRequest_WhenSourceExistsWithoutExcludes()
        {
            var elasticQuery = new JsonObject
            {
                ["query"] = new JsonObject { ["match_all"] = new JsonObject() },
                ["_source"] = new JsonObject
                {
                    ["includes"] = new JsonArray("field1", "field2")
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
            var sourceExcludes = parsedRequest!["_source"]?["excludes"]?.AsArray();
            Assert.IsNotNull(sourceExcludes);
            Assert.AreEqual(2, sourceExcludes.Count);

            var sourceIncludes = parsedRequest["_source"]?["includes"]?.AsArray();
            Assert.IsNotNull(sourceIncludes);
            Assert.AreEqual(2, sourceIncludes.Count);
        }

        [TestMethod]
        public async Task Search_KennisbankUser_MergesWithExistingExcludes()
        {
            var elasticQuery = new JsonObject
            {
                ["query"] = new JsonObject { ["match_all"] = new JsonObject() },
                ["_source"] = new JsonObject
                {
                    ["excludes"] = new JsonArray("existingField")
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
            var sourceExcludes = parsedRequest!["_source"]?["excludes"]?.AsArray();
            Assert.IsNotNull(sourceExcludes);
            Assert.AreEqual(3, sourceExcludes.Count);
            Assert.IsTrue(sourceExcludes.Any(x => x?.ToString() == "existingField"));
            Assert.IsTrue(sourceExcludes.Any(x => x?.ToString() == "VAC.toelichting"));
            Assert.IsTrue(sourceExcludes.Any(x => x?.ToString() == "Kennisbank.vertalingen.deskMemo"));
        }

        [TestMethod]
        public async Task Search_KennisbankUser_DoesNotDuplicateExistingExcludes()
        {
            var elasticQuery = new JsonObject
            {
                ["query"] = new JsonObject { ["match_all"] = new JsonObject() },
                ["_source"] = new JsonObject
                {
                    ["excludes"] = new JsonArray("VAC.toelichting")
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
            var sourceExcludes = parsedRequest!["_source"]?["excludes"]?.AsArray();
            Assert.IsNotNull(sourceExcludes);

            // Should have original toelichting + deskMemo (not duplicate toelichting)
            Assert.AreEqual(2, sourceExcludes.Count);
            var toelichtingCount = sourceExcludes.Count(x => x?.ToString() == "VAC.toelichting");
            Assert.AreEqual(1, toelichtingCount);
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
                ["query"] = new JsonObject { ["match_all"] = new JsonObject() },
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

            // Should not have _source.excludes added
            Assert.IsFalse(parsedRequest.AsObject().ContainsKey("_source"));

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
        public async Task Search_KennisbankUser_WithNoExcludedFieldsConfigured_DoesNotModifyRequestOrResponse()
        {
            _configurationMock.Setup(c => c["ELASTICSEARCH_EXCLUDED_FIELDS_KENNISBANK"]).Returns("");
            CreateController(); // Recreate with empty excluded fields

            var elasticQuery = new JsonObject
            {
                ["query"] = new JsonObject { ["match_all"] = new JsonObject() }
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
            Assert.IsFalse(parsedRequest!.AsObject().ContainsKey("_source"));

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
        public async Task Search_UserWithKennisbankAndKcmRoles_AppliesRequestExclusionsLikeKcmUser()
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

            // Assert - Request should NOT have excluded fields added
            var parsedRequest = JsonNode.Parse(capturedRequest);
            Assert.IsNotNull(parsedRequest);
            var sourceExcludes = parsedRequest["_source"]?["excludes"]?.AsArray();
            Assert.IsNull(sourceExcludes);
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

        // Note: Authentication is configured at the HttpClient level in Program.cs via DI,
        // not in the controller or service layer, so we don't test it here

        [TestMethod]
        public async Task Search_ReturnsElasticsearchResponseWithCorrectStatusCode()
        {
            var elasticQuery = new JsonObject
            {
                ["query"] = new JsonObject { ["match_all"] = new JsonObject() }
            };

            var elasticResponse = new
            {
                took = 5,
                hits = new
                {
                    total = new { value = 2 },
                    hits = new[]
                    {
                        new { _source = new { title = "Doc 1" } },
                        new { _source = new { title = "Doc 2" } }
                    }
                }
            };

            _mockHttp.When(HttpMethod.Post, "https://elasticsearch.example.com/test-index/_search")
                .Respond(HttpStatusCode.OK, "application/json", JsonSerializer.Serialize(elasticResponse));

            var result = await _controller.Search("test-index", elasticQuery, CancellationToken.None);

            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);
            Assert.AreEqual(200, okResult.StatusCode);
        }

        #endregion

        #region Error Handling Tests

        [TestMethod]
        public async Task Search_Returns404_WhenElasticsearchReturnsNotFound()
        {
            var elasticQuery = new JsonObject
            {
                ["query"] = new JsonObject { ["match_all"] = new JsonObject() }
            };

            var errorResponse = new
            {
                error = new
                {
                    type = "index_not_found_exception",
                    reason = "no such index [test-index]"
                }
            };

            _mockHttp.When(HttpMethod.Post, "https://elasticsearch.example.com/test-index/_search")
                .Respond(HttpStatusCode.NotFound, "application/json", JsonSerializer.Serialize(errorResponse));

            var result = await _controller.Search("test-index", elasticQuery, CancellationToken.None);

            var statusCodeResult = result as StatusCodeResult;
            Assert.IsNotNull(statusCodeResult);
            Assert.AreEqual(404, statusCodeResult.StatusCode);
        }

        [TestMethod]
        public async Task Search_Returns500_WhenElasticsearchIsUnreachable()
        {
            var elasticQuery = new JsonObject
            {
                ["query"] = new JsonObject { ["match_all"] = new JsonObject() }
            };

            _mockHttp.When(HttpMethod.Post, "https://elasticsearch.example.com/test-index/_search")
                .Throw(new HttpRequestException("Connection refused"));

            var result = await _controller.Search("test-index", elasticQuery, CancellationToken.None);

            var statusCodeResult = result as StatusCodeResult;
            Assert.IsNotNull(statusCodeResult);
            Assert.AreEqual(500, statusCodeResult.StatusCode);
        }

        [TestMethod]
        public async Task Search_LogsError_WhenElasticsearchReturnsError()
        {
            var elasticQuery = new JsonObject
            {
                ["query"] = new JsonObject { ["match_all"] = new JsonObject() }
            };

            _mockHttp.When(HttpMethod.Post, "https://elasticsearch.example.com/test-index/_search")
                .Respond(HttpStatusCode.InternalServerError, "application/json", "{\"error\":\"Something went wrong\"}");

            await _controller.Search("test-index", elasticQuery, CancellationToken.None);

            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((o, t) => o.ToString()!.Contains("Elasticsearch request failed")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        #endregion
    }
}
