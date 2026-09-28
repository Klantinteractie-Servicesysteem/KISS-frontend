using Kiss.Bff.Beheer.Data;
using Kiss.Bff.Intern.ContactmomentDetails.Features;
using ContactmomentDetails = Kiss.Bff.Intern.ContactmomentDetails.Data.Entities.ContactmomentDetails;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Kiss.Bff.Intern.ContactmomentDetails.Data.Entities;

namespace Kiss.Bff.Test
{
    [TestClass]
    public class WriteContactmomentDetailsTests : TestHelper
    {
        [TestInitialize]
        public void Initialize()
        {
          InitializeDatabase();
        }

        [TestCleanup]
        public void Cleanup()
        {
            using var dbContext = new BeheerDbContext(_dbContextOptions);
            dbContext.Database.EnsureDeleted();
            dbContext.Dispose();
        }

        [TestMethod]
        public async Task Post_ValidModel_ReturnsOk()
        {
            using var dbContext = new BeheerDbContext(_dbContextOptions);
            // Arrange
            var controller = new ContactmomentDetailsToevoegen(dbContext);
            var validModel = new ContactmomentDetailsModel
            {
                Id = "1",
                Startdatum = DateTime.Now,
                Einddatum = DateTime.Now.AddHours(1),
                Gespreksresultaat = "Result 1",
                Vraag = "Question 1",
                EmailadresKcm = "test@example.com", // User.GetEmail()
                GespreksId = "gesprek-1",
                Bronnen = new[] {new ContactmomentDetailsBronModel { Soort = "Soort", Titel = "Titel", Url = "Url" } }
            };

            // Act
            var result = await controller.Post(validModel, CancellationToken.None);

            // Assert
            Assert.IsNotNull(result);
            //Assert.AreEqual(200, result.StatusCode);

            // Check if the model is added to the database
            var addedEntity = await dbContext.ContactMomentDetails.Include(x=> x.Bronnen).FirstOrDefaultAsync();
            var addedBron = addedEntity?.Bronnen.FirstOrDefault();
            Assert.IsNotNull(addedEntity);
            Assert.IsNotNull(addedBron);
            Assert.AreEqual(validModel.Id, addedEntity.Id);
            Assert.AreEqual(validModel.Bronnen.First().Titel, addedBron.Titel);
            Assert.AreEqual(validModel.GespreksId, addedEntity.GespreksId);
        }


        [TestMethod]
        public async Task Post_ModelWithExistingId_UpdatesExistingEntity()
        {
            // Arrange
            using var dbContext = new BeheerDbContext(_dbContextOptions);
            var existingEntity = new ContactmomentDetails
            {
                Id = "1",
                Startdatum = DateTime.Now,
                Einddatum = DateTime.Now.AddHours(1),
                Gespreksresultaat = "Result 1",
                Vraag = "Question 1",
                EmailadresKcm = "test@example.com",
                GespreksId = "gesprek-1",
                Bronnen = new List<ContactmomentDetailsBron> { new ContactmomentDetailsBron
                {
                    Titel = "Titel1",
                    Soort = "Soort1",
                    Url = "Url1"
                } }
            };
            dbContext.ContactMomentDetails.Add(existingEntity);
            dbContext.SaveChanges();

            var controller = new ContactmomentDetailsToevoegen(dbContext);
            var model = new ContactmomentDetailsModel
            {
                Id = "1",
                Startdatum = DateTime.Now,
                Einddatum = DateTime.Now.AddHours(1),
                Gespreksresultaat = "Result 2",
                Vraag = "Question 2",
                EmailadresKcm = "test@example.com",
                GespreksId = "gesprek-2",
                Bronnen = new[] {new ContactmomentDetailsBronModel { Soort = "Soort2", Url = "Url2", Titel = "Titel2" } }
            };

            // Act
            var result = await controller.Post(model, CancellationToken.None) as OkResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(200, result.StatusCode);

            // Check if the entity was updated in the database
            var updatedEntity = await dbContext.ContactMomentDetails.Include(x=> x.Bronnen).FirstOrDefaultAsync(x=> x.Id == model.Id);
            var updatedBron = updatedEntity?.Bronnen?.FirstOrDefault();
            Assert.IsNotNull(updatedEntity);
            Assert.IsNotNull(updatedBron);
            Assert.AreEqual(model.Gespreksresultaat, updatedEntity.Gespreksresultaat);
            Assert.AreEqual(model.Bronnen.First().Titel, updatedBron.Titel);
            Assert.AreEqual(model.GespreksId, updatedEntity.GespreksId);
        }
    }
}



