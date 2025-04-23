using Kiss.Bff.Beheer.Data;
using Kiss.Bff.NieuwsEnWerkinstructies.Data.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace Kiss.Bff.NieuwsEnWerkinstructies.Features
{
    [ApiController]
    [Authorize(Policies.KcmOrRedactiePolicy)]
    public class MarkeerGelezenController : ControllerBase
    {
        private readonly BeheerDbContext _context;

        public MarkeerGelezenController(BeheerDbContext context)
        {
            _context = context;
        }

        [HttpPut("api/berichten/{id}/read")]
        public async Task<IActionResult> MarkeerGelezen([FromRoute] int id, [FromBody] MarkeerGelezenModel model, CancellationToken token)
        {
            var userId = User.GetId();

            var entity = await _context.Gelezen.Where(x => x.UserId == userId && x.BerichtId == id).FirstOrDefaultAsync(token);

            if (entity == null)
            {
                if (!model.IsGelezen)
                {
                    return Ok();
                }

                entity = new BerichtGelezen
                {
                    BerichtId = id,
                    UserId = userId,
                };
                await _context.Gelezen.AddAsync(entity, token);
            }

            if (!model.IsGelezen)
            {
                _context.Gelezen.Remove(entity);
            }
            else
            {
                entity.GelezenOp = DateTimeOffset.UtcNow;
            }

            try
            {
                await _context.SaveChangesAsync(token);
                return Ok();
            }
            catch (NpgsqlException ex) when (ex.SqlState == PostgresErrorCodes.ForeignKeyViolation)
            {
                return NotFound();
            }
        }
    }

    public record MarkeerGelezenModel(bool IsGelezen);
}

