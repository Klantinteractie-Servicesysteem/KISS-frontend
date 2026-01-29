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
    public class SkillsController : ControllerBase
    {
        private readonly BeheerDbContext _context;

        public SkillsController(BeheerDbContext context)
        {
            _context = context;
        }

        // GET: api/Skills
        [HttpGet]
        [RequirePermission(RequirePermissionTo.skillsread, RequirePermissionTo.skillsbeheer)]
        public ActionResult<IAsyncEnumerable<Skill>> GetSkills()
        {
            var result = _context.Skills.Where(x => !x.IsDeleted)
                .Select(x => new SkillViewModel { Id = x.Id, Naam = x.Naam })
                .OrderBy(x => x.Naam)
                .AsAsyncEnumerable();

            return Ok(result);
        }

        // GET: api/Skills/5
        [HttpGet("{id}")]
        [RequirePermission(RequirePermissionTo.skillsbeheer)]
        public async Task<ActionResult<SkillViewModel>> GetSkill(int id, CancellationToken token)
        {
            if (_context.Skills == null)
            {
                return NotFound();
            }
            var skill = await _context.Skills.FirstOrDefaultAsync(x => x.Id == id, cancellationToken: token);

            if (skill == null || skill.IsDeleted)
            {
                return NotFound();
            }

            return new SkillViewModel { Id = skill.Id, Naam = skill.Naam };
        }

        // PUT: api/Skills/5
        [HttpPut("{id}")]
        [RequirePermission(RequirePermissionTo.skillsbeheer)]
        public async Task<IActionResult> PutSkill(int id, SkillPutModel skill, CancellationToken token)
        {
            var current = await _context.Skills.FirstOrDefaultAsync(x => x.Id == id, token);

            if (current == null)
            {
                return NotFound();
            }

            current.Naam = skill.Naam;

            await _context.SaveChangesAsync(token);

            return NoContent();
        }

        // POST: api/Skills
        [HttpPost]
        [RequirePermission(RequirePermissionTo.skillsbeheer)]
        public async Task<ActionResult<Skill>> PostSkill(SkillPostModel skill, CancellationToken token)
        {
            var newSkill = new Skill { Naam = skill.Naam };

            await _context.Skills.AddAsync(newSkill, token);

            await _context.SaveChangesAsync(token);

            return CreatedAtAction("GetSkill", new { id = newSkill.Id }, newSkill);
        }

        // DELETE: api/Skills/5
        [HttpDelete("{id}")]
        [RequirePermission(RequirePermissionTo.skillsbeheer)]
        public async Task<IActionResult> DeleteSkill(int id, CancellationToken token)
        {

            if (_context.Skills == null)
            {
                return NotFound();
            }
            var skill = await _context.Skills.FirstOrDefaultAsync(x => x.Id == id, token);

            if (skill == null)
            {
                return NotFound();
            }

            //soft delete
            skill.IsDeleted = true;
            await _context.SaveChangesAsync(token);

            return NoContent();
        }

    }
    public class SkillViewModel
    {
        public int Id { get; set; }
        public string Naam { get; set; } = string.Empty;
    }

    public class SkillPutModel
    {
        [Required]
        public string Naam { get; set; } = default!;
    }

    public class SkillPostModel
    {
        [Required]
        public string Naam { get; set; } = default!;
    }
}
