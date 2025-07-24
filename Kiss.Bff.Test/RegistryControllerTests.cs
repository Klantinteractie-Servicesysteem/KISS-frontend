using Kiss.Bff.Beheer.Data;
using Kiss.Bff.Extern;
using Kiss.Bff.Intern.Environment;
using Kiss.Bff.Intern.Registry.Data;
using Kiss.Bff.NieuwsEnWerkinstructies.Data.Entities;
using Kiss.Bff.NieuwsEnWerkinstructies.Features;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Moq;
using Newtonsoft.Json;


namespace Kiss.Bff.Test
{
    [TestClass]
    public class RegistryControllerTests : TestHelper
    {
        [TestMethod]
        public void GetRegistrySystems_ReturnsExpectedSystems()
        {
            // Arrange
            var registryConfig = CreateRegistryConfigWithTwoRegistries();

            var afdelingUrl = "https://afdeling.nl";
            var groepenUrl = "https://groepen.nl";
            var vacObjectenUrl = "https://vacobjecten.nl";
            var vacObjectTypeUrl = "https://vacobjecttype.nl";
            var medewerkerObjectenUrl = "https://medewerkerobjecten.nl";
            var medewerkerObjectTypesUrl = "https://medewerkerobjecttypen.nl";

            var configurationMock = new Mock<IConfiguration>();
            configurationMock.Setup(cfg => cfg["AFDELINGEN_BASE_URL"]).Returns(afdelingUrl);
            configurationMock.Setup(cfg => cfg["GROEPEN_BASE_URL"]).Returns(groepenUrl);
            configurationMock.Setup(cfg => cfg["VAC_OBJECTEN_BASE_URL"]).Returns(vacObjectenUrl);
            configurationMock.Setup(cfg => cfg["VAC_OBJECT_TYPE_URL"]).Returns(vacObjectTypeUrl);
            configurationMock.Setup(cfg => cfg["MEDEWERKER_OBJECTEN_BASE_URL"]).Returns(medewerkerObjectenUrl);
            configurationMock.Setup(cfg => cfg["MEDEWERKER_OBJECTTYPES_BASE_URL"]).Returns(medewerkerObjectTypesUrl);

            var controller = new RegistryController(configurationMock.Object, registryConfig);

            // Act
            var result = controller.GetRegistrySystems();

            // Assert
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            var okResult = (OkObjectResult)result;

            // serialze from json to kissconnectionsmodel
            var kissConnectionsModel = JsonConvert.DeserializeObject<KissConnectionsModel>(JsonConvert.SerializeObject(okResult.Value));

            var registries = kissConnectionsModel?.Registries;

            Assert.IsNotNull(registries);
            Assert.AreEqual(2, registries.Count);

            Assert.AreEqual(registryConfig.Systemen[0].Identifier.ToString(), registries[0].Identifier.ToString());
            Assert.AreEqual(registryConfig.Systemen[1].Identifier.ToString(), registries[1].Identifier.ToString());

            Assert.AreEqual(true, (bool)registries[0].IsDefault);
            Assert.AreEqual(false, (bool)registries[1].IsDefault);

            Assert.AreEqual(afdelingUrl, kissConnectionsModel.AfdelingenBaseUrl);
            Assert.AreEqual(groepenUrl, kissConnectionsModel.GroepenBaseUrl);
            Assert.AreEqual(vacObjectenUrl, kissConnectionsModel.VacObjectenBaseUrl);
            Assert.AreEqual(vacObjectTypeUrl, kissConnectionsModel.VacObjectTypeUrl);
            Assert.AreEqual(medewerkerObjectenUrl, kissConnectionsModel.MedewerkerObjectenBaseUrl);
            Assert.AreEqual(medewerkerObjectTypesUrl, kissConnectionsModel.MedewerkerObjectTypeUrl);
        }

        private RegistryConfig CreateRegistryConfigWithTwoRegistries()
        {
            return new RegistryConfig
            {
                Systemen =
                [
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
                        },
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
                        },
                    }

                ]
            }
            ;
        }
    }
}
