using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kiss.Bff.Beheer.Data;
using Kiss.Bff.Beheer.Links.Controllers;
using Kiss.Bff.Beheer.Links.Data.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Kiss.Bff.Test
{
    [TestClass]
    public class CategorienControllerTests : TestHelper
    {

        [TestInitialize] 
        public void Initialize() 
        {
            InitializeDatabase();
        }

        [TestMethod]
        public void GetLinks_ReturnsOkResult_WithCategories()
        {
            // Arrange
            using var dbContext = new BeheerDbContext(_dbContextOptions);
            var controller = new CategorienController(dbContext);

            dbContext.Links.AddRange(
                new Link { Categorie = "Category 1" },
                new Link { Categorie = "Category 2" },
                new Link { Categorie = "Category 1" },
                new Link { Categorie = "Category 3" }
            );
            dbContext.SaveChanges();

            // Act
            var actionResult = controller.GetLinks();
            var okResult = actionResult as OkObjectResult;
            var categories = okResult?.Value as IEnumerable<string>;

            // Assert
            Assert.IsNotNull(okResult);
            Assert.IsNotNull(categories);
            Assert.AreEqual(3, categories.Count());
            CollectionAssert.Contains(categories.ToList(), "Category 1");
            CollectionAssert.Contains(categories.ToList(), "Category 2");
            CollectionAssert.Contains(categories.ToList(), "Category 3");
        }

        //[TestMethod]
        //public void GetLinks_ReturnsNotFoundResult_WhenLinksTableIsNull()
        //{
        //    // Arrange
        //    using var dbContext = new BeheerDbContext(_dbContextOptions);
        //    var controller = new CategorienController(dbContext);

        //    // Act
        //    var result = controller.GetLinks() as NotFoundResult;

        //    // Assert
        //    Assert.IsNotNull(result);
        //}
    }
}
