using Kiss.Bff.Beheer.Data;
using Kiss.Bff.Beheer.Gespreksresultaten.Controllers;
using Kiss.Bff.Beheer.Gespreksresultaten.Data.Entities;
using Microsoft.AspNetCore.Mvc;

namespace Kiss.Bff.Test
{
    [TestClass]
    public class GespreksresultatenUnitTests : TestHelper
    {
        [TestInitialize]
        public void Initialize()
        {
            InitializeDatabase();
        }

        [TestMethod]
        public async Task GetGespreksresultaten_ReturnsOkResultWithGespreksresultaten()
        {
            // Arrange
            var gespreksresultaten = new List<Gespreksresultaat>
            {
                new Gespreksresultaat { Id = Guid.NewGuid(), Definitie = "Resultaat 1" },
                new Gespreksresultaat { Id = Guid.NewGuid(), Definitie = "Resultaat 2" }
            };

            using var context = new BeheerDbContext(_dbContextOptions);
            await context.Gespreksresultaten.AddRangeAsync(gespreksresultaten);
            await context.SaveChangesAsync();


            var controller = new GespreksresultatenController(context);

            // Act
            var result = controller.GetGespreksresultaten();

            // Assert
            var okResult = result.Result as OkObjectResult;
            var count = 0;

            if (okResult?.Value is IAsyncEnumerable<GespreksresultaatModel> gespreksresultaatModels)
            {
                await foreach (var item in gespreksresultaatModels)
                {
                    count++;
                    if (count == 1)
                    {
                        Assert.AreEqual("Resultaat 1", item.Definitie);
                    }
                    else if (count == 2)
                    {
                        Assert.AreEqual("Resultaat 2", item.Definitie);
                    }
                }
            }

            Assert.AreEqual(2, count);
        }

        [TestMethod]
        public async Task GetGespreksresultaat_ReturnsGespreksresultaat()
        {
            // Arrange
            var gespreksresultaat = new Gespreksresultaat { Id = Guid.NewGuid(), Definitie = "Resultaat" };
            using var context = new BeheerDbContext(_dbContextOptions);
            await context.Gespreksresultaten.AddAsync(gespreksresultaat);
            await context.SaveChangesAsync();

            var controller = new GespreksresultatenController(context);

            // Act
            var result = await controller.GetGespreksresultaat(gespreksresultaat.Id, CancellationToken.None);

            // Assert
            Assert.AreEqual(gespreksresultaat.Id, result?.Value?.Id);
            Assert.AreEqual(gespreksresultaat.Definitie, result?.Value?.Definitie);
        }

        [TestMethod]
        public async Task GetGespreksresultaat_WithInvalidId_ReturnsNotFound()
        {
            // Arrange
            var invalidId = Guid.NewGuid();

            using var context = new BeheerDbContext(_dbContextOptions);
            var controller = new GespreksresultatenController(context);

            // Act
            var result = await controller.GetGespreksresultaat(invalidId, CancellationToken.None);

            // Assert
            Assert.IsInstanceOfType(result.Result, typeof(NotFoundResult));
        }

        [TestMethod]
        public async Task PutGespreksresultaat_WithValidId_ReturnsNoContent()
        {
            // Arrange
            var gespreksresultaatId = Guid.NewGuid();
            var gespreksresultaat = new Gespreksresultaat { Id = gespreksresultaatId, Definitie = "Resultaat" };
            var updatedDefinitie = "Updated Resultaat";

            using var context = new BeheerDbContext(_dbContextOptions);
            await context.Gespreksresultaten.AddAsync(gespreksresultaat);
            await context.SaveChangesAsync();
            var controller = new GespreksresultatenController(context);

            // Act
            var result = await controller.PutGespreksresultaat(gespreksresultaatId, new GespreksresultaatModel(gespreksresultaatId, updatedDefinitie), CancellationToken.None);

            // Assert
            Assert.IsInstanceOfType(result, typeof(NoContentResult));

            var updatedGespreksresultaat = await context.Gespreksresultaten.FindAsync(gespreksresultaatId);
            Assert.AreEqual(updatedDefinitie, updatedGespreksresultaat?.Definitie);
        }

        [TestMethod]
        public async Task PutGespreksresultaat_WithInvalidId_ReturnsNotFound()
        {
            // Arrange
            var invalidId = Guid.NewGuid();

            using var context = new BeheerDbContext(_dbContextOptions);
            var controller = new GespreksresultatenController(context);

            // Act
            var result = await controller.PutGespreksresultaat(invalidId, new GespreksresultaatModel(invalidId, "Resultaat"), CancellationToken.None);

            // Assert
            Assert.IsInstanceOfType(result, typeof(NotFoundResult));
        }

        [TestMethod]
        public async Task PostGespreksresultaat_ReturnsCreatedResponseWithGespreksresultaat()
        {
            // Arrange
            var gespreksresultaatModel = new GespreksresultaatModel(Guid.Empty, "Resultaat");

            using var context = new BeheerDbContext(_dbContextOptions);
            var controller = new GespreksresultatenController(context);

            // Act
            var result = await controller.PostGespreksresultaat(gespreksresultaatModel, CancellationToken.None);

            // Assert
            Assert.IsInstanceOfType(result.Result, typeof(CreatedAtActionResult));

            var createdAtActionResult = (CreatedAtActionResult?)result.Result;
            Assert.AreEqual(201, createdAtActionResult?.StatusCode);

            var gespreksresultaatResult = (GespreksresultaatModel?)createdAtActionResult?.Value;
            Assert.AreEqual(gespreksresultaatModel.Definitie, gespreksresultaatResult?.Definitie);
            Assert.AreNotEqual(Guid.Empty, gespreksresultaatResult?.Id);
        }

        [TestMethod]
        public async Task DeleteGespreksresultaat_WithValidId_ReturnsNoContent()
        {
            // Arrange
            var gespreksresultaatId = Guid.NewGuid();
            var gespreksresultaat = new Gespreksresultaat { Id = gespreksresultaatId, Definitie = "Resultaat" };

            using var context = new BeheerDbContext(_dbContextOptions);
            await context.Gespreksresultaten.AddAsync(gespreksresultaat);
            await context.SaveChangesAsync();
            var controller = new GespreksresultatenController(context);

            // Act
            var result = await controller.DeleteGespreksresultaat(gespreksresultaatId, CancellationToken.None);

            // Assert
            Assert.IsInstanceOfType(result, typeof(NoContentResult));

            var deletedGespreksresultaat = await context.Gespreksresultaten.FindAsync(gespreksresultaatId);
            Assert.IsNull(deletedGespreksresultaat);
        }

        [TestMethod]
        public async Task DeleteGespreksresultaat_WithInvalidId_ReturnsNoContent()
        {
            // Arrange
            var invalidId = Guid.NewGuid();

            using var context = new BeheerDbContext(_dbContextOptions);
            var controller = new GespreksresultatenController(context);

            // Act
            var result = await controller.DeleteGespreksresultaat(invalidId, CancellationToken.None);

            // Assert
            Assert.IsInstanceOfType(result, typeof(NoContentResult));
        }
    }
}
