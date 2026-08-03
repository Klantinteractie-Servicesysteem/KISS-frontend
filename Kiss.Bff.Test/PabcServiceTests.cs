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
    public class PabcServiceTests
    {
        private PabcConfig _config = null!;
        private Mock<ILogger<PabcService>> _loggerMock = null!;

        [TestInitialize]
        public void Initialize()
        {
            _config = new PabcConfig
            {
                BaseUrl = "https://pabc.example.com",
                ApiKey = "test-api-key"
            };
            _loggerMock = new Mock<ILogger<PabcService>>();
        }

        [TestMethod]
        public async Task GetAllowedZaaktypenAsync_ReturnsAllowedZaaktypen_WhenPabcReturnsMatchingRoles()
        {
            // Arrange
            var pabcResponse = new
            {
                results = new[]
                {
                    new
                    {
                        entityType = new { id = "zaaktype-1", name = "Melding", type = "zaaktype" },
                        applicationRoles = new[] { new { name = "klantcontactmedewerker", application = "kiss" } }
                    },
                    new
                    {
                        entityType = new { id = "zaaktype-2", name = "Aanvraag", type = "zaaktype" },
                        applicationRoles = new[] { new { name = "klantcontactmedewerker", application = "kiss" } }
                    },
                    new
                    {
                        entityType = new { id = "zaaktype-3", name = "Other", type = "zaaktype" },
                        applicationRoles = new[] { new { name = "behandelaar", application = "zac" } }
                    }
                }
            };

            var httpClient = CreateMockHttpClient(HttpStatusCode.OK, JsonSerializer.Serialize(pabcResponse));
            var service = new PabcService(httpClient, _config, _loggerMock.Object);
            var user = CreateUser("Medewerker");

            // Act
            var result = await service.GetAllowedZaaktypenAsync(user);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(2, result.Count);
            Assert.IsTrue(result.Contains("zaaktype-1"));
            Assert.IsTrue(result.Contains("zaaktype-2"));
            Assert.IsFalse(result.Contains("zaaktype-3"));
        }

        [TestMethod]
        public async Task GetAllowedZaaktypenAsync_ReturnsEmptySet_WhenUserHasNoRoles()
        {
            // Arrange
            var httpClient = CreateMockHttpClient(HttpStatusCode.OK, "{}");
            var service = new PabcService(httpClient, _config, _loggerMock.Object);
            var user = new ClaimsPrincipal(new ClaimsIdentity());

            // Act
            var result = await service.GetAllowedZaaktypenAsync(user);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(0, result.Count);
        }

        [TestMethod]
        public async Task GetAllowedZaaktypenAsync_ReturnsEmptySet_WhenPabcReturnsError()
        {
            // Arrange
            var httpClient = CreateMockHttpClient(HttpStatusCode.InternalServerError, "");
            var service = new PabcService(httpClient, _config, _loggerMock.Object);
            var user = CreateUser("Medewerker");

            // Act
            var result = await service.GetAllowedZaaktypenAsync(user);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(0, result.Count);
        }

        [TestMethod]
        public async Task GetAllowedZaaktypenAsync_IgnoresNonZaaktypeEntities()
        {
            // Arrange
            var pabcResponse = new
            {
                results = new[]
                {
                    new
                    {
                        entityType = new { id = "doc-1", name = "Document", type = "documenttype" },
                        applicationRoles = new[] { new { name = "klantcontactmedewerker", application = "kiss" } }
                    }
                }
            };

            var httpClient = CreateMockHttpClient(HttpStatusCode.OK, JsonSerializer.Serialize(pabcResponse));
            var service = new PabcService(httpClient, _config, _loggerMock.Object);
            var user = CreateUser("Medewerker");

            // Act
            var result = await service.GetAllowedZaaktypenAsync(user);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(0, result.Count);
        }

        private static ClaimsPrincipal CreateUser(params string[] roles)
        {
            var claims = roles.Select(r => new Claim(ClaimTypes.Role, r)).ToList();
            var identity = new ClaimsIdentity(claims, "test");
            return new ClaimsPrincipal(identity);
        }

        private HttpClient CreateMockHttpClient(HttpStatusCode statusCode, string content)
        {
            var handler = CreateMockHandler(statusCode, content);
            return new HttpClient(handler.Object) { BaseAddress = new Uri(_config.BaseUrl + "/") };
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
