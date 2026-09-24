using Kiss.Bff.Beheer.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Kiss.Bff.Intern.ContactmomentDetails.Features
{
    [ApiController]
    public class ReadContactmomentDetails : ControllerBase
    {
        private readonly BeheerDbContext _db;

        public ReadContactmomentDetails(BeheerDbContext db)
        {
            _db = db;
        }

        [HttpGet("/api/contactmomentdetails")]
        public async Task<IActionResult> Get([FromQuery] string id, CancellationToken token)
        {
            var contactmoment = await _db.ContactMomentDetails
                .Where(x => x.Id == id)
                .Select(x => new ContactmomentDetailsDetailModel
                {
                    Id = x.Id,
                    Einddatum = x.Einddatum,
                    EmailadresKcm = x.EmailadresKcm,
                    Gespreksresultaat = x.Gespreksresultaat,
                    SpecifiekeVraag = x.SpecifiekeVraag,
                    Startdatum = x.Startdatum,
                    VerantwoordelijkeAfdeling = x.VerantwoordelijkeAfdeling,
                    Vraag = x.Vraag,
                    GespreksId = x.GespreksId
                })
                .FirstOrDefaultAsync(token);

            if (contactmoment == null)
            {
                return NotFound();
            }

            return Ok(contactmoment);
        }
    }

}
