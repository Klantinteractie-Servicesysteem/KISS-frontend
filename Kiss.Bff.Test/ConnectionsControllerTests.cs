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
    public class ConnectionsControllerTests : TestHelper
    {
        [TestMethod]
        public void GetRegistrySystems_ReturnsExpectedSystems()
        {
            // Arrange
            var registryConfig = CreateRegistryConfigWithTwoRegistries();

            var afdelingUrl = "https://afdeling.nl";
            var afdelingObjectTypeUrl = "https://afdeling-objecttype.nl";
            var groepenUrl = "https://groepen.nl";
            var groepenObjectTypeUrl = "https://groepen-objecttype.nl";
            var vacObjectenUrl = "https://vacobjecten.nl";
            var vacObjectTypeUrl = "https://vacobjecttype.nl";
            var medewerkerObjectenUrl = "https://medewerkerobjecten.nl";
            var medewerkerObjectTypeUrl = "https://medewerkerobjecttypen.nl";
            var sdgObjectenUrl = "https://sdgobjecten.nl";
            var sdgObjectTypeUrl = "https://sdgobjecttype.nl";

            var configurationMock = new Mock<IConfiguration>();
            configurationMock.Setup(cfg => cfg["AFDELINGEN_BASE_URL"]).Returns(afdelingUrl);
            configurationMock.Setup(cfg => cfg["AFDELINGEN_OBJECT_TYPE_URL"]).Returns(afdelingObjectTypeUrl);
            configurationMock.Setup(cfg => cfg["GROEPEN_BASE_URL"]).Returns(groepenUrl);
            configurationMock.Setup(cfg => cfg["GROEPEN_OBJECT_TYPE_URL"]).Returns(groepenObjectTypeUrl);
            configurationMock.Setup(cfg => cfg["VAC_OBJECTEN_BASE_URL"]).Returns(vacObjectenUrl);
            configurationMock.Setup(cfg => cfg["VAC_OBJECT_TYPE_URL"]).Returns(vacObjectTypeUrl);
            configurationMock.Setup(cfg => cfg["MEDEWERKER_OBJECTEN_BASE_URL"]).Returns(medewerkerObjectenUrl);
            configurationMock.Setup(cfg => cfg["MEDEWERKER_OBJECT_TYPE_URL"]).Returns(medewerkerObjectTypeUrl);
            configurationMock.Setup(cfg => cfg["SDG_OBJECTEN_BASE_URL"]).Returns(sdgObjectenUrl);
            configurationMock.Setup(cfg => cfg["SDG_OBJECT_TYPE_URL"]).Returns(sdgObjectTypeUrl);

            var controller = new KissConnectionsController(configurationMock.Object, registryConfig);

            // Act
            var result = controller.GetConnections();

            // Assert
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            var okResult = (OkObjectResult)result;

            // serialze from json to kissconnectionsmodel
            var kissConnectionsModel = JsonConvert.DeserializeObject<KissConnectionsModel>(JsonConvert.SerializeObject(okResult.Value));

            var registries = kissConnectionsModel?.Registries;

            Assert.IsNotNull(registries);
            Assert.AreEqual(2, registries.Count);

            Assert.AreEqual(registryConfig.Systemen[0].KlantinteractieRegistry.BaseUrl.ToString(), registries[0].KlantinteractieRegistry.ToString());
            Assert.AreEqual(registryConfig.Systemen[1].KlantinteractieRegistry.BaseUrl.ToString(), registries[1].KlantinteractieRegistry.ToString());

            Assert.AreEqual(registryConfig.Systemen[0].InterneTaakRegistry.BaseUrl.ToString(), registries[0].InterneTaakRegistry.ToString());
            Assert.AreEqual(registryConfig.Systemen[1].InterneTaakRegistry.BaseUrl.ToString(), registries[1].InterneTaakRegistry.ToString());

            Assert.AreEqual(registryConfig.Systemen[0].KlantRegistry.BaseUrl.ToString(), registries[0].KlantRegistry.ToString());
            Assert.AreEqual(registryConfig.Systemen[1].KlantRegistry.BaseUrl.ToString(), registries[1].KlantRegistry.ToString());

            Assert.AreEqual(registryConfig.Systemen[0].ContactmomentRegistry.BaseUrl.ToString(), registries[0].ContactmomentRegistry.ToString());
            Assert.AreEqual(registryConfig.Systemen[1].ContactmomentRegistry.BaseUrl.ToString(), registries[1].ContactmomentRegistry.ToString());

            Assert.AreEqual(registryConfig.Systemen[0].ZaaksysteemRegistry.BaseUrl.ToString(), registries[0].ZaaksysteemRegistry.ToString());
            Assert.AreEqual(registryConfig.Systemen[1].ZaaksysteemRegistry.BaseUrl.ToString(), registries[1].ZaaksysteemRegistry.ToString());

            Assert.AreEqual(true, (bool)registries[0].IsDefault);
            Assert.AreEqual(false, (bool)registries[1].IsDefault);

            Assert.AreEqual(afdelingUrl, kissConnectionsModel.AfdelingenBaseUrl);
            Assert.AreEqual(afdelingObjectTypeUrl, kissConnectionsModel.AfdelingObjectTypeUrl);
            Assert.AreEqual(groepenUrl, kissConnectionsModel.GroepenBaseUrl);
            Assert.AreEqual(groepenObjectTypeUrl, kissConnectionsModel.GroepenObjectTypeUrl);
            Assert.AreEqual(vacObjectenUrl, kissConnectionsModel.VacObjectenBaseUrl);
            Assert.AreEqual(vacObjectTypeUrl, kissConnectionsModel.VacObjectTypeUrl);
            Assert.AreEqual(medewerkerObjectenUrl, kissConnectionsModel.MedewerkerObjectenBaseUrl);
            Assert.AreEqual(medewerkerObjectTypeUrl, kissConnectionsModel.MedewerkerObjectTypeUrl);
            Assert.AreEqual(sdgObjectenUrl, kissConnectionsModel.SdgObjectenBaseUrl);
            Assert.AreEqual(sdgObjectTypeUrl, kissConnectionsModel.SdgObjectTypeUrl);
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
                            BaseUrl = "https://test-system.com/klant",
                        },
                        ContactmomentRegistry = new ContactmomentRegistry
                        {
                            BaseUrl = "https://test-system.com/contact",
                        },
                        InterneTaakRegistry = new InternetaakRegistry
                        {
                            ObjectTypeUrl = "https://test-system.com/interne-taak/objecttype",
                            ObjectTypeVersion = "1.0",
                            BaseUrl = "https://test-system.com/interne-taak",
                        },
                        KlantRegistry = new KlantRegistry
                        {
                            BaseUrl = "https://test-system.com/klant",
                        },
                        ZaaksysteemRegistry = new ZaaksysteemRegistry
                        {
                            BaseUrl = "https://test-system.com/zaak",
                        },
                    },
                    new RegistrySystem
                    {
                        IsDefault = false,
                        Identifier = "test-system2",
                        RegistryVersion = RegistryVersion.OpenKlant2,
                        KlantinteractieRegistry = new KlantinteractieRegistry
                        {
                            BaseUrl = "https://test-system.com/klant2",
                        },
                        ContactmomentRegistry = new ContactmomentRegistry
                        {
                            BaseUrl = "https://test-system.com/contact2",
                        },
              InterneTaakRegistry = new InternetaakRegistry
                        {
                            ObjectTypeUrl = "https://test-system.com/interne-taak/objecttype",
                            ObjectTypeVersion = "1.0",
                            BaseUrl = "https://test-system.com/interne-taak2",
                        },
                        KlantRegistry = new KlantRegistry
                        {
                            BaseUrl = "https://test-system.com/klant2",
                        },
                        ZaaksysteemRegistry = new ZaaksysteemRegistry
                        {
                            BaseUrl = "https://test-system.com/zaak2",
                        },
                    }

                ]
            }
            ;
        }
    }
}
