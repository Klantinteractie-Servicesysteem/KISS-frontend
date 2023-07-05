using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kiss.Bff.Beheer.Data;
using Kiss.Bff.Beheer.Links.Controllers;
using Kiss.Bff.Beheer.Links.Data.Entities;
using Microsoft.AspNetCore.Mvc;
using static Kiss.Bff.Beheer.Links.Controllers.LinksController;

namespace Kiss.Bff.Test
{
    [TestClass]
    public class LinksControllerUnitTests : TestHelper
    {
        private LinksController? _controller;

        [TestInitialize]
        public void Initialize()
        {
            InitializeDatabase();
            _controller = new LinksController(new BeheerDbContext(_dbContextOptions));
        }

        //[TestMethod]
        //public void GetLinks_ReturnsOkResult_WhenLinksExist()
        //{
        //    // Arrange
        //    var dbContext = new BeheerDbContext(_dbContextOptions);
        //    dbContext.Links.Add(new Link { Id = 1, Titel = "Link 1", Categorie = "Category 1", Url = "https://example.com/1" });
        //    dbContext.SaveChanges();

        //    // Act
        //    var actionResult = _controller.GetLinks();
        //    var objectResult = actionResult.Result as ObjectResult;
        //    var resultLinks = objectResult.Value as IAsyncEnumerable<GetLinkModel>;

        //    // Assert
        //    Assert.IsNotNull(objectResult);
        //    Assert.IsNotNull(resultLinks);

        //    var linksList = new List<GetLinkModel>();
        //    resultLinks.GetAsyncEnumerator().MoveNextAsync().AsTask().Wait();
        //    var enumerator = resultLinks.GetAsyncEnumerator();
        //    while (enumerator.MoveNextAsync().AsTask().Result)
        //    {
        //        linksList.Add(enumerator.Current);
        //    }

        //    Assert.AreEqual(1, linksList.Count);
        //    Assert.AreEqual(1, linksList[0].Id);
        //    Assert.AreEqual("Link 1", linksList[0].Titel);
        //    Assert.AreEqual("Category 1", linksList[0].Categorie);
        //    Assert.AreEqual("https://example.com/1", linksList[0].Url);
        //}

        //[TestMethod]
        //public void GetLinks_ReturnsNotFoundResult_WhenLinksDoNotExist()
        //{
        //    // Act
        //    var actionResult = _controller.GetLinks();
        //    var notFoundResult = actionResult.Result as NotFoundResult;

        //    // Assert
        //    Assert.IsNotNull(notFoundResult);
        //}


        [TestMethod]
        public async Task GetLink_WithValidId_ReturnsLink()
        {
            // Arrange
            var link = new Link { Id = 1, Titel = "Link 1", Categorie = "Category 1", Url = "https://example.com/1" };

            using var context = new BeheerDbContext(_dbContextOptions);
            await context.Links.AddAsync(link);
            await context.SaveChangesAsync();

            var controller = new LinksController(context);

            // Act
            var result = await controller.GetLink(1);

            // Assert
            Assert.IsInstanceOfType(result.Value, typeof(Link));

            var returnedLink = result?.Value;
            Assert.AreEqual(link.Id, returnedLink?.Id);
            Assert.AreEqual(link.Titel, returnedLink?.Titel);
            Assert.AreEqual(link.Categorie, returnedLink?.Categorie);
            Assert.AreEqual(link.Url, returnedLink?.Url);
        }

        [TestMethod]
        public async Task GetLink_WithInvalidId_ReturnsNotFoundResult()
        {
            // Arrange
            using var context = new BeheerDbContext(_dbContextOptions);
            var controller = new LinksController(context);

            // Act
            var result = await controller.GetLink(1);

            // Assert
            Assert.IsInstanceOfType(result.Result, typeof(NotFoundResult));
        }

        [TestMethod]
        public async Task PutLink_WithValidId_ReturnsNoContent()
        {
            // Arrange
            var link = new Link { Id = 1, Titel = "Link 1", Categorie = "Category 1", Url = "https://example.com/1" };

            using var context = new BeheerDbContext(_dbContextOptions);
            await context.Links.AddAsync(link);
            await context.SaveChangesAsync();


            var updatedLink = new LinkPutModel
            {
                Id = 1,
                Titel = "Updated Link",
                Categorie = "Updated Category",
                Url = "https://example.com/updated"
            };


            var controller = new LinksController(context);

            // Act
            var result = await controller.PutLink(1, updatedLink, CancellationToken.None);

            // Assert
            Assert.IsInstanceOfType(result, typeof(NoContentResult));
    
            var updatedLinkEntity = await context.Links.FindAsync(1);
            Assert.IsNotNull(updatedLinkEntity);
            Assert.AreEqual(updatedLink.Titel, updatedLinkEntity.Titel);
            Assert.AreEqual(updatedLink.Categorie, updatedLinkEntity.Categorie);
            Assert.AreEqual(updatedLink.Url, updatedLinkEntity.Url);
            
        }

        [TestMethod]
        public async Task PutLink_WithInvalidId_ReturnsNotFoundResult()
        {
            // Arrange
            var link = new LinksController.LinkPutModel
            {
                Id = 1,
                Titel = "Link 1",
                Categorie = "Category 1",
                Url = "https://example.com/1"
            };

            using var context = new BeheerDbContext(_dbContextOptions);
            var controller = new LinksController(context);

            // Act
            var result = await controller.PutLink(1, link, CancellationToken.None);

            // Assert
            Assert.IsInstanceOfType(result, typeof(NotFoundResult));
        }

        [TestMethod]
        public async Task PostLink_WithValidLink_ReturnsCreatedResultWithLink()
        {
            // Arrange
            var link = new LinkPostModel
            {
                Titel = "New Link",
                Categorie = "New Category",
                Url = "https://example.com/new"
            };

            using var context = new BeheerDbContext(_dbContextOptions);
            var controller = new LinksController(context);

            // Act
            var result = await controller.PostLink(link);

            // Assert
            Assert.IsInstanceOfType(result.Result, typeof(CreatedAtActionResult));

            var createdAtActionResult = (CreatedAtActionResult?)result.Result;
            var createdLink = (Link?)createdAtActionResult?.Value;
            Assert.AreEqual(link.Titel, createdLink?.Titel);
            Assert.AreEqual(link.Categorie, createdLink?.Categorie);
            Assert.AreEqual(link.Url, createdLink?.Url);
        }

        [TestMethod]
        public async Task DeleteLink_WithValidId_ReturnsNoContent()
        {
            // Arrange
            var link = new Link { Id = 1, Titel = "Link 1", Categorie = "Category 1", Url = "https://example.com/1" };

            using var context = new BeheerDbContext(_dbContextOptions);
            await context.Links.AddAsync(link);
            await context.SaveChangesAsync();

            var controller = new LinksController(context);

            // Act
            var result = await controller.DeleteLink(1);

            // Assert
            Assert.IsInstanceOfType(result, typeof(NoContentResult));

            var deletedLink = await context.Links.FindAsync(1);
            Assert.IsNull(deletedLink);
        }

        [TestMethod]
        public async Task DeleteLink_WithInvalidId_ReturnsNotFoundResult()
        {
            // Arrange
            using var context = new BeheerDbContext(_dbContextOptions);
            var controller = new LinksController(context);

            // Act
            var result = await controller.DeleteLink(1);

            // Assert
            Assert.IsInstanceOfType(result, typeof(NotFoundResult));
        }
    }
}
