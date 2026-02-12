using System.ComponentModel.DataAnnotations;
using Kiss.Bff.Beheer.Data;
using Kiss.Bff.Beheer.Links.Data.Entities;
using Kiss.Bff.Config.Permissions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Kiss.Bff.Intern.Links.Features
{
    [Route("api/[controller]")]
    [ApiController]
    public class LinksController : ControllerBase
    {
        private readonly BeheerDbContext _context;

        public LinksController(BeheerDbContext context)
        {
            _context = context;
        }

        // GET: api/Links
        [HttpGet]
        [RequirePermission(RequirePermissionTo.linksread)]
        public IActionResult GetLinks()
        {
            if (_context.Links == null)
            {
                return NotFound();
            }

            var result = _context
               .Links
               .GroupBy(x => x.Categorie)
               .Select(categorieGroep => new
               {
                   Categorie = categorieGroep.Key,
                   Items = categorieGroep
                    .OrderBy(x => x.Titel)
                    .Select(categorieGroepItems => new { categorieGroepItems.Id, categorieGroepItems.Titel, categorieGroepItems.Categorie, categorieGroepItems.Url })
               })
               .OrderBy(x => x.Categorie)
               .AsAsyncEnumerable();

            return Ok(result);
        }

        // GET: api/Links/5
        [HttpGet("{id}")]
        [RequirePermission(RequirePermissionTo.linksbeheer)]
        public async Task<ActionResult<Link>> GetLink(int id)
        {
            if (_context.Links == null)
            {
                return NotFound();
            }
            var link = await _context.Links.FindAsync(id);

            if (link == null)
            {
                return NotFound();
            }

            return link;
        }

        // PUT: api/Links/5       
        [HttpPut("{id}")]
        [RequirePermission(RequirePermissionTo.linksbeheer)]
        public async Task<IActionResult> PutLink(int id, LinkPutModel link, CancellationToken token)
        {
            var current = await _context.Links.FirstOrDefaultAsync(x => x.Id == id, token);

            if (current == null)
            {
                return NotFound();
            }

            current.Titel = link.Titel;
            current.Url = link.Url;
            current.Categorie = link.Categorie;
            current.DateUpdated = DateTimeOffset.UtcNow;

            await _context.SaveChangesAsync(token);

            return NoContent();

        }

        // POST: api/Links
        [HttpPost]
        [RequirePermission(RequirePermissionTo.linksbeheer)]
        public async Task<ActionResult<Link>> PostLink(LinkPostModel link)
        {
            if (_context.Links == null)
            {
                return Problem("Entity set 'BeheerDbContext.Links'  is null.");
            }
            var newLink = new Link
            {
                Titel = link.Titel,
                Url = link.Url,
                Categorie = link.Categorie,
                DateCreated = DateTimeOffset.UtcNow
            };

            await _context.Links.AddAsync(newLink);

            await _context.SaveChangesAsync();

            return CreatedAtAction("GetLink", new { id = newLink.Id }, newLink);
        }

        // DELETE: api/Links/5
        [HttpDelete("{id}")]
        [RequirePermission(RequirePermissionTo.linksbeheer)]
        public async Task<IActionResult> DeleteLink(int id)
        {
            if (_context.Links == null)
            {
                return NotFound();
            }
            var link = await _context.Links.FindAsync(id);
            if (link == null)
            {
                return NotFound();
            }

            _context.Links.Remove(link);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        public class LinkPostModel
        {
            [Required]
            public string Titel { get; set; } = string.Empty;
            [Required]
            public string Url { get; set; } = string.Empty;
            [Required]
            public string Categorie { get; set; } = string.Empty;

        }

        public class LinkPutModel
        {
            [Required]
            public int Id { get; set; }
            [Required]
            public string Titel { get; set; } = string.Empty;
            [Required]
            public string Url { get; set; } = string.Empty;
            [Required]
            public string Categorie { get; set; } = string.Empty;

        }

    }
}
