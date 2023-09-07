using System.Net.Http.Json;
using Kiss.Bff.Beheer.Data;
using Kiss.Bff.Test.Config;
using Kiss.Bff.ZaakGerichtWerken.Contactmomenten;
using Microsoft.Extensions.DependencyInjection;

namespace Kiss.Bff.Test
{
    [TestClass]
    public class GetFaqTest
    {
        public static CustomWebApplicationFactory WebApplicationFactory { get; private set; } = new();

        [ClassCleanup]
        public static void Cleanup()
        {
            WebApplicationFactory?.Dispose();
        }

        [TestMethod]
        public async Task Test()
        {
            // ARRANGE
            var vraag1 = Guid.NewGuid().ToString();
            var vraag2 = Guid.NewGuid().ToString();

            var cm1 = CreateCm(vraag1);
            var cm2 = CreateCm(vraag1);
            var cm3 = CreateCm(vraag2);

            using (var scope = WebApplicationFactory.Services.CreateScope())
            {
                var ctx = scope.ServiceProvider.GetRequiredService<BeheerDbContext>();
                ctx.Add(cm1);
                ctx.Add(cm2);
                ctx.Add(cm3);
                ctx.SaveChanges();
            }

            using var client = WebApplicationFactory.CreateClient();

            // ACT
            var result = await client
                .AsKlantcontactmedewerker()
                .GetFromJsonAsync<string[]>("api/faq");

            // ASSERT
            Assert.IsNotNull(result);
            Assert.AreEqual(2, result.Length);
            Assert.AreEqual(vraag1, result[0]);
            Assert.AreEqual(vraag2, result[1]);
        }

        private static ContactmomentDetails CreateCm(string vraag) => new() { Id = Guid.NewGuid().ToString(), Vraag = vraag };
    }
}
