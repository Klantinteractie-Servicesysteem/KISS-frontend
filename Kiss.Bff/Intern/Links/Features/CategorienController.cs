using Kiss.Bff.Beheer.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Kiss.Bff.Intern.Links.Features
{
    [Route("api/[controller]")]
    [Authorize(Policies.KcmOrRedactiePolicy)]
    [ApiController]
    public class CategorienController : ControllerBase
    {
        private readonly BeheerDbContext _context;

        public CategorienController(BeheerDbContext context)
        {
            _context = context;
        }

        // GET: api/Categorien
        [HttpGet]
        public IActionResult GetLinks()
        {
            if (_context.Links == null)
            {
                return NotFound();
            }

            var result = _context
               .Links
               .GroupBy(x => x.Categorie)
               .Select(categorieGroep => categorieGroep.Key)
               .AsAsyncEnumerable();

            return Ok(result);
        }


    }
}
