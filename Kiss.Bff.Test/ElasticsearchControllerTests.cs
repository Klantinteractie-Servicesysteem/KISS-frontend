using System.Net;
using System.Security.Claims;
using System.Text;
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
        private Mock<IConfiguration> _configurationMock;
        private Mock<ILogger<ElasticsearchController>> _loggerMock;
        private Mock<IsKennisbank> _isKennisbankMock;
        private MockHttpMessageHandler _mockHttp;
        private ElasticsearchController _controller;
        private DefaultHttpContext _httpContext;

        [TestInitialize]
        public void Setup()
        {
            _configurationMock = new Mock<IConfiguration>();
            _loggerMock = new Mock<ILogger<ElasticsearchController>>();
            _isKennisbankMock = new Mock<IsKennisbank>();
            _mockHttp = new MockHttpMessageHandler();

            // Setup default configuration
            _configurationMock.Setup(c => c["ELASTIC_BASE_URL"]).Returns("https://elasticsearch.example.com");
            _configurationMock.Setup(c => c["ELASTIC_USERNAME"]).Returns("testuser");
            _configurationMock.Setup(c => c["ELASTIC_PASSWORD"]).Returns("testpass");
            _configurationMock.Setup(c => c["ELASTICSEARCH_KENNISBANK_EXCLUDED_FIELDS"]).Returns("VAC.toelichting,Kennisbank.vertalingen.deskMemo");

            _httpContext = new DefaultHttpContext
            {
                User = new ClaimsPrincipal(new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.Email, "test@example.com"),
                    new Claim(ClaimTypes.Role, "Kennisbank")
                }, "TestAuth"))
            };

            CreateController();
        }

        private void CreateController()
        {
            var httpClient = _mockHttp.ToHttpClient();
            _controller = new ElasticsearchController(
                httpClient,
                _configurationMock.Object,
                _loggerMock.Object,
                _isKennisbankMock.Object
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
            _isKennisbankMock.Setup(x => x(It.IsAny<ClaimsPrincipal>())).Returns(true);

            var requestBody = JsonSerializer.Serialize(new
            {
                query = new { match_all = new { } }
            });

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

            _httpContext.Request.Body = new MemoryStream(Encoding.UTF8.GetBytes(requestBody));

            await _controller.Search("test-index");

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
            _isKennisbankMock.Setup(x => x(It.IsAny<ClaimsPrincipal>())).Returns(true);

            var requestBody = JsonSerializer.Serialize(new
            {
                query = new { match_all = new { } },
                _source = new { includes = new[] { "field1", "field2" } }
            });

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

            _httpContext.Request.Body = new MemoryStream(Encoding.UTF8.GetBytes(requestBody));

            await _controller.Search("test-index");

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
            _isKennisbankMock.Setup(x => x(It.IsAny<ClaimsPrincipal>())).Returns(true);

            var requestBody = JsonSerializer.Serialize(new
            {
                query = new { match_all = new { } },
                _source = new
                {
                    excludes = new[] { "existingField" }
                }
            });

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

            _httpContext.Request.Body = new MemoryStream(Encoding.UTF8.GetBytes(requestBody));

            await _controller.Search("test-index");

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
            // Arrange
            _isKennisbankMock.Setup(x => x(It.IsAny<ClaimsPrincipal>())).Returns(true);

            var requestBody = JsonSerializer.Serialize(new
            {
                query = new { match_all = new { } },
                _source = new
                {
                    excludes = new[] { "VAC.toelichting" }
                }
            });

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

            _httpContext.Request.Body = new MemoryStream(Encoding.UTF8.GetBytes(requestBody));

            // Act
            await _controller.Search("test-index");

            // Assert
            var parsedRequest = JsonNode.Parse(capturedRequest);
            var sourceExcludes = parsedRequest!["_source"]?["excludes"]?.AsArray();
            Assert.IsNotNull(sourceExcludes);

            // Should have original toelichting + internalField (not duplicate toelichting)
            Assert.AreEqual(2, sourceExcludes.Count);
            var toelichtingCount = sourceExcludes.Count(x => x?.ToString() == "VAC.toelichting");
            Assert.AreEqual(1, toelichtingCount);
        }

        #endregion

        #region Response Transformation Tests (Kennisbank Users)

        [TestMethod]
        public async Task Search_KennisbankUser_RemovesExcludedFieldsFromResponse()
        {
            // Arrange
            _isKennisbankMock.Setup(x => x(It.IsAny<ClaimsPrincipal>())).Returns(true);

            var requestBody = JsonSerializer.Serialize(new { query = new { match_all = new { } } });

            var elasticResponse = "{\"hits\":{\"hits\":[{\"_source\":{\"title\":\"First item\",\"object_meta\":null,\"object_bron\":\"VAC\",\"VAC\":{\"allowedField\":\"This should remain\",\"status\":\"actief\",\"toelichting\":\"This has to be removed\"}}},{\"_source\":{\"title\":\"Second item\",\"object_bron\":\"Kennisbank\",\"Kennisbank\":{\"existingField\":\"This has to stay\",\"vertalingen\":{\"deskMemo\":\"This has to be removed\"}}}}]}}";

            _mockHttp.When(HttpMethod.Post, "https://elasticsearch.example.com/test-index/_search")
                .Respond("application/json", elasticResponse);

            _httpContext.Request.Body = new MemoryStream(Encoding.UTF8.GetBytes(requestBody));

            // Act
            var result = await _controller.Search("test-index");

            // Assert
            var contentResult = result as ContentResult;
            Assert.IsNotNull(contentResult);

            var responseJson = JsonNode.Parse(contentResult.Content!);
            var sourceVAC = responseJson!["hits"]?["hits"]?[0]?["_source"]?["VAC"]?.AsObject();

            Assert.IsNotNull(sourceVAC);
            Assert.IsFalse(sourceVAC.ContainsKey("toelichting"));
            Assert.IsTrue(sourceVAC.ContainsKey("allowedField"));
            Assert.AreEqual("This should remain", sourceVAC["allowedField"]?.ToString());
        }

        [TestMethod]
        public async Task Search_KennisbankUser_RemovesExcludedFieldsFromNestedObjects()
        {
            // Arrange
            _isKennisbankMock.Setup(x => x(It.IsAny<ClaimsPrincipal>())).Returns(true);

            var requestBody = JsonSerializer.Serialize(new { query = new { match_all = new { } } });

            var elasticResponse = "{\"hits\":{\"hits\":[{\"_source\":{\"title\":\"First item\",\"object_meta\":null,\"object_bron\":\"VAC\",\"VAC\":{\"allowedField\":\"This should remain\",\"status\":\"actief\",\"toelichting\":\"This has to be removed\"}}},{\"_source\":{\"title\":\"Second item\",\"object_bron\":\"Kennisbank\",\"Kennisbank\":{\"existingField\":\"This has to stay\",\"vertalingen\":{\"deskMemo\":\"This has to be removed\"}}}}]}}";

            _mockHttp.When(HttpMethod.Post, "https://elasticsearch.example.com/test-index/_search")
                .Respond("application/json", elasticResponse);

            _httpContext.Request.Body = new MemoryStream(Encoding.UTF8.GetBytes(requestBody));

            // Act
            var result = await _controller.Search("test-index");

            // Assert
            var contentResult = result as ContentResult;
            Assert.IsNotNull(contentResult);

            var responseJson = JsonNode.Parse(contentResult.Content!);

            var sourceKennisbank = responseJson!["hits"]?["hits"]?[1]?["_source"]?["Kennisbank"]?.AsObject();

            Assert.IsNotNull(sourceKennisbank);
            Assert.IsFalse(sourceKennisbank["vertalingen"]?.AsObject().ContainsKey("deskMemo"));
            Assert.IsTrue(sourceKennisbank.ContainsKey("existingField"));
            Assert.AreEqual("This has to stay", sourceKennisbank["existingField"]?.ToString());
        }

        [TestMethod]
        public async Task Search_KennisbankUser_RemovesExcludedFieldsFromArrays()
        {
            // Arrange
            _isKennisbankMock.Setup(x => x(It.IsAny<ClaimsPrincipal>())).Returns(true);

            var requestBody = JsonSerializer.Serialize(new { query = new { match_all = new { } } });

            var elasticResponse = "{\"hits\":{\"hits\":[{\"_source\":{\"title\":\"First item\",\"object_meta\":null,\"object_bron\":\"VAC\",\"VAC\":[{\"allowedField\":\"This should remain\",\"status\":\"actief\",\"toelichting\":\"This has to be removed\"}]}},{\"_source\":{\"title\":\"Second item\",\"object_bron\":\"Kennisbank\",\"Kennisbank\":{\"existingField\":\"This has to stay\",\"vertalingen\":{\"deskMemo\":\"This has to be removed\"}}}}]}}";

            _mockHttp.When(HttpMethod.Post, "https://elasticsearch.example.com/test-index/_search")
                .Respond("application/json", elasticResponse);

            _httpContext.Request.Body = new MemoryStream(Encoding.UTF8.GetBytes(requestBody));

            // Act
            var result = await _controller.Search("test-index");

            // Assert
            var contentResult = result as ContentResult;
            Assert.IsNotNull(contentResult);

            var responseJson = JsonNode.Parse(contentResult.Content!);

            var sourceVAC = responseJson!["hits"]?["hits"]?[0]?["_source"]?["VAC"]?[0]?.AsObject();

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
            // Arrange
            _isKennisbankMock.Setup(x => x(It.IsAny<ClaimsPrincipal>())).Returns(false);

            var originalQuery = new
            {
                query = new { match_all = new { } },
                size = 10
            };
            var requestBody = JsonSerializer.Serialize(originalQuery);

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

            _httpContext.Request.Body = new MemoryStream(Encoding.UTF8.GetBytes(requestBody));

            // Act
            await _controller.Search("test-index");

            // Assert
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
            // Arrange
            _isKennisbankMock.Setup(x => x(It.IsAny<ClaimsPrincipal>())).Returns(false);

            var requestBody = JsonSerializer.Serialize(new { query = new { match_all = new { } } });

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

            _httpContext.Request.Body = new MemoryStream(Encoding.UTF8.GetBytes(requestBody));

            // Act
            var result = await _controller.Search("test-index");

            // Assert
            var contentResult = result as ContentResult;
            Assert.IsNotNull(contentResult);

            var responseJson = JsonNode.Parse(contentResult.Content!);
            var source = responseJson!["hits"]?["hits"]?[0]?["_source"]?.AsObject();

            Assert.IsNotNull(source);
            Assert.IsTrue(source.ContainsKey("toelichting"));
            Assert.IsTrue(source.ContainsKey("internalField"));
            Assert.AreEqual("This should remain for non-Kennisbank users", source["toelichting"]?.ToString());
        }

        [TestMethod]
        public async Task Search_KennisbankUser_WithNoExcludedFieldsConfigured_DoesNotModifyRequestOrResponse()
        {
            // Arrange
            _configurationMock.Setup(c => c["ELASTICSEARCH_KENNISBANK_EXCLUDED_FIELDS"]).Returns("");
            CreateController(); // Recreate with empty excluded fields

            _isKennisbankMock.Setup(x => x(It.IsAny<ClaimsPrincipal>())).Returns(true);

            var requestBody = JsonSerializer.Serialize(new { query = new { match_all = new { } } });

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

            _httpContext.Request.Body = new MemoryStream(Encoding.UTF8.GetBytes(requestBody));

            // Act
            var result = await _controller.Search("test-index");

            // Assert - Request should not be modified
            var parsedRequest = JsonNode.Parse(capturedRequest);
            Assert.IsFalse(parsedRequest!.AsObject().ContainsKey("_source"));

            // Assert - Response should not be modified
            var contentResult = result as ContentResult;
            var responseJson = JsonNode.Parse(contentResult!.Content!);
            var source = responseJson!["hits"]?["hits"]?[0]?["_source"]?.AsObject();
            Assert.IsTrue(source!.ContainsKey("toelichting"));
        }

        #endregion

        #region Proxy Functionality Tests

        [TestMethod]
        public async Task Search_ForwardsRequestToElasticsearch()
        {
            // Arrange
            _isKennisbankMock.Setup(x => x(It.IsAny<ClaimsPrincipal>())).Returns(false);

            var requestBody = JsonSerializer.Serialize(new
            {
                query = new { term = new { field = "value" } },
                size = 20
            });

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

            _httpContext.Request.Body = new MemoryStream(Encoding.UTF8.GetBytes(requestBody));

            // Act
            await _controller.Search("my-index");

            // Assert
            Assert.IsTrue(wasCalled);
        }

        [TestMethod]
        public async Task Search_AddsBasicAuthenticationHeader()
        {
            // Arrange
            _isKennisbankMock.Setup(x => x(It.IsAny<ClaimsPrincipal>())).Returns(false);

            var requestBody = JsonSerializer.Serialize(new { query = new { match_all = new { } } });

            string? capturedAuthHeader = null;
            _mockHttp.When(HttpMethod.Post, "https://elasticsearch.example.com/test-index/_search")
                .With(req =>
                {
                    capturedAuthHeader = req.Headers.Authorization?.ToString();
                    return true;
                })
                .Respond("application/json", JsonSerializer.Serialize(new
                {
                    hits = new { hits = Array.Empty<object>() }
                }));

            _httpContext.Request.Body = new MemoryStream(Encoding.UTF8.GetBytes(requestBody));

            // Act
            await _controller.Search("test-index");

            // Assert
            Assert.IsNotNull(capturedAuthHeader);
            Assert.IsTrue(capturedAuthHeader.StartsWith("Basic "));

            // Verify the credentials are correctly encoded
            var base64Credentials = capturedAuthHeader.Replace("Basic ", "");
            var decodedCredentials = Encoding.UTF8.GetString(Convert.FromBase64String(base64Credentials));
            Assert.AreEqual("testuser:testpass", decodedCredentials);
        }

        [TestMethod]
        public async Task Search_ReturnsElasticsearchResponseWithCorrectStatusCode()
        {
            // Arrange
            _isKennisbankMock.Setup(x => x(It.IsAny<ClaimsPrincipal>())).Returns(false);

            var requestBody = JsonSerializer.Serialize(new { query = new { match_all = new { } } });

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

            _httpContext.Request.Body = new MemoryStream(Encoding.UTF8.GetBytes(requestBody));

            // Act
            var result = await _controller.Search("test-index");

            // Assert
            var contentResult = result as ContentResult;
            Assert.IsNotNull(contentResult);
            Assert.AreEqual(200, contentResult.StatusCode);
            Assert.AreEqual("application/json", contentResult.ContentType);
        }

        #endregion

        #region Error Handling Tests

        [TestMethod]
        public async Task Search_ReturnsConfigurationError_WhenConfigurationMissing()
        {
            // Arrange
            var configMock = new Mock<IConfiguration>();
            configMock.Setup(c => c["ELASTIC_BASE_URL"]).Returns((string?)null);

            // Act & Assert
            Assert.ThrowsException<InvalidOperationException>(() =>
            {
                new ElasticsearchController(
                    new HttpClient(),
                    configMock.Object,
                    _loggerMock.Object,
                    _isKennisbankMock.Object
                );
            });
        }

        [TestMethod]
        public async Task Search_ReturnsBadRequest_WhenRequestBodyIsEmpty()
        {
            // Arrange
            _httpContext.Request.Body = new MemoryStream(Encoding.UTF8.GetBytes(""));

            // Act
            var result = await _controller.Search("test-index");

            // Assert
            var badRequestResult = result as BadRequestObjectResult;
            Assert.IsNotNull(badRequestResult);
            Assert.AreEqual(400, badRequestResult.StatusCode);
        }

        [TestMethod]
        public async Task Search_ReturnsBadRequest_WhenRequestBodyIsWhitespace()
        {
            // Arrange
            _httpContext.Request.Body = new MemoryStream(Encoding.UTF8.GetBytes("   \n\t  "));

            // Act
            var result = await _controller.Search("test-index");

            // Assert
            var badRequestResult = result as BadRequestObjectResult;
            Assert.IsNotNull(badRequestResult);
            Assert.AreEqual(400, badRequestResult.StatusCode);
        }

        [TestMethod]
        public async Task Search_ReturnsBadRequest_WhenRequestBodyIsInvalidJson()
        {
            // Arrange
            var invalidJson = "{ invalid json here }}}";
            _httpContext.Request.Body = new MemoryStream(Encoding.UTF8.GetBytes(invalidJson));

            // Act
            var result = await _controller.Search("test-index");

            // Assert
            var badRequestResult = result as BadRequestObjectResult;
            Assert.IsNotNull(badRequestResult);
            Assert.AreEqual(400, badRequestResult.StatusCode);
        }

        [TestMethod]
        public async Task Search_ReturnsBadRequest_WhenRequestBodyIsNotJsonObject()
        {
            // Arrange
            var jsonArray = "[1, 2, 3]";
            _httpContext.Request.Body = new MemoryStream(Encoding.UTF8.GetBytes(jsonArray));

            // Act
            var result = await _controller.Search("test-index");

            // Assert
            var badRequestResult = result as BadRequestObjectResult;
            Assert.IsNotNull(badRequestResult);
            Assert.AreEqual(400, badRequestResult.StatusCode);
        }

        [TestMethod]
        public async Task Search_Returns502_WhenElasticsearchReturnsError()
        {
            // Arrange
            _isKennisbankMock.Setup(x => x(It.IsAny<ClaimsPrincipal>())).Returns(false);

            var requestBody = JsonSerializer.Serialize(new { query = new { match_all = new { } } });

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

            _httpContext.Request.Body = new MemoryStream(Encoding.UTF8.GetBytes(requestBody));

            // Act
            var result = await _controller.Search("test-index");

            // Assert
            var statusCodeResult = result as ObjectResult;
            Assert.IsNotNull(statusCodeResult);
            Assert.AreEqual(502, statusCodeResult.StatusCode);
        }

        [TestMethod]
        public async Task Search_Returns502_WhenElasticsearchIsUnreachable()
        {
            // Arrange
            _isKennisbankMock.Setup(x => x(It.IsAny<ClaimsPrincipal>())).Returns(false);

            var requestBody = JsonSerializer.Serialize(new { query = new { match_all = new { } } });

            _mockHttp.When(HttpMethod.Post, "https://elasticsearch.example.com/test-index/_search")
                .Throw(new HttpRequestException("Connection refused"));

            _httpContext.Request.Body = new MemoryStream(Encoding.UTF8.GetBytes(requestBody));

            // Act
            var result = await _controller.Search("test-index");

            // Assert
            var statusCodeResult = result as ObjectResult;
            Assert.IsNotNull(statusCodeResult);
            Assert.AreEqual(502, statusCodeResult.StatusCode);
        }

        [TestMethod]
        public async Task Search_LogsError_WhenElasticsearchReturnsError()
        {
            // Arrange
            _isKennisbankMock.Setup(x => x(It.IsAny<ClaimsPrincipal>())).Returns(false);

            var requestBody = JsonSerializer.Serialize(new { query = new { match_all = new { } } });

            _mockHttp.When(HttpMethod.Post, "https://elasticsearch.example.com/test-index/_search")
                .Respond(HttpStatusCode.InternalServerError, "application/json", "{\"error\":\"Something went wrong\"}");

            _httpContext.Request.Body = new MemoryStream(Encoding.UTF8.GetBytes(requestBody));

            // Act
            await _controller.Search("test-index");

            // Assert
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((o, t) => o.ToString()!.Contains("Elasticsearch returned error")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        #endregion
    }
}
