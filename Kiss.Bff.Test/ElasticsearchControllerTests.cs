using System.Text.Json;
using System.Text.Json.Nodes;
using Kiss.Bff.Extern.Elasticsearch;
using Microsoft.Extensions.Configuration;
using Moq;
using RichardSzalay.MockHttp;

namespace Kiss.Bff.Test
{
    [TestClass]
    public class ElasticsearchControllerTests
    {
        #region Request Transformation Tests (Kennisbank Users)

        [TestMethod]
        public async Task Search_KennisbankUser_RemovesExcludedFieldsFromRequest()
        {
            var configuration = new Mock<IConfiguration>();
            configuration.Setup(c => c["ELASTIC_EXCLUDED_FIELDS_KENNISBANK"]).Returns("VAC.toelichting,Kennisbank.vertalingen.deskMemo");

            var service = new ElasticsearchService(null!, (isKennisBank) => true, (isKcm) => false, null!, configuration.Object);

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

            service.ApplyRequestTransform(elasticQuery);

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
            var configuration = new Mock<IConfiguration>();
            configuration.Setup(c => c["ELASTIC_EXCLUDED_FIELDS_KENNISBANK"]).Returns("VAC.toelichting,Kennisbank.vertalingen.deskMemo");

            var service = new ElasticsearchService(null!, (isKennisBank) => true, (isKcm) => false, null!, configuration.Object);

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

            service.ApplyResponseTransform(elasticResponse);

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
            var configuration = new Mock<IConfiguration>();
            configuration.Setup(c => c["ELASTIC_EXCLUDED_FIELDS_KENNISBANK"]).Returns("VAC.toelichting,Kennisbank.vertalingen.deskMemo");

            var service = new ElasticsearchService(null!, (isKennisBank) => true, (isKcm) => false, null!, configuration.Object);

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

            service.ApplyResponseTransform(elasticResponse);

            var sourceKennisbank = elasticResponse.Hits?.Hits?[1].Source?["Kennisbank"]?.AsObject();

            Assert.IsNotNull(sourceKennisbank);
            Assert.IsFalse(sourceKennisbank["vertalingen"]?.AsObject().ContainsKey("deskMemo"));
            Assert.IsTrue(sourceKennisbank.ContainsKey("allowedField"));
            Assert.AreEqual("This should remain", sourceKennisbank["allowedField"]?.ToString());
        }

        [TestMethod]
        public async Task Search_KennisbankUser_RemovesExcludedFieldsFromArrays()
        {
            var configuration = new Mock<IConfiguration>();
            configuration.Setup(c => c["ELASTIC_EXCLUDED_FIELDS_KENNISBANK"]).Returns("VAC.toelichting,Kennisbank.vertalingen.deskMemo");

            var service = new ElasticsearchService(null!, (isKennisBank) => true, (isKcm) => false, null!, configuration.Object);

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

            service.ApplyResponseTransform(elasticResponse);

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
            var configuration = new Mock<IConfiguration>();
            configuration.Setup(c => c["ELASTIC_EXCLUDED_FIELDS_KENNISBANK"]).Returns("VAC.toelichting,Kennisbank.vertalingen.deskMemo");

            var service = new ElasticsearchService(null!, (isKennisBank) => false, (isKcm) => true, null!, configuration.Object);

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

            service.ApplyRequestTransform(elasticQuery);

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
            var configuration = new Mock<IConfiguration>();
            configuration.Setup(c => c["ELASTIC_EXCLUDED_FIELDS_KENNISBANK"]).Returns("VAC.toelichting,Kennisbank.vertalingen.deskMemo");

            var service = new ElasticsearchService(null!, (isKennisBank) => false, (isKcm) => true, null!, configuration.Object);

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

            service.ApplyResponseTransform(elasticResponse);

            var source = elasticResponse.Hits?.Hits?[0].Source?.AsObject();

            Assert.IsNotNull(source);
            Assert.IsTrue(source.ContainsKey("toelichting"));
            Assert.IsTrue(source.ContainsKey("allowedField"));
            Assert.AreEqual("This should remain for non-Kennisbank users", source["toelichting"]?.ToString());
        }

        [TestMethod]
        public async Task Search_KennisbankUser_WithNoEnvironmentVariableConfigured_DoesNotModifyRequestOrResponse()
        {
            var configuration = new Mock<IConfiguration>();
            configuration.Setup(c => c["ELASTIC_EXCLUDED_FIELDS_KENNISBANK"]).Returns("");

            var service = new ElasticsearchService(null!, (isKennisBank) => true, (isKcm) => false, null!, configuration.Object);

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
            service.ApplyRequestTransform(elasticQuery);
            var fields = elasticQuery!["query"]?["multi_match"]?["fields"]?.AsArray();
            Assert.IsNotNull(fields);
            Assert.AreEqual(2, fields.Count);
            Assert.IsTrue(fields.Any(x => x?.ToString() == "VAC.toelichting^1.0"));
            Assert.IsTrue(fields.Any(x => x?.ToString() == "title^1.0"));

            // Assert - Response should not be modified
            service.ApplyResponseTransform(elasticResponse);
            var source = elasticResponse.Hits?.Hits?[0].Source?.AsObject();
            Assert.IsNotNull(source);
            Assert.IsTrue(source.ContainsKey("toelichting"));
        }

        [TestMethod]
        public async Task Search_UserWithKennisbankAndKcmRoles_DoesNotRemoveFieldsFromRequest()
        {
            var configuration = new Mock<IConfiguration>();
            configuration.Setup(c => c["ELASTIC_EXCLUDED_FIELDS_KENNISBANK"]).Returns("VAC.toelichting,Kennisbank.vertalingen.deskMemo");

            var service = new ElasticsearchService(null!, (isKennisBank) => true, (isKcm) => true, null!, configuration.Object);

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

            service.ApplyRequestTransform(elasticQuery);

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
            var configuration = new Mock<IConfiguration>();
            configuration.Setup(c => c["ELASTIC_EXCLUDED_FIELDS_KENNISBANK"]).Returns("VAC.toelichting,Kennisbank.vertalingen.deskMemo");

            var service = new ElasticsearchService(null!, (isKennisBank) => true, (isKcm) => true, null!, configuration.Object);

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

            service.ApplyResponseTransform(responseBody);

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
            var configuration = new Mock<IConfiguration>();
            configuration.Setup(c => c["ELASTIC_EXCLUDED_FIELDS_KENNISBANK"]).Returns("VAC.toelichting,Kennisbank.vertalingen.deskMemo");

            var mockHttp = new MockHttpMessageHandler();
            var wasCalled = false;
            mockHttp.When(HttpMethod.Post, "https://elasticsearch.example.com/my-index/_search")
                .With(req =>
                {
                    wasCalled = true;
                    return true;
                })
                .Respond("application/json", JsonSerializer.Serialize(new
                {
                    hits = new { hits = Array.Empty<object>() }
                }));

            var httpClient = mockHttp.ToHttpClient();
            httpClient.BaseAddress = new Uri("https://elasticsearch.example.com");

            var service = new ElasticsearchService(httpClient, (isKennisBank) => true, (isKcm) => false, null!, configuration.Object);

            var elasticQuery = new JsonObject
            {
                ["query"] = new JsonObject
                {
                    ["term"] = new JsonObject { ["field"] = "value" }
                },
                ["size"] = 20
            };

            await service.Search("my-index/_search", elasticQuery, CancellationToken.None);

            Assert.IsTrue(wasCalled);
        }

        #endregion
    }
}
