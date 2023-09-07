using Kiss.Bff.Test.Config;
using static System.Net.HttpStatusCode;

namespace Kiss.Bff.Test
{
    [TestClass]
    public class AuthorizationCheckTests
    {
        public static CustomWebApplicationFactory WebApplicationFactory { get; private set; } = new();

        [ClassCleanup]
        public static void Cleanup()
        {
            WebApplicationFactory?.Dispose();
        }

        [DataTestMethod]
        [DataRow("/api/postcontactmomenten", "post")]
        [DataRow("/api/internetaak/api/version/objects", "post")]
        [DataRow("/api/faq")]
        public async Task CheckEndpointReturnsNotAuthorized(string url, string method = "get")
        {
            using var request = new HttpRequestMessage(new(method), url);
            using var httpClient = WebApplicationFactory.CreateDefaultClient();
            using var response = await httpClient
                .SendAsync(request);
            Assert.AreEqual(Unauthorized, response.StatusCode);
        }
    }
}
