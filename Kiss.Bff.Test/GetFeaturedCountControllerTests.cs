using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Kiss.Bff.Beheer.Data;
using Kiss.Bff.NieuwsEnWerkinstructies.Controllers;
using Kiss.Bff.NieuwsEnWerkinstructies.Data.Entities;
using Kiss.Bff.NieuwsEnWerkinstructies.Migrations;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace Kiss.Bff.Test
{
    [TestClass]
    public class GetFeaturedCountControllerTests : TestHelper
    {
        [TestInitialize]
        public void Initialize()
        {
            InitializeDatabase();
        }

        [TestMethod]
        public async Task GetCount_ReturnsActionResult_WithFeaturedCount()
        {
            // Arrange
            var berichten = new List<Bericht>
        {
            new Bericht { IsBelangrijk = true },
            new Bericht { IsBelangrijk = true },
                new Bericht { IsBelangrijk = false }
            };

            using var dbContext = new BeheerDbContext(_dbContextOptions);
            dbContext.Berichten.AddRange(berichten);
            dbContext.SaveChanges();

            var controller = new GetFeaturedCountController(dbContext);

            // Act
            var result = await controller.GetCount(new CancellationToken());

            // Assert
            Assert.IsInstanceOfType(result, typeof(ActionResult<FeaturedCount>));
            Assert.IsInstanceOfType(result.Value, typeof(FeaturedCount));
            var featuredCount = result.Value;
            Assert.AreEqual(2, featuredCount.Count);
        }
    }
}
