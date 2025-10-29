using backenddef.Data;
using backenddef.Dtos;
using backenddef.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace backenddef.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ProductsController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<ProductReadDto>>> GetAll()
        {
            var products = await _context.Products.Include(p => p.User).ToListAsync();
            return products.Select(p => new ProductReadDto
            {
                Id = p.Id,
                Name = p.Name,
                Description = p.Description,
                Price = p.Price,
                Stock = p.Stock,
                EmpresaId = p.UserId,
                EmpresaName = p.User?.Name
            }).ToList();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ProductReadDto>> GetById(int id)
        {
            var p = await _context.Products.Include(x => x.User).FirstOrDefaultAsync(p => p.Id == id);
            if (p == null) return NotFound();

            return new ProductReadDto
            {
                Id = p.Id,
                Name = p.Name,
                Description = p.Description,
                Price = p.Price,
                Stock = p.Stock,
                EmpresaId = p.UserId,
                EmpresaName = p.User?.Name
            };
        }

        [HttpPost]
        [Authorize(Roles = "Empresa")]
        public async Task<ActionResult<ProductReadDto>> Create(ProductCreateDto dto)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var product = new Product
            {
                Name = dto.Name,
                Description = dto.Description,
                Price = dto.Price,
                Stock = dto.Stock,
                UserId = userId
            };

            _context.Products.Add(product);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetById), new { id = product.Id }, new ProductReadDto
            {
                Id = product.Id,
                Name = product.Name,
                Description = product.Description,
                Price = product.Price,
                Stock = product.Stock,
                EmpresaId = userId,
                EmpresaName = User.Identity!.Name
            });
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Empresa")]
        public async Task<ActionResult> Update(int id, ProductUpdateDto dto)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var product = await _context.Products.FirstOrDefaultAsync(p => p.Id == id && p.UserId == userId);
            if (product == null) return NotFound();

            product.Name = dto.Name;
            product.Description = dto.Description;
            product.Price = dto.Price;
            product.Stock = dto.Stock;

            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Empresa")]
        public async Task<ActionResult> Delete(int id)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var product = await _context.Products.FirstOrDefaultAsync(p => p.Id == id && p.UserId == userId);
            if (product == null) return NotFound();

            _context.Products.Remove(product);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}
