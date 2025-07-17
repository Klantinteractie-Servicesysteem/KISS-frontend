using System.Reflection;
using Kiss.Bff.Extern;
using Microsoft.AspNetCore.Mvc;

namespace Kiss.Bff.Intern.Environment
{
    [Route("api/environment")]
    [ApiController]
    public class EnvironmentController : ControllerBase
    {
        private readonly RegistryConfig _registryConfig;
        private readonly IConfiguration _configuration;

        public EnvironmentController(RegistryConfig klantContactConfig, IConfiguration configuration)
        {
            _registryConfig = klantContactConfig;
            _configuration = configuration;
        }

        [HttpGet("use-vacs")]
        public IActionResult GetUseVacs()
        {
            return bool.TryParse(_configuration["USE_VACS"] ?? "false", out var useVacs) ?
                (IActionResult)Ok(new { useVacs }) : Ok(new { useVacs = false });
        }

        [HttpGet("use-medewerkeremail")]
        public IActionResult GetUseMedewerkerEmail()
        {
            return bool.TryParse(_configuration["USE_MEDEWERKEREMAIL"] ?? "false", out var useMedewerkeremail) ?
                (IActionResult)Ok(new { useMedewerkeremail }) : Ok(new { useMedewerkeremail = false });
        }

        [HttpGet("build-info")]
        public IActionResult GetBuildInfo()
        {
            var versionInfo = Assembly.GetExecutingAssembly()
                .GetCustomAttribute<AssemblyInformationalVersionAttribute>()?
                .InformationalVersion;

            return Ok(new { versionInfo });
        }

        [HttpGet("registers")]
        public IActionResult GetRegistrySystems()
        {
            var model = new
            {
                Systemen = _registryConfig.Systemen
                    // don't expose secrets
                    .Select((x) => new
                    {
                        x.IsDefault,
                        x.Identifier,
                        x.RegistryVersion,
                        x.ZaaksysteemRegistry?.DeeplinkUrl,
                        x.ZaaksysteemRegistry?.DeeplinkProperty
                    })
            };

            return Ok(model);
        }
    }
}
