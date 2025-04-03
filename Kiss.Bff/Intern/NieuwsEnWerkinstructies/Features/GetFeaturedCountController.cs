using Kiss.Bff.Beheer.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Kiss.Bff.NieuwsEnWerkinstructies.Features
{
    [ApiController]
    [Authorize(Policies.KcmOrRedactiePolicy)]
    public class GetFeaturedCountController : ControllerBase
    {
        private readonly BeheerDbContext _context;

        public GetFeaturedCountController(BeheerDbContext context)
        {
            _context = context;
        }

        [HttpGet("api/berichten/featuredcount")]
        public async Task<ActionResult<FeaturedCount>> GetCount(CancellationToken token)
        {
            var userId = User.GetId();

            var count = await _context.Berichten
            .Where(x =>
                    (x.PublicatieDatum <= DateTimeOffset.UtcNow) &&
                    (!x.PublicatieEinddatum.HasValue || x.PublicatieEinddatum >= DateTimeOffset.UtcNow)
                )
                .Select(x => new
                {
                    x.IsBelangrijk,
                    x.Gelezen,
                    Datum = x.DateUpdated > x.PublicatieDatum
                        ? x.DateUpdated
                        : x.PublicatieDatum
                })
                .Where(x => x.IsBelangrijk && !x.Gelezen.Any(g => g.UserId == userId && g.GelezenOp >= x.Datum))
                .CountAsync(token);

            return new FeaturedCount(count);
        }
    }

    public record struct FeaturedCount(int Count);
}
