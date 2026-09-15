using System.Net;
using System.Security.Claims;
using System.Text.Json;
using Kiss.Bff.Extern.Pabc;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Protected;

namespace Kiss.Bff.Test
{
    [TestClass]
    public class PabcClientTests
    {
        private const string BaseUrl = "https://pabc.example.com";
        private Mock<ILogger<PabcClient>> _loggerMock = null!;

        [TestInitialize]
        public void Initialize()
        {
            _loggerMock = new Mock<ILogger<PabcClient>>();
        }

        [TestMethod]
        public async Task GetApplicationRolesPerEntityTypeAsync_ReturnsResponse_WhenPabcReturnsSuccess()
        {
            // Arrange
            var pabcResponse = new
            {
                results = new[]
                {
                    new
                    {
                        entityType = new { id = "zaaktype-1", name = "Melding", type = "zaaktype" },
                        applicationRoles = new[] { new { name = "klantcontactmedewerker-zaaktype-filter", application = "kiss" } }
                    },
                    new
                    {
                        entityType = new { id = "zaaktype-2", name = "Aanvraag", type = "zaaktype" },
                        applicationRoles = new[] { new { name = "behandelaar", application = "zac" } }
                    }
                }
            };

            var httpClient = CreateMockHttpClient(HttpStatusCode.OK, JsonSerializer.Serialize(pabcResponse));
            var client = new PabcClient(httpClient, _loggerMock.Object);
            var user = CreateUser("Medewerker");

            // Act
            var result = await client.GetApplicationRolesPerEntityTypeAsync(user);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsNotNull(result.Results);
            Assert.AreEqual(2, result.Results.Count);
            Assert.AreEqual("zaaktype-1", result.Results[0].EntityType?.Id);
            Assert.AreEqual("klantcontactmedewerker-zaaktype-filter", result.Results[0].ApplicationRoles[0].Name);
        }

        [TestMethod]
        public async Task GetApplicationRolesPerEntityTypeAsync_ReturnsNull_WhenUserHasNoRoles()
        {
            // Arrange
            var httpClient = CreateMockHttpClient(HttpStatusCode.OK, "{}");
            var client = new PabcClient(httpClient, _loggerMock.Object);
            var user = new ClaimsPrincipal(new ClaimsIdentity());

            // Act
            var result = await client.GetApplicationRolesPerEntityTypeAsync(user);

            // Assert
            Assert.IsNull(result);
        }

        [TestMethod]
        public async Task GetApplicationRolesPerEntityTypeAsync_ReturnsNull_WhenPabcReturnsError()
        {
            // Arrange
            var httpClient = CreateMockHttpClient(HttpStatusCode.InternalServerError, "");
            var client = new PabcClient(httpClient, _loggerMock.Object);
            var user = CreateUser("Medewerker");

            // Act
            var result = await client.GetApplicationRolesPerEntityTypeAsync(user);

            // Assert
            Assert.IsNull(result);
        }

        [TestMethod]
        public async Task GetKissApplicationRolesAsync_ReturnsUnscopedKissRoles_IgnoringOtherApplicationsAndScopedRoles()
        {
            // Arrange
            var pabcResponse = new
            {
                results = new object[]
                {
                    new
                    {
                        applicationRoles = new[]
                        {
                            new { name = "Redacteur", application = "kiss" },
                            new { name = "behandelaar", application = "zac" }
                        }
                    },
                    new
                    {
                        entityType = new { id = "zaaktype-1", name = "Melding", type = "zaaktype" },
                        applicationRoles = new[] { new { name = "Beheerder", application = "kiss" } }
                    }
                }
            };

            var httpClient = CreateMockHttpClient(HttpStatusCode.OK, JsonSerializer.Serialize(pabcResponse));
            var client = new PabcClient(httpClient, _loggerMock.Object);
            var user = CreateUser("Medewerker");

            // Act
            var result = await client.GetKissApplicationRolesAsync(user);

            // Assert
            CollectionAssert.AreEquivalent(new[] { "Redacteur" }, result.ToArray());
        }

        [TestMethod]
        public async Task GetKissApplicationRolesAsync_ReturnsEmptySet_WhenPabcReturnsError()
        {
            // Arrange
            var httpClient = CreateMockHttpClient(HttpStatusCode.InternalServerError, "");
            var client = new PabcClient(httpClient, _loggerMock.Object);
            var user = CreateUser("Medewerker");

            // Act
            var result = await client.GetKissApplicationRolesAsync(user);

            // Assert
            Assert.AreEqual(0, result.Count);
        }

        [TestMethod]
        public async Task GetKissApplicationRolesAsync_ReturnsEmptySet_WhenUserHasNoRoles()
        {
            // Arrange
            var httpClient = CreateMockHttpClient(HttpStatusCode.OK, "{}");
            var client = new PabcClient(httpClient, _loggerMock.Object);
            var user = new ClaimsPrincipal(new ClaimsIdentity());

            // Act
            var result = await client.GetKissApplicationRolesAsync(user);

            // Assert
            Assert.AreEqual(0, result.Count);
        }

        private static ClaimsPrincipal CreateUser(params string[] roles)
        {
            var claims = roles.Select(r => new Claim(ClaimTypes.Role, r)).ToList();
            var identity = new ClaimsIdentity(claims, "test");
            return new ClaimsPrincipal(identity);
        }

        private static HttpClient CreateMockHttpClient(HttpStatusCode statusCode, string content)
        {
            var handler = CreateMockHandler(statusCode, content);
            return new HttpClient(handler.Object) { BaseAddress = new Uri(BaseUrl + "/") };
        }

        private static Mock<HttpMessageHandler> CreateMockHandler(HttpStatusCode statusCode, string content)
        {
            var handler = new Mock<HttpMessageHandler>();
            handler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = statusCode,
                    Content = new StringContent(content)
                });
            return handler;
        }
    }
}
