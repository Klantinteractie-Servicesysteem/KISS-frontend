using Kiss.Bff.Beheer.Data;
using Kiss.Bff.Intern.ContactmomentDetails.Data.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Kiss.Bff.Intern.ContactmomentDetails.Features
{
    [ApiController]
    public class ContactmomentDetailsToevoegen : ControllerBase
    {
        private readonly BeheerDbContext _db;

        public ContactmomentDetailsToevoegen(BeheerDbContext db)
        {
            _db = db;
        }

        [HttpPut("/api/contactmomentdetails")]
        public async Task<IActionResult> Post(ContactmomentDetailsModel model, CancellationToken cancellationToken)
        {
            var entity = await _db.ContactMomentDetails.Include(x => x.Bronnen).FirstOrDefaultAsync(x => x.Id == model.Id, cancellationToken: cancellationToken);

            if (entity == null)
            {
                entity = new Data.Entities.ContactmomentDetails
                {
                    Id = model.Id,
                    Bronnen = new List<ContactmomentDetailsBron>()
                };
                await _db.AddAsync(entity, cancellationToken);
            }

            entity.Bronnen.Clear();
            entity.Einddatum = model.Einddatum;
            entity.EmailadresKcm = User.GetEmail();
            entity.Gespreksresultaat = model.Gespreksresultaat;
            entity.SpecifiekeVraag = model.SpecifiekeVraag;
            entity.Startdatum = model.Startdatum;
            entity.VerantwoordelijkeAfdeling = model.VerantwoordelijkeAfdeling;
            entity.Vraag = model.Vraag;
            entity.GespreksId = model.GespreksId;

            foreach (var b in model.Bronnen)
            {
                entity.Bronnen.Add(new ContactmomentDetailsBron
                {
                    Soort = b.Soort,
                    Titel = b.Titel,
                    Url = b.Url
                });
            }

            await _db.SaveChangesAsync(cancellationToken);

            return Ok();
        }
    }
}
