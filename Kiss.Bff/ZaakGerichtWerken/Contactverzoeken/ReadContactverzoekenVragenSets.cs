﻿using System.Threading;
using Kiss.Bff.Beheer.Data;
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
        public async Task<IActionResult> Get(CancellationToken cancellationToken)
        {
            var contactVerzoekVragenSets = await _db.ContactVerzoekVragenSets.ToListAsync(cancellationToken);

            if (contactVerzoekVragenSets == null)
            {
                return Ok(new List<ContactVerzoekVragenSet>());
            }

            return Ok(contactVerzoekVragenSets);
        }

        [HttpGet("/api/contactverzoekvragenset/{id:int}")]
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
