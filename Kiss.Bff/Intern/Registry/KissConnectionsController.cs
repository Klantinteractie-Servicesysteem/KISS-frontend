using Kiss.Bff.Extern;
using Kiss.Bff.Intern.Registry.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Kiss.Bff.Intern.Environment
{
    [Route("api/connections")]
    [ApiController]
    [Authorize(Policies.KcmOrRedactiePolicy)]
    public class KissConnectionsController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly RegistryConfig _registryConfig;

        public KissConnectionsController(IConfiguration configuration, RegistryConfig klantContactConfig)
        {
            _configuration = configuration;
            _registryConfig = klantContactConfig;
        }

        public IActionResult GetConnections()
        {
            var kissConnectionsModel = new KissConnectionsModel
            {
                AfdelingenBaseUrl = _configuration["AFDELINGEN_BASE_URL"],
                GroepenBaseUrl = _configuration["GROEPEN_BASE_URL"],
                VacObjectenBaseUrl = _configuration["VAC_OBJECTEN_BASE_URL"],
                VacObjectTypeUrl = _configuration["VAC_OBJECT_TYPE_URL"],
                MedewerkerObjectenBaseUrl = _configuration["MEDEWERKER_OBJECTEN_BASE_URL"],
                MedewerkerObjectTypeUrl = _configuration["MEDEWERKER_OBJECTTYPES_BASE_URL"],
                Registries = _registryConfig.Systemen.Select(system => new RegistryModel
                {
                    IsDefault = system.IsDefault,
                    RegistryVersion = system.RegistryVersion.ToString(),
                    KlantinteractieRegistry = system.KlantinteractieRegistry?.BaseUrl,
                    ZaaksysteemRegistry = system.ZaaksysteemRegistry?.BaseUrl,
                    KlantRegistry = system.KlantRegistry?.BaseUrl,
                    InterneTaakRegistry = system.InterneTaakRegistry?.BaseUrl,
                    ContactmomentRegistry = system.ContactmomentRegistry?.BaseUrl
                }).ToList()
            };
            return Ok(kissConnectionsModel);
        }
    }
}
