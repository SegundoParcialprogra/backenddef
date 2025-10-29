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
    public class ReviewsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ReviewsController(AppDbContext context)
        {
            _context = context;
        }

        [HttpPost]
        [Authorize(Roles = "Cliente")]
        public async Task<ActionResult> Create(ReviewCreateDto dto)
        {
            var clienteId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            // Verificar que el cliente compró el producto
            var purchased = await _context.Orders
                .Include(o => o.Items)
                .AnyAsync(o => o.ClienteId == clienteId && o.Items.Any(i => i.ProductId == dto.ProductId));

            if (!purchased)
                return BadRequest("No puedes reseñar un producto que no has comprado");

            var review = new Review
            {
                ProductId = dto.ProductId,
                ClienteId = clienteId,
                Rating = dto.Rating,
                Comment = dto.Comment,
                CreatedAt = DateTime.UtcNow
            };

            _context.Reviews.Add(review);
            await _context.SaveChangesAsync();

            return Ok();
        }

        [HttpGet("product/{productId}")]
        public async Task<ActionResult<IEnumerable<ReviewReadDto>>> GetByProduct(int productId)
        {
            var reviews = await _context.Reviews
                .Include(r => r.Cliente)
                .Include(r => r.Product)
                .Where(r => r.ProductId == productId)
                .ToListAsync();

            return reviews.Select(r => new ReviewReadDto
            {
                Id = r.Id,
                ProductId = r.ProductId,
                ProductName = r.Product?.Name,
                ClienteId = r.ClienteId,
                ClienteName = r.Cliente?.Name,
                Rating = r.Rating,
                Comment = r.Comment,
                CreatedAt = r.CreatedAt
            }).ToList();
        }
    }
}
