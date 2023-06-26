using System.ComponentModel.DataAnnotations;
using System.Web;
using Kiss.Bff.Beheer.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Kiss.Bff.ZaakGerichtWerken.Contactmomenten
{
    [ApiController]
    [Route("api/contactmomenten/contactmomenten/api/v1/klantcontactmomenten")]
    public class KoppelKlantContactmomentWorkaround : ControllerBase
    {
        private readonly BeheerDbContext _db;

        public KoppelKlantContactmomentWorkaround(BeheerDbContext db)
        {
            _db = db;
        }

        [HttpGet]
        public async Task<ActionResult<ZgwPagination<KlantContactmoment>>> Get([FromQuery] KlantContactmomentQuery model, CancellationToken token)
        {
            var dbQuery = _db.KlantContactmomenten.Where(x => x.Klant == model.Klant);
            var count = await dbQuery.CountAsync(token);
            const int PageSize = 500;
            var page = model.Page.HasValue
                ? Math.Max(1, model.Page.Value)
                : 1;
            var skip = (page - 1) * PageSize;
            var results = await dbQuery.OrderBy(x => x.Contactmoment).Skip(skip).Take(PageSize).ToListAsync(token);

            var currentUri = new Uri($"{Request.Scheme}://{Request.Host}{Request.Path}{Request.QueryString}");
            var queryString = HttpUtility.ParseQueryString(currentUri.Query);

            string? next = null;

            if (skip + results.Count < count)
            {
                queryString["page"] = (page + 1).ToString();
                next = new UriBuilder(currentUri)
                {
                    Query = queryString.ToString()
                }.Uri.ToString();
            }

            string? previous = null;

            if (page > 1)
            {
                queryString["page"] = (page - 1).ToString();
                previous = new UriBuilder(currentUri)
                {
                    Query = queryString.ToString()
                }.Uri.ToString();
            }

            return new ZgwPagination<KlantContactmoment>
            {
                Previous = previous,
                Next = next,
                Count = count,
                Results = results
            };
        }

        [HttpPost]
        public async Task Post([FromBody] KlantContactmoment model, CancellationToken token)
        {
            await _db.AddAsync(model, token);

            try
            {
                await _db.SaveChangesAsync(token);
            }
            catch (DbUpdateException)
            {
                _db.Entry(model).State = EntityState.Modified;
                await _db.SaveChangesAsync(token);
            }
        }
    }

    public class KlantContactmomentQuery
    {
        [Required]
        public string Klant { get; set; } = default!;
        public int? Page { get; set; }
    }

    public class ZgwPagination<T>
    {
        public int Count { get; set; }
        public string? Next { get; set; }
        public string? Previous { get; set; }
        public IEnumerable<T> Results { get; set; } = Enumerable.Empty<T>();
    }

    public class KlantContactmoment
    {
        [Required]
        public string Klant { get; set; } = default!;
        [Required]
        public string Contactmoment { get; set; } = default!;
        [Required]
        public string Rol { get; set; } = default!;
    }
}
