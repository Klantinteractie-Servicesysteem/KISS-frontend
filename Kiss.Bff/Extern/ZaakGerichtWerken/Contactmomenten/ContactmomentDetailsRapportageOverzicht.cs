﻿using Kiss.Bff.Beheer.Data;
using Kiss.Bff.ZaakGerichtWerken.Contactmomenten;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using System.Linq.Expressions;

namespace Kiss.Bff.Extern.ZaakGerichtWerken.Contactmomenten
{
    [ApiController]
    public class ContactmomentDetailsRapportageOverzicht : ControllerBase
    {
        private readonly BeheerDbContext _db;
        private const int MaxPageSize = 5000;

        public ContactmomentDetailsRapportageOverzicht(BeheerDbContext db)
        {
            _db = db;
        }

        [HttpGet("/api/contactmomentendetails")]
        [Authorize(Policy = Policies.ExternSysteemPolicy)]
        public async Task<IActionResult> Get(
            [FromQuery] string from,
            [FromQuery] string to,
            CancellationToken token,
            [FromQuery] int pageSize = 5000,
            [FromQuery] int page = 1)
        {
            if (!DateTime.TryParseExact(from, "yyyy-MM-ddTHH:mm:ssZ", CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out DateTime fromDate) ||
                !DateTime.TryParseExact(to, "yyyy-MM-ddTHH:mm:ssZ", CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out DateTime toDate))
            {
                return BadRequest("Invalid date format. Use ISO 8601 format (yyyy-MM-ddTHH:mm:ssZ).");
            }

            if (pageSize < 1 || pageSize > MaxPageSize)
            {
                return BadRequest($"Page size must be between 1 and {MaxPageSize}.");
            }

            Expression<Func<ContactmomentDetailsModel, bool>> dateRangeSelector = x => x.Startdatum >= fromDate && x.Startdatum <= toDate;

            var totalCount = await _db.ContactMomentDetails
                .Where(dateRangeSelector)
                .CountAsync(token);

            var contactmomenten = await _db.ContactMomentDetails
                .Where(dateRangeSelector)
                .OrderByDescending(x => x.Startdatum)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(token);

            string baseUrl = $"{Request.Scheme}://{Request.Host}{Request.Path}";
            string? next = (page * pageSize < totalCount)
                ? QueryHelpers.AddQueryString(baseUrl, new Dictionary<string, string?>
                {
                    ["from"] = from,
                    ["to"] = to,
                    ["pageSize"] = pageSize.ToString(),
                    ["page"] = (page + 1).ToString()
                })
                : null;

            string? previous = (page > 1)
                ? QueryHelpers.AddQueryString(baseUrl, new Dictionary<string, string?>
                {
                    ["from"] = from,
                    ["to"] = to,
                    ["pageSize"] = pageSize.ToString(),
                    ["page"] = (page - 1).ToString()
                })
                : null;

            var response = new
            {
                count = totalCount,
                next = next,
                previous = previous,
                results = contactmomenten
            };

            return Ok(response);
        }
    }
}
