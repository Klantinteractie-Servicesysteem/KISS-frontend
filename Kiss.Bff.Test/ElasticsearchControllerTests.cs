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
        private ElasticsearchService _service = null!;

        [TestInitialize]
        public void Setup()
        {
            _configurationMock = new Mock<IConfiguration>();
            _loggerMock = new Mock<ILogger<ElasticsearchController>>();
            _mockHttp = new MockHttpMessageHandler();
            _isKennisbank = (user) => user?.IsInRole("Kennisbank") ?? false;
            _isKcm = (user) => user?.IsInRole("Kcm") ?? false;

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
            CreateService();

            _controller = new ElasticsearchController(
                _service,
                _loggerMock.Object
            );
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = _httpContext
            };
        }

        private void CreateService()
        {
            var httpClient = _mockHttp.ToHttpClient();
            httpClient.BaseAddress = new Uri("https://elasticsearch.example.com");

            _service = new ElasticsearchService(httpClient, _isKennisbank, _isKcm, _httpContext.User, _configurationMock.Object);
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

            _service.ApplyRequestTransform(elasticQuery);

            var fields = elasticQuery["query"]?["multi_match"]?["fields"]?.AsArray();
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
            var elasticResponse = new ElasticResponse
            {
                Hits = new HitsWrapper
                {
                    Hits = new List<Hit>
                    {
                        new Hit
                        {
                            Source = new JsonObject
                            {
                                ["title"] = "First item",
                                ["VAC"] = new JsonObject
                                    {
                                        ["allowedField"] = "This should remain",
                                        ["toelichting"] = "This must be removed!"
                                    },
                                ["toelichting"] = "Should remain"
                            }
                        },
                    new Hit
                    {
                        Source = new JsonObject
                        {
                            ["title"] = "Second item",
                            ["Kennisbank"] = new JsonObject
                            {
                                ["allowedField"] = "This should remain",
                                ["vertalingen"] = new JsonObject
                                {
                                    ["deskMemo"] = "This must be removed!"
                                }
                            },
                            ["toelichting"] = "This should remain"
                        }
                    }
                }
                }
            };

            _service.ApplyResponseTransform(elasticResponse);

            var sourceVAC = elasticResponse.Hits?.Hits?[0].Source?.AsObject();

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
            var elasticResponse = new ElasticResponse
            {
                Hits = new HitsWrapper
                {
                    Hits = new List<Hit>
                    {
                        new Hit
                        {
                            Source = new JsonObject
                            {
                                ["title"] = "First item",
                                ["VAC"] = new JsonObject
                                    {
                                        ["allowedField"] = "This should remain",
                                        ["toelichting"] = "This must be removed!"
                                    },
                                ["toelichting"] = "Should remain"
                            }
                        },
                    new Hit
                    {
                        Source = new JsonObject
                        {
                            ["title"] = "Second item",
                            ["Kennisbank"] = new JsonObject
                            {
                                ["allowedField"] = "This should remain",
                                ["vertalingen"] = new JsonObject
                                {
                                    ["deskMemo"] = "This must be removed!"
                                }
                            },
                            ["toelichting"] = "This should remain"
                        }
                    }
                }
                }
            };

            _service.ApplyResponseTransform(elasticResponse);

            var sourceKennisbank = elasticResponse.Hits?.Hits?[1].Source?["Kennisbank"]?.AsObject();

            Assert.IsNotNull(sourceKennisbank);
            Assert.IsFalse(sourceKennisbank["vertalingen"]?.AsObject().ContainsKey("deskMemo"));
            Assert.IsTrue(sourceKennisbank.ContainsKey("allowedField"));
            Assert.AreEqual("This should remain", sourceKennisbank["allowedField"]?.ToString());
        }

        [TestMethod]
        public async Task Search_KennisbankUser_RemovesExcludedFieldsFromArrays()
        {
            var elasticResponse = new ElasticResponse
            {
                Hits = new HitsWrapper
                {
                    Hits = new List<Hit>
                    {
                        new Hit
                        {
                            Source = new JsonObject
                            {
                                ["title"] = "First item",
                                ["VAC"] = new JsonArray(

                                    new JsonObject {["allowedField"] = "This should remain"},
                                    new JsonObject {["toelichting"] = "This must be removed!"}
                                ),
                                ["toelichting"] = "Should remain"
                            }
                        },
                        new Hit
                        {
                            Source = new JsonObject
                            {
                                ["title"] = "Second item",
                                ["Kennisbank"] = new JsonObject
                                {
                                    ["allowedField"] = "This should remain",
                                    ["vertalingen"] = new JsonObject
                                    {
                                        ["deskMemo"] = "This must be removed!"
                                    }
                                },
                                ["toelichting"] = "This should remain"
                            }
                        }
                    }
                }
            };

            _service.ApplyResponseTransform(elasticResponse);

            var sourceVAC = elasticResponse.Hits?.Hits?[0].Source?["VAC"]?[0]?.AsObject();

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

            _service.ApplyRequestTransform(elasticQuery);

            // Fields should NOT be removed
            var fields = elasticQuery["query"]?["multi_match"]?["fields"]?.AsArray();
            Assert.IsNotNull(fields);
            Assert.AreEqual(3, fields.Count);
            Assert.IsTrue(fields.Any(x => x?.ToString() == "VAC.toelichting^1.0"));
            Assert.IsTrue(fields.Any(x => x?.ToString() == "Kennisbank.vertalingen.deskMemo^1.0"));
            Assert.IsTrue(fields.Any(x => x?.ToString() == "title^1.0"));

            // Original query should be preserved
            Assert.IsNotNull(elasticQuery["query"]);
            Assert.AreEqual(10, elasticQuery["size"]?.GetValue<int>());
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

            var elasticResponse = new ElasticResponse
            {
                Hits = new HitsWrapper
                {
                    Hits = new List<Hit>
                    {
                        new Hit
                        {
                            Source = new JsonObject
                            {
                                ["title"] = "Test document",
                                ["toelichting"] = "This should remain for non-Kennisbank users",
                                ["allowedField"] = "This should remain"
                            }
                        }
                    }
                }
            };

            var source = elasticResponse.Hits?.Hits?[0].Source?.AsObject();

            Assert.IsNotNull(source);
            Assert.IsTrue(source.ContainsKey("toelichting"));
            Assert.IsTrue(source.ContainsKey("allowedField"));
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

            var elasticResponse = new ElasticResponse
            {
                Hits = new HitsWrapper
                {
                    Hits = new List<Hit>
                    {
                        new Hit
                        {
                            Source = new JsonObject
                            {
                                ["toelichting"] = "Should remain"
                            }
                        }
                    }
                }
            };

            // Assert - Request should not be modified
            _service.ApplyRequestTransform(elasticQuery);
            var fields = elasticQuery!["query"]?["multi_match"]?["fields"]?.AsArray();
            Assert.IsNotNull(fields);
            Assert.AreEqual(2, fields.Count);
            Assert.IsTrue(fields.Any(x => x?.ToString() == "VAC.toelichting^1.0"));
            Assert.IsTrue(fields.Any(x => x?.ToString() == "title^1.0"));

            // Assert - Response should not be modified
            _service.ApplyResponseTransform(elasticResponse);
            var source = elasticResponse.Hits?.Hits?[0].Source?.AsObject();
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

            _service.ApplyRequestTransform(elasticQuery);

            var fields = elasticQuery["query"]?["multi_match"]?["fields"]?.AsArray();
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

            var responseBody = new ElasticResponse
            {
                Hits = new HitsWrapper
                {
                    Hits = new List<Hit>
                    {
                        new Hit
                        {
                            Source = new JsonObject
                            {
                                ["title"] = "First item",
                                ["VAC"] = new JsonObject
                                {
                                    ["allowedField"] = "This should remain",
                                    ["toelichting"] = "This must be removed!"
                                },
                                ["toelichting"] = "Should remain"
                            }
                        },
                        new Hit
                        {
                            Source = new JsonObject
                            {
                                ["title"] = "Second item",
                                ["Kennisbank"] = new JsonObject
                                {
                                    ["allowedField"] = "This should remain",
                                    ["vertalingen"] = new JsonObject
                                    {
                                        ["deskMemo"] = "This must be removed!"
                                    }
                                },
                                ["toelichting"] = "This should remain"
                            }
                        }
                    }
                }
            };

            _service.ApplyResponseTransform(responseBody);

            // Verify VAC.toelichting is NOT removed
            var sourceVAC = responseBody.Hits?.Hits?[0].Source?["VAC"]?.AsObject();
            Assert.IsNotNull(sourceVAC);
            Assert.IsTrue(sourceVAC.ContainsKey("toelichting"));
            Assert.IsTrue(sourceVAC.ContainsKey("allowedField"));

            // Verify Kennisbank.vertalingen.deskMemo is NOT removed
            var sourceKennisbank = responseBody.Hits?.Hits?[1].Source?["Kennisbank"]?.AsObject();
            Assert.IsNotNull(sourceKennisbank);
            Assert.IsTrue(sourceKennisbank["vertalingen"]?.AsObject().ContainsKey("deskMemo"));
            Assert.IsTrue(sourceKennisbank.ContainsKey("allowedField"));
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
