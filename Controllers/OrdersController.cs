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
    public class OrdersController : ControllerBase
    {
        private readonly AppDbContext _context;

        public OrdersController(AppDbContext context)
        {
            _context = context;
        }

        [HttpPost]
        [Authorize(Roles = "Cliente")]
        public async Task<ActionResult> Create(OrderCreateDto dto)
        {
            var clienteId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            var empresa = await _context.Users.FirstOrDefaultAsync(u => u.Id == dto.EmpresaId && u.Role == UserRole.Empresa);
            if (empresa == null) return BadRequest("Empresa no encontrada");

            var orderItems = new List<OrderItem>();
            foreach (var item in dto.Items)
            {
                var product = await _context.Products.FirstOrDefaultAsync(p => p.Id == item.ProductId && p.UserId == dto.EmpresaId);
                if (product == null) return BadRequest($"Producto {item.ProductId} no encontrado");
                if (product.Stock < item.Quantity) return BadRequest($"Stock insuficiente para {product.Name}");

                // Descontar stock
                product.Stock -= item.Quantity;

                orderItems.Add(new OrderItem
                {
                    ProductId = product.Id,
                    Quantity = item.Quantity,
                    Price = product.Price
                });
            }

            var order = new Order
            {
                ClienteId = clienteId,
                EmpresaId = dto.EmpresaId,
                Status = OrderStatus.Nuevo,
                OrderDate = DateTime.UtcNow,
                Items = orderItems
            };

            _context.Orders.Add(order);
            await _context.SaveChangesAsync();

            return Ok(new { order.Id });
        }

        [HttpGet("cliente")]
        [Authorize(Roles = "Cliente")]
        public async Task<ActionResult<IEnumerable<OrderReadDto>>> GetOrdersCliente()
        {
            var clienteId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var orders = await _context.Orders
                .Include(o => o.Items)
                .ThenInclude(oi => oi.Product)
                .Include(o => o.Empresa)
                .Where(o => o.ClienteId == clienteId)
                .ToListAsync();

            return orders.Select(o => new OrderReadDto
            {
                Id = o.Id,
                ClienteId = o.ClienteId,
                ClienteName = User.Identity!.Name,
                EmpresaId = o.EmpresaId,
                EmpresaName = o.Empresa?.Name,
                OrderDate = o.OrderDate,
                Status = o.Status.ToString(),
                Items = o.Items.Select(i => new OrderItemReadDto
                {
                    ProductId = i.ProductId,
                    ProductName = i.Product?.Name,
                    Quantity = i.Quantity,
                    Price = i.Price
                }).ToList()
            }).ToList();
        }

        [HttpGet("empresa")]
        [Authorize(Roles = "Empresa")]
        public async Task<ActionResult<IEnumerable<OrderReadDto>>> GetOrdersEmpresa()
        {
            var empresaId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var orders = await _context.Orders
                .Include(o => o.Items)
                .ThenInclude(oi => oi.Product)
                .Include(o => o.Cliente)
                .Where(o => o.EmpresaId == empresaId)
                .ToListAsync();

            return orders.Select(o => new OrderReadDto
            {
                Id = o.Id,
                ClienteId = o.ClienteId,
                ClienteName = o.Cliente?.Name,
                EmpresaId = o.EmpresaId,
                EmpresaName = User.Identity!.Name,
                OrderDate = o.OrderDate,
                Status = o.Status.ToString(),
                Items = o.Items.Select(i => new OrderItemReadDto
                {
                    ProductId = i.ProductId,
                    ProductName = i.Product?.Name,
                    Quantity = i.Quantity,
                    Price = i.Price
                }).ToList()
            }).ToList();
        }

        [HttpPut("{id}/status")]
        [Authorize(Roles = "Empresa")]
        public async Task<ActionResult> UpdateStatus(int id, OrderStatusUpdateDto dto)
        {
            var empresaId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var order = await _context.Orders.FirstOrDefaultAsync(o => o.Id == id && o.EmpresaId == empresaId);
            if (order == null) return NotFound();

            if (!Enum.TryParse<OrderStatus>(dto.Status, true, out var newStatus))
                return BadRequest("Estado inválido");

            order.Status = newStatus;
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
