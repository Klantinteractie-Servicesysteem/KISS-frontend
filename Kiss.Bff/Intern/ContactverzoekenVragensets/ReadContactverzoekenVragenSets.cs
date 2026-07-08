using Kiss.Bff.Beheer.Data;
using Kiss.Bff.Config.Permissions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Kiss.Bff.Intern.ContactverzoekenVragensets
{
    [ApiController]
    [RequirePermission(RequirePermissionTo.contactformulierenread)]
    public class ReadContactverzoekenVragenSets : ControllerBase
    {
        private readonly BeheerDbContext _db;

        public ReadContactverzoekenVragenSets(BeheerDbContext db)
        {
            _db = db;
        }

        [HttpGet("api/contactverzoekvragensets")]
        public async Task<IActionResult> Get([FromQuery] string? soort, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(soort) || soort == "afdeling" || soort == "groep")
            {
                var contactVerzoekVragenSets = await _db.ContactVerzoekVragenSets
                .Where(x => string.IsNullOrEmpty(soort) || x.OrganisatorischeEenheidSoort == soort)
                .ToListAsync(cancellationToken);

                return Ok(contactVerzoekVragenSets);
            }

            return BadRequest();
        }

        [HttpGet("api/contactverzoekvragensets/{id:int}")]
        public async Task<IActionResult> GetById(int id, CancellationToken cancellationToken)
        {
            var contactVerzoekVragenSet = await _db.ContactVerzoekVragenSets.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

            if (contactVerzoekVragenSet == null)
            {
                return NotFound();
            }

            return Ok(contactVerzoekVragenSet);
        }

    }
}
