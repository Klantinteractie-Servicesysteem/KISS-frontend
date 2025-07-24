using Kiss.Bff.Beheer.Data;
using Kiss.Bff.Extern;
using Kiss.Bff.Intern.Environment;
using Kiss.Bff.NieuwsEnWerkinstructies.Data.Entities;
using Kiss.Bff.NieuwsEnWerkinstructies.Features;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Newtonsoft.Json;


namespace Kiss.Bff.Test
{
    [TestClass]
    public class RegistryControllerTests : TestHelper
    {
        [TestInitialize] 
        public void Initialize()
        {
            InitializeDatabase();
        }

        [TestMethod]
        public void GetRegistrySystems_ReturnsExpectedSystems()
        {
            // Arrange
            RegistryConfig config = new RegistryConfig
            {
                Systemen = new List<RegistrySystem>
                {
                    new RegistrySystem
                    {
                        IsDefault = true,
                        Identifier = "test-system",
                        RegistryVersion = RegistryVersion.OpenKlant1,
                        KlantinteractieRegistry = new KlantinteractieRegistry
                        {
                            BaseUrl = "https://test-system.com/api",
                            ClientId = "client-id",
                            ClientSecret = "client-secret"
                        }
                    },
                    new RegistrySystem
                    {
                        IsDefault = false,
                        Identifier = "test-system2",
                        RegistryVersion = RegistryVersion.OpenKlant2,
                        KlantinteractieRegistry = new KlantinteractieRegistry
                        {
                            BaseUrl = "https://test-system2.com/api",
                            ClientId = "client-id-2",
                            ClientSecret = "client-secret-2"
                        }
                    }

                }
            };
            var controller = new RegistryController(config);

            // Act
            var result = controller.GetRegistrySystems();

            // Assert
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            var okResult = (OkObjectResult)result;

            // Convert anonymous object to JSON and then back to dynamic
            var json = JsonConvert.SerializeObject(okResult.Value);
            // Safely handle potential null value from JsonConvert.DeserializeObject
            dynamic? parsed = JsonConvert.DeserializeObject<dynamic>(json);
            if (parsed == null)
            {
                Assert.Fail("Deserialization returned null.");
            }

            var systemen = parsed.Systemen;
            Assert.AreEqual(2, systemen.Count);

            Assert.AreEqual(config.Systemen[0].Identifier.ToString(), systemen[0].Identifier.ToString());
            Assert.AreEqual(config.Systemen[1].Identifier.ToString(), systemen[1].Identifier.ToString());
            Assert.AreEqual(true, (bool)systemen[0].IsDefault);
            Assert.AreEqual(false, (bool)systemen[1].IsDefault);

            // check if secrets are excluded
            Assert.IsNull(systemen[0]["Token"]);
            Assert.IsNull(systemen[0]["ClientSecret"]);
        }
    }
}
