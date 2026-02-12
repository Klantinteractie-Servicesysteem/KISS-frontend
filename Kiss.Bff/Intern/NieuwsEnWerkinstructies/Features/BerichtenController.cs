using System.ComponentModel.DataAnnotations;
using Kiss.Bff.Beheer.Data;
using Kiss.Bff.Config.Permissions;
using Kiss.Bff.NieuwsEnWerkinstructies.Data.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Kiss.Bff.NieuwsEnWerkinstructies.Features
{
    [Route("api/[controller]")]
    [ApiController]
    public class BerichtenController : ControllerBase
    {
        private readonly BeheerDbContext _context;

        public BerichtenController(BeheerDbContext context)
        {
            _context = context;
        }

        // GET: api/Berichten
        [HttpGet]
        [RequirePermission(RequirePermissionTo.berichtenread)]
        public ActionResult<IAsyncEnumerable<BerichtOverviewModel>> GetBerichten()
        {
            if (_context.Berichten == null)
            {
                return NotFound();
            }

            var result = _context
                .Berichten
                .OrderByDescending(x => x.DateUpdated ?? x.DateCreated)
                .Select(x => new BerichtOverviewModel
                {
                    Id = x.Id,
                    Titel = x.Titel,
                    DateCreated = x.DateCreated,
                    DateUpdated = x.DateUpdated,
                    Type = x.Type,
                    Publicatiedatum = x.PublicatieDatum
                })
                .AsAsyncEnumerable();

            return Ok(result);
        }

        // GET: api/Berichten/5
        [HttpGet("{id:int}")]
        [RequirePermission(RequirePermissionTo.berichtenread)]
        public async Task<ActionResult<BerichtViewModel>> GetBericht(int id, CancellationToken token)
        {
            if (_context.Berichten == null)
            {
                return NotFound();
            }

            var bericht = await _context.Berichten
                .Include(x => x.Skills.Where(s => !s.IsDeleted))
                .FirstOrDefaultAsync(x => x.Id == id, cancellationToken: token);

            if (bericht == null)
            {
                return NotFound();
            }

            return MapBericht(bericht);
        }



        // PUT: api/Berichten/5      
        [HttpPut("{id}")]
        [RequirePermission(RequirePermissionTo.berichtenbeheer)]
        public async Task<IActionResult> PutBericht(int id, BerichtPutModel bericht, CancellationToken token)
        {
            var current = await _context.Berichten.Include(x => x.Skills).FirstOrDefaultAsync(x => x.Id == id, token);

            if (current == null)
            {
                return NotFound();
            }

            current.Titel = bericht.Titel;
            current.Type = bericht.Type;
            current.Inhoud = bericht.Inhoud;
            current.PublicatieDatum = bericht.PublicatieDatum;
            current.PublicatieEinddatum = bericht.PublicatieEinddatum;
            current.IsBelangrijk = bericht.IsBelangrijk;
            current.DateUpdated = DateTimeOffset.UtcNow;

            UpdateSkills(bericht.Skills, current);


            await _context.SaveChangesAsync(token);

            return NoContent();
        }



        // POST: api/Berichten
        [HttpPost]
        [RequirePermission(RequirePermissionTo.berichtenbeheer)]
        public async Task<ActionResult<BerichtViewModel>> PostBericht(BerichtPostModel bericht, CancellationToken token)
        {
            var newBericht = new Bericht
            {
                Titel = bericht.Titel,
                Type = bericht.Type,
                Inhoud = bericht.Inhoud,
                PublicatieDatum = bericht.PublicatieDatum,
                IsBelangrijk = bericht.IsBelangrijk,
                DateCreated = DateTimeOffset.UtcNow,
                PublicatieEinddatum = bericht.PublicatieEinddatum,
            };

            UpdateSkills(bericht.Skills, newBericht);

            await _context.Berichten.AddAsync(newBericht, token);
            await _context.SaveChangesAsync(token);

            return CreatedAtAction(nameof(GetBericht), new { id = newBericht.Id }, MapBericht(newBericht));

        }

        // DELETE: api/Berichten/5
        [HttpDelete("{id}")]
        [RequirePermission(RequirePermissionTo.berichtenbeheer)]
        public async Task<IActionResult> DeleteBericht(int id, CancellationToken token)
        {
            var bericht = await _context.Berichten.FirstOrDefaultAsync(x => x.Id == id, cancellationToken: token);

            if (bericht == null)
            {
                return NotFound(token);
            }

            _context.Berichten.Remove(bericht);
            await _context.SaveChangesAsync(token);

            return NoContent();
        }

        private static BerichtViewModel MapBericht(Bericht bericht) => new()
        {
            Id = bericht.Id,
            Inhoud = bericht.Inhoud,
            IsBelangrijk = bericht.IsBelangrijk,
            PublicatieDatum = bericht.PublicatieDatum,
            PublicatieEinddatum = bericht.PublicatieEinddatum,
            DateCreated = bericht.DateCreated,
            DateUpdated = bericht.DateUpdated,
            Titel = bericht.Titel,
            Type = bericht.Type,
            Skills = bericht.Skills
                .Where(x => !x.IsDeleted)
                .Select(skill => skill.Id)
                .ToList()
        };


        private void UpdateSkills(List<int>? selectedSkills, Bericht? current)
        {
            if (current == null)
            {
                return;
            }

            if (selectedSkills == null)
            {
                current.Skills.Clear();
                return;
            }

            // remove not selected skill 
            var notSelected = current.Skills.Where(x => !selectedSkills.Contains(x.Id)).ToList();
            foreach (var item in notSelected)
            {
                current.Skills.Remove(item);
            }

            //add new skills
            foreach (var skillId in selectedSkills.Where(x => !current.Skills.Any(s => s.Id == x)))
            {
                var newSkill = new Skill { Id = skillId };
                _context.Attach(newSkill);
                current.Skills.Add(newSkill);
            }
        }

    }
    public class BerichtOverviewModel
    {
        public int Id { get; set; }
        public string Titel { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public DateTimeOffset Publicatiedatum { get; set; }
        public DateTimeOffset DateCreated { get; set; }
        public DateTimeOffset? DateUpdated { get; set; }
    }


    public class BerichtPutModel
    {
        [Required]
        public string Titel { get; set; } = string.Empty;
        [Required]
        public string Inhoud { get; set; } = string.Empty;
        public bool IsBelangrijk { get; set; }
        public List<int>? Skills { get; set; }
        public DateTimeOffset PublicatieDatum { get; set; }
        public DateTimeOffset? PublicatieEinddatum { get; set; }
        public string Type { get; set; } = string.Empty;
    }

    public class BerichtPostModel
    {
        [Required]
        public string Titel { get; set; } = string.Empty;
        [Required]
        public string Inhoud { get; set; } = string.Empty;
        public bool IsBelangrijk { get; set; }
        public List<int>? Skills { get; set; }
        public DateTimeOffset PublicatieDatum { get; set; }
        public DateTimeOffset? PublicatieEinddatum { get; set; }
        public string Type { get; set; } = string.Empty;
    }

    public class BerichtViewModel
    {
        public int Id { get; set; }

        public DateTimeOffset PublicatieDatum { get; set; }
        public DateTimeOffset? PublicatieEinddatum { get; set; }
        public DateTimeOffset DateCreated { get; set; }
        public DateTimeOffset? DateUpdated { get; set; }

        public string Titel { get; set; } = string.Empty;
        public string Inhoud { get; set; } = string.Empty;
        public bool IsBelangrijk { get; set; }

        public List<int> Skills { get; set; } = new();
        public string Type { get; set; } = string.Empty;
    }
}

