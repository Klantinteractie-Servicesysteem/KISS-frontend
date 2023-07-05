using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using IdentityModel;
using Kiss.Bff.Beheer.Data;
using Kiss.Bff.NieuwsEnWerkinstructies.Controllers;
using Kiss.Bff.NieuwsEnWerkinstructies.Data.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace Kiss.Bff.Test
{
    [TestClass]
    public class MarkeerGelezenControllerTests : TestHelper
    {
        [TestInitialize]
        public void Initialize()
        {
            InitializeDatabase();
        }

        [TestMethod]
        public async Task MarkeerGelezen_ExistingEntity_UpdatesGelezenOp()
        {
            using var dbContext = new BeheerDbContext(_dbContextOptions);
            var userId = "testuser";
            var berichtId = 1;
            var existingGelezen = new BerichtGelezen { BerichtId = berichtId };
            dbContext.Gelezen.Add(existingGelezen);
            dbContext.SaveChanges();

            var controller = new MarkeerGelezenController(dbContext);
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
                    {
                        new Claim("http://schemas.microsoft.com/identity/claims/objectidentifier", userId)
                    }))
                }
            };

            // Act
            var model = new MarkeerGelezenModel(true);
            var result = await controller.MarkeerGelezen(berichtId, model, new CancellationToken());

            // Assert
            Assert.IsInstanceOfType(result, typeof(OkResult));
            var updatedEntity = await dbContext.Gelezen.FindAsync(userId, berichtId);
            Assert.IsNotNull(updatedEntity);
            Assert.IsTrue(updatedEntity.GelezenOp > DateTimeOffset.MinValue);
        }

       
        [TestMethod]
        public async Task MarkeerGelezen_ExistingEntity_IsGelezenFalse_RemovesGelezen()
        {
            // Arrange
            using var dbContext = new BeheerDbContext(_dbContextOptions);
            var userId = "testuser";
            var berichtId = 1;
            var existingGelezen = new BerichtGelezen { UserId = userId, BerichtId = berichtId };
            dbContext.Gelezen.Add(existingGelezen);
            dbContext.SaveChanges();

            var controller = new MarkeerGelezenController(dbContext);
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
                   {
                        new Claim("http://schemas.microsoft.com/identity/claims/objectidentifier", userId)
                   }))
                }
            };

            // Act
            var model = new MarkeerGelezenModel(false);
            var result = await controller.MarkeerGelezen(berichtId, model, new CancellationToken());

            // Assert
            Assert.IsInstanceOfType(result, typeof(OkResult));
            var removedEntity = await dbContext.Gelezen.FindAsync(userId, berichtId);
            Assert.IsNull(removedEntity);
        }
    }
}
