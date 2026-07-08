using Kiss.Bff.Beheer.Data;
using Kiss.Bff.Beheer.Gespreksresultaten.Data.Entities;
using Kiss.Bff.Config.Permissions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Kiss.Bff.Intern.Gespreksresultaten.Features
{
    [Route("api/[controller]")]
    [ApiController]
    public class GespreksresultatenController : ControllerBase
    {
        private readonly BeheerDbContext _context;

        public GespreksresultatenController(BeheerDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        [RequirePermission(RequirePermissionTo.gespreksresultatenread)]
        public ActionResult<IAsyncEnumerable<GespreksresultaatModel>> GetGespreksresultaten()
        {
            var result = _context
               .Gespreksresultaten
               .AsNoTracking()
               .OrderBy(x => x.Definitie)
               .Select(x => new GespreksresultaatModel(x.Id, x.Definitie))
               .AsAsyncEnumerable();

            return Ok(result);
        }

        [HttpGet("{id}")]
        [RequirePermission(RequirePermissionTo.gespreksresultatenread)]
        public async Task<ActionResult<GespreksresultaatModel>> GetGespreksresultaat(Guid id, CancellationToken token)
        {
            var gesprekresultaat = await _context.Gespreksresultaten
                .Where(x => x.Id == id)
                .Select(x => new GespreksresultaatModel(x.Id, x.Definitie))
                .FirstOrDefaultAsync(token);

            return gesprekresultaat == null
                ? NotFound()
                : gesprekresultaat;
        }

        [HttpPut("{id}")]
        [RequirePermission(RequirePermissionTo.gespreksresultatenbeheer)]
        public async Task<IActionResult> PutGespreksresultaat(Guid id, GespreksresultaatModel gespreksresultaat, CancellationToken token)
        {
            var entity = await _context.Gespreksresultaten.FirstOrDefaultAsync(x => x.Id == id, cancellationToken: token);
            if (entity == null) return NotFound();

            entity.DateUpdated = DateTimeOffset.UtcNow;
            entity.Definitie = gespreksresultaat.Definitie;

            await _context.SaveChangesAsync(token);
            return NoContent();
        }

        [HttpPost]
        [RequirePermission(RequirePermissionTo.gespreksresultatenbeheer)]
        public async Task<ActionResult<Gespreksresultaat>> PostGespreksresultaat(GespreksresultaatModel gespreksresultaat, CancellationToken token)
        {
            var entity = new Gespreksresultaat
            {
                DateCreated = DateTimeOffset.UtcNow,
                Definitie = gespreksresultaat.Definitie
            };

            await _context.Gespreksresultaten.AddAsync(entity, token);
            await _context.SaveChangesAsync(token);

            return CreatedAtAction(nameof(GetGespreksresultaat), new { id = entity.Id }, gespreksresultaat with { Id = entity.Id });
        }

        [HttpDelete("{id}")]
        [RequirePermission(RequirePermissionTo.gespreksresultatenbeheer)]
        public async Task<IActionResult> DeleteGespreksresultaat(Guid id, CancellationToken token)
        {
            var gespreksresultaat = await _context.Gespreksresultaten.FirstOrDefaultAsync(x => x.Id == id, cancellationToken: token);
            if (gespreksresultaat == null)
            {
                return NoContent();
            }

            _context.Gespreksresultaten.Remove(gespreksresultaat);
            await _context.SaveChangesAsync(token);

            return NoContent();
        }
    }

    public record GespreksresultaatModel(Guid Id, string Definitie);
}
