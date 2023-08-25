using System.Text.Json;
using Kiss.Bff.Beheer.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Kiss.Bff.ZaakGerichtWerken.Contactverzoeken
{
    [ApiController]
    public class ReadContactverzoekenVragenSets : ControllerBase
    {
        private readonly BeheerDbContext _db;

        public ReadContactverzoekenVragenSets(BeheerDbContext db)
        {
            _db = db;
        }

        [HttpGet("/api/contactverzoekvragensets")]
        public async Task<IActionResult> Get(CancellationToken token)
        {
            var list = await _db.ContactVerzoekVragenSets.ToListAsync();

            if (list == null || !list.Any())
            {
                return NotFound();
            }

            return Ok(list);
        }
    }
}
