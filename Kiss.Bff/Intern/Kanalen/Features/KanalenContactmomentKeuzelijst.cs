using Kiss.Bff.Beheer.Data;
using Kiss.Bff.Config.Permissions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Kiss.Bff.Intern.Kanalen
{
    [Route("api/[controller]")]
    [ApiController]
    public class KanalenContactmomentKeuzelijst : ControllerBase
    {
        private readonly BeheerDbContext _context;

        public KanalenContactmomentKeuzelijst(BeheerDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        [RequirePermission(RequirePermissionTo.kanalenread)]
        public ActionResult<IAsyncEnumerable<KanalenKeuzelijstModel>> Get()
        {
            var result = _context
               .Kanalen
               .AsNoTracking()
               .OrderBy(x => x.Naam)
               .Select(x => new KanalenOverzichtModel(x.Id, x.Naam))
               .AsAsyncEnumerable();

            return Ok(result);
        }

    }

    public record KanalenKeuzelijstModel(Guid Id, string Naam);
}
