using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Kiss.Bff.Beheer.Data;
using Kiss.Bff.NieuwsEnWerkinstructies.Controllers;
using Kiss.Bff.NieuwsEnWerkinstructies.Data.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Kiss.Bff.Test
{
    [TestClass]
    public class SearchBerichtenControllerTests : TestHelper
    {
        [TestInitialize]
        public void Initialize()
        {
            InitializeDatabase();
        }

        [TestMethod]
        public async Task GetBerichten_ReturnsSearchResults()
        {
            // Arrange
            var dbContextOptions = new DbContextOptionsBuilder<BeheerDbContext>()
                .UseInMemoryDatabase(databaseName: "TestDatabase")
                .Options;

            using var dbContext = new BeheerDbContext(dbContextOptions);
            var controller = new SearchBerichtenController(dbContext);

            // Mock User for the controller
            var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
        new Claim(ClaimTypes.NameIdentifier, "1"), // Replace "1" with the desired user ID
            }, "mock"));

            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = user
                }
            };

            // Seed test data
            var berichten = new List<Bericht>
    {
        new Bericht
        {
            Id = 1,
            Inhoud = "Test Bericht 1",
            IsBelangrijk = true,
            PublicatieDatum = new DateTimeOffset(2022, 1, 1, 0, 0, 0, TimeSpan.Zero),
            DateUpdated = new DateTimeOffset(2022, 1, 1, 0, 0, 0, TimeSpan.Zero),
            Titel = "Bericht 1",
            Type = "Type A",
        },
        new Bericht
        {
            Id = 2,
            Inhoud = "Test Bericht 2",
            IsBelangrijk = false,
            PublicatieDatum = new DateTimeOffset(2022, 1, 2, 0, 0, 0, TimeSpan.Zero),
            DateUpdated = new DateTimeOffset(2022, 1, 2, 0, 0, 0, TimeSpan.Zero),
            Titel = "Bericht 2",
            Type = "Type B",
        }
    };

            await dbContext.Berichten.AddRangeAsync(berichten);
            await dbContext.SaveChangesAsync();

            // Act
            var filterModel = new BerichtFilterModel("Type A", null, null, null, null);
            var token = new CancellationToken();
            var result = await controller.GetBerichten(filterModel, token) as ObjectResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(200, result.StatusCode);

            var response = result.Value as IEnumerable<SearchBerichtenResponseModel>;
            Assert.IsNotNull(response);
            Assert.AreEqual(1, response.Count());

            var bericht = response.First();
            Assert.AreEqual(1, bericht.Id);
            Assert.AreEqual("/api/berichten/1", bericht.Url);
            Assert.AreEqual("Test Bericht 1", bericht.Inhoud);
            Assert.IsTrue(bericht.IsBelangrijk);
            Assert.IsFalse(bericht.IsGelezen);
            Assert.AreEqual(new DateTimeOffset(2022, 1, 1, 0, 0, 0, TimeSpan.Zero), bericht.PublicatieDatum);
            Assert.AreEqual(new DateTimeOffset(2022, 1, 1, 0, 0, 0, TimeSpan.Zero), bericht.WijzigingsDatum);
            Assert.AreEqual("Bericht 1", bericht.Titel);
            Assert.AreEqual("Type A", bericht.Type);
        }
    }
}
