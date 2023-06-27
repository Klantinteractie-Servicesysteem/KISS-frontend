using Kiss.Bff.Beheer.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Kiss.Bff.Beheer.Verwerking
{
    [ApiController]
    public class GetVerwerkingsLogs : ControllerBase
    {
        private readonly BeheerDbContext _db;

        public GetVerwerkingsLogs(BeheerDbContext db)
        {
            _db = db;
        }

        [HttpGet("api/verwerkingslogs")]
        [Authorize(Policy = Policies.RedactiePolicy)]
        public IActionResult Get()
        {
            var data = _db.VerwerkingsLogs
                .OrderByDescending(x => x.InsertedAt)
                .Take(10000)
                .AsAsyncEnumerable();

            return Ok(data);
        }
    }
}
