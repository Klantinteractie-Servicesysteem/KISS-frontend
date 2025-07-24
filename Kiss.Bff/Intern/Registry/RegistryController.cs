using Kiss.Bff.Extern;
using Kiss.Bff.Intern.Registry.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Kiss.Bff.Intern.Environment
{
    [Route("api/registry")]
    [ApiController]
    [Authorize(Policies.KcmOrRedactiePolicy)]
    public class RegistryController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly RegistryConfig _registryConfig;

        public RegistryController(IConfiguration configuration, RegistryConfig klantContactConfig)
        {
            _configuration = configuration;
            _registryConfig = klantContactConfig;
        }

        [HttpGet("all")]
        public IActionResult GetRegistrySystems()
        {
            var kissConnectionsModel = new
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
                    Identifier = system.Identifier,
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
