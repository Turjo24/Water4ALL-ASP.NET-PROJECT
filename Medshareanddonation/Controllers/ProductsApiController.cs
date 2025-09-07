using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using Medshareanddonation.Data;
using Medshareanddonation.Models;

namespace Medshareanddonation.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin")] // Only admins can manage products
    public class ProductsApiController : ControllerBase
    {
        private readonly ApplicationDbContext _db;
        private readonly IWebHostEnvironment _env;

        public ProductsApiController(ApplicationDbContext db, IWebHostEnvironment env)
        {
            _db = db;
            _env = env;
        }

        // GET: api/products
        [HttpGet]
        [AllowAnonymous] // Anyone can see products
        public async Task<IActionResult> GetAll()
        {
            var prods = await _db.Products.Include(p => p.Category)
                                          .Where(p => p.IsActive)
                                          .Select(p => new {
                                              p.Id,
                                              p.Name,
                                              p.Description,
                                              p.Price,
                                              Category = p.Category.Name,
                                              p.ImageUrl
                                          })
                                          .ToListAsync();
            return Ok(prods);
        }

        // GET: api/products/5
        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> Get(int id)
        {
            var p = await _db.Products.Include(x => x.Category)
                                      .FirstOrDefaultAsync(x => x.Id == id && x.IsActive);
            if (p == null) return NotFound();
            return Ok(p);
        }

        // POST: api/products
        [HttpPost]
        public async Task<IActionResult> Create([FromForm] Product model, IFormFile ImageUrl)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            if (ImageUrl != null)
            {
                var folder = Path.Combine(_env.WebRootPath, "uploads");
                if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);

                var fileName = Guid.NewGuid() + Path.GetExtension(ImageUrl.FileName);
                var filePath = Path.Combine(folder, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await ImageUrl.CopyToAsync(stream);
                }

                model.ImageUrl = "/uploads/" + fileName;
            }

            _db.Products.Add(model);
            await _db.SaveChangesAsync();

            return CreatedAtAction(nameof(Get), new { id = model.Id }, model);
        }

        // PUT: api/products/5
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromForm] Product model, IFormFile? ImageUrl)
        {
            var product = await _db.Products.FindAsync(id);
            if (product == null) return NotFound();

            product.Name = model.Name;
            product.Description = model.Description;
            product.Price = model.Price;
            product.CategoryId = model.CategoryId;
            product.IsActive = model.IsActive;

            if (ImageUrl != null)
            {
                var folder = Path.Combine(_env.WebRootPath, "uploads");
                if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);

                var fileName = Guid.NewGuid() + Path.GetExtension(ImageUrl.FileName);
                var filePath = Path.Combine(folder, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await ImageUrl.CopyToAsync(stream);
                }

                product.ImageUrl = "/uploads/" + fileName;
            }

            _db.Products.Update(product);
            await _db.SaveChangesAsync();

            return Ok(product);
        }

        // DELETE: api/products/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var product = await _db.Products.FindAsync(id);
            if (product == null) return NotFound();

            _db.Products.Remove(product);
            await _db.SaveChangesAsync();

            return NoContent();
        }
    }
}
