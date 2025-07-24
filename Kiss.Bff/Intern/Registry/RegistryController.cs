using System.Reflection;
using Kiss.Bff.Extern;
using Microsoft.AspNetCore.Mvc;

namespace Kiss.Bff.Intern.Environment
{
    [Route("api/registry")]
    [ApiController]
    public class RegistryController : ControllerBase
    {
        private readonly RegistryConfig _registryConfig;

        public RegistryController(RegistryConfig klantContactConfig)
        {
            _registryConfig = klantContactConfig;
        }

        [HttpGet("all")]
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
                    })
            };

            return Ok(model);
        }
    }
}
