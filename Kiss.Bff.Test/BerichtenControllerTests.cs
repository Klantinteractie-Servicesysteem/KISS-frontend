using Kiss.Bff.Beheer.Data;
using Kiss.Bff.NieuwsEnWerkinstructies.Controllers;
using Kiss.Bff.NieuwsEnWerkinstructies.Data.Entities;
using Microsoft.AspNetCore.Mvc;


namespace Kiss.Bff.Test
{
    [TestClass]
    public class BerichtenControllerTests : TestHelper
    {
        [TestInitialize] 
        public void Initialize()
        {
            InitializeDatabase();
        }

        [TestMethod]
        public void GetBerichten_ReturnsOkResult_WithBerichtOverviewModels()
        {
            // Arrange
            using var dbContext = new BeheerDbContext(_dbContextOptions);
            var controller = new BerichtenController(dbContext);
            dbContext.Berichten.Add(new Bericht { Id = 1, Titel = "Bericht 1" });
            dbContext.Berichten.Add(new Bericht { Id = 2, Titel = "Bericht 2" });
            dbContext.SaveChanges();

            // Act
            var result = controller.GetBerichten();

            // Assert
            Assert.IsInstanceOfType(result.Result, typeof(OkObjectResult));
            var okResult = result.Result as OkObjectResult;
            Assert.IsInstanceOfType(okResult?.Value, typeof(IEnumerable<BerichtOverviewModel>));
            var overviewModels = okResult?.Value as IEnumerable<BerichtOverviewModel>;
            Assert.AreEqual(2, overviewModels?.Count());
            Assert.AreEqual("Bericht 1", overviewModels?.First().Titel);
            Assert.AreEqual("Bericht 2", overviewModels?.Last().Titel);
        }

        [TestMethod]
        public void GetBericht_ReturnsOkObjectResult_WithBerichtViewModel()
        {
            // Arrange
            using var dbContext = new BeheerDbContext(_dbContextOptions);
            var controller = new BerichtenController(dbContext);
            dbContext.Berichten.Add(new Bericht { Id = 1, Titel = "Bericht 1" });
            dbContext.SaveChanges();

            // Act
            var result = controller.GetBericht(1, new CancellationToken());

            // Assert
            Assert.IsInstanceOfType(result.Result, typeof(ActionResult<BerichtViewModel>));
            var okResult = result.Result;
            Assert.IsInstanceOfType(okResult.Value, typeof(BerichtViewModel));
            var viewModel = okResult.Value;
            Assert.AreEqual(1, viewModel?.Id);
            Assert.AreEqual("Bericht 1", viewModel?.Titel);
        }

        [TestMethod]
        public void PutBericht_ReturnsNoContentResult_WhenBerichtIsUpdated()
        {
            // Arrange
            using var dbContext = new BeheerDbContext(_dbContextOptions);
            var controller = new BerichtenController(dbContext);
            dbContext.Berichten.Add(new Bericht { Id = 1, Titel = "Bericht 1" });
            dbContext.SaveChanges();
            var berichtModel = new BerichtPutModel { Titel = "Updated Bericht" };

            // Act
            var result = controller.PutBericht(1, berichtModel, new CancellationToken());

            // Assert
            Assert.IsInstanceOfType(result.Result, typeof(NoContentResult));
        }

        [TestMethod]
        public void PostBericht_ReturnsCreatedAtActionResult_WithCreatedBerichtViewModel()
        {
            // Arrange
            using var dbContext = new BeheerDbContext(_dbContextOptions);
            var controller = new BerichtenController(dbContext);
            var berichtModel = new BerichtPostModel { Titel = "New Bericht" };

            // Act
            var result = controller.PostBericht(berichtModel, new CancellationToken());

            // Assert
            Assert.IsInstanceOfType(result.Result, typeof(ActionResult<BerichtViewModel>));
            var actionResult = result.Result;
            Assert.IsInstanceOfType(actionResult.Result, typeof(CreatedAtActionResult));
            var createdAtActionResult = actionResult.Result as CreatedAtActionResult;
            Assert.AreEqual(nameof(BerichtenController.GetBericht), createdAtActionResult?.ActionName);
            Assert.IsNotNull(createdAtActionResult?.RouteValues);
            Assert.AreEqual(1, createdAtActionResult?.RouteValues["id"]);
            Assert.IsInstanceOfType(createdAtActionResult?.Value, typeof(BerichtViewModel));
            var viewModel = createdAtActionResult?.Value as BerichtViewModel;
            Assert.AreEqual("New Bericht", viewModel?.Titel);
        }
        [TestMethod]
        public void DeleteBericht_ReturnsNoContentResult_WhenBerichtIsDeleted()
        {
            // Arrange
            using var dbContext = new BeheerDbContext(_dbContextOptions);
            var controller = new BerichtenController(dbContext);
            dbContext.Berichten.Add(new Bericht { Id = 1, Titel = "Bericht 1" });
            dbContext.SaveChanges();

            // Act
            var result = controller.DeleteBericht(1, new CancellationToken());

            // Assert
            Assert.IsInstanceOfType(result.Result, typeof(NoContentResult));
        }
    }
}
