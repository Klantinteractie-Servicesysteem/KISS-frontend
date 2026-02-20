using Kiss.Bff.Beheer.Data;
using Kiss.Bff.Beheer.Gespreksresultaten.Data.Entities;
using Kiss.Bff.Config.Permissions;
using Kiss.Bff.Intern.Kanalen.Data.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Kiss.Bff.Intern.Kanalen
{
    [Route("api/[controller]")]
    [ApiController]
    public class KanaalToevoegen : ControllerBase
    {
        private readonly BeheerDbContext _context;

        public KanaalToevoegen(BeheerDbContext context)
        {
            _context = context;
        }


        [HttpPost]
        [RequirePermission(RequirePermissionTo.kanalenbeheer)]
        public async Task<ActionResult<KanaalToevoegenModel>> Post(KanaalToevoegenModel model, CancellationToken token)
        {
            var entity = new Kanaal
            {
                DateCreated = DateTimeOffset.UtcNow,
                Naam = model.Naam
            };

            //de foutmelding bij een savechanges poging varieert afhanelijk van de gebruikte database technologie
            //derhalve een custom check om een zinvolle melding te kunnen retourneren
            //ook al is er geen 100% garantie dat de naam na deze check bij het opslaan nog steeds uniek is
            if (await _context.Kanalen.AnyAsync(x => x.Naam == model.Naam, token))
            {
                return Conflict("De naam van het kanaal moet uniek zijn");
            }

            await _context.Kanalen.AddAsync(entity, token);
            await _context.SaveChangesAsync(token);

            return NoContent();

        }

    }

    public record KanaalToevoegenModel(string Naam);
}
