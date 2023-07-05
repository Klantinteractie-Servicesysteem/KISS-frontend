using Kiss.Bff.Beheer.Data;
using System.Security.Claims;
using Microsoft.Extensions.Logging;
using Moq;
using static Kiss.Bff.Beheer.Verwerking.VerwerkingMiddleware;

namespace Kiss.Bff.Test
{
    [TestClass]
    public class VerwerkingMiddlewareUnitTests : TestHelper
    {

        [TestInitialize]
        public void Initialize()
        {
            InitializeDatabase();
        }


        [TestMethod]
        public async Task SendAsync_LogsHttpRequestToDatabase()
        {
            // Arrange
            var request = new HttpRequestMessage(HttpMethod.Post, "https://www.nu.nl");

            var user = new ClaimsPrincipal();
            var loggerMock = new Mock<ILogger<VerwerkingsHttpClientMiddleware>>();

            using var context = new BeheerDbContext(_dbContextOptions);
            var middleware = new VerwerkingsHttpClientMiddleware(context, user, loggerMock.Object);

            // Act
            var response = await middleware.SendAsync((r, _) => Task.FromResult(new HttpResponseMessage()), request, CancellationToken.None);

            // Assert
            Assert.IsTrue(response.IsSuccessStatusCode);

            var loggedRequest = context.VerwerkingsLogs.FirstOrDefault();
            Assert.AreEqual(request.Method.Method, loggedRequest?.Method);
            Assert.AreEqual(request.RequestUri?.AbsoluteUri, loggedRequest?.ApiEndpoint);
        }
    }
}
