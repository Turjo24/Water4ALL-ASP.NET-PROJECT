using Medshareanddonation.Data;
using Medshareanddonation.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Medshareanddonation.Models.DTOs;

namespace Medshareanddonation.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class OrdersApiController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public OrdersApiController(ApplicationDbContext context)
        {
            _context = context;
        }

        
        [Authorize(Roles = "User")]
        [HttpPost("Create")]
        public async Task<ActionResult<Order>> Create([FromBody] Order model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
                return Unauthorized();

            model.UserId = userId;
            model.OrderDate = DateTime.Now;
            model.Status = "Pending";

            _context.Orders.Add(model);
            await _context.SaveChangesAsync();

            return Ok(model);
        }

        
        [HttpGet("MyOrders")]
        [Authorize(Roles = "User")]
        public async Task<IActionResult> MyOrders()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
                return Unauthorized();

            var orders = await _context.Orders
                .Where(o => o.UserId == userId)
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
                .AsNoTracking()
                .Select(o => new
                {
                    o.Id,
                    o.OrderDate,
                    o.TotalAmount,
                    o.Status,
                    Items = o.OrderItems.Select(oi => new
                    {
                        oi.ProductId,
                        oi.Quantity,
                        oi.UnitPrice,
                        oi.TotalPrice,
                        ProductName = oi.Product.Name
                    })
                })
                .ToListAsync();

            return Ok(orders);
        }



        // ------------------------
        // Get Order by Id (User)
        // ------------------------
        [Authorize(Roles = "User")]
        [HttpGet("{id}")]
        public async Task<ActionResult<Order>> GetOrderById(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
                return Unauthorized();

            var order = await _context.Orders
                .Where(o => o.Id == id && o.UserId == userId)
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
                .AsNoTracking()
                .FirstOrDefaultAsync();

            if (order == null) return NotFound();

            return Ok(order);
        }

        // ------------------------
        // Admin: Get All Orders
        // ------------------------
        [HttpGet("AllOrders")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<IEnumerable<object>>> AllOrders()
        {
            var orders = await _context.Orders
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
                .AsNoTracking()
                .Select(o => new
                {
                    o.Id,
                    o.UserId,
                    o.OrderDate,
                    o.TotalAmount,
                    o.Status,
                    o.ShippingName,
                    o.ShippingPhone,
                    o.ShippingAddress,
                    o.City,
                    o.PostalCode,
                    o.PaymentMethod,
                    o.PaymentStatus,
                    o.Notes,
                    Items = o.OrderItems.Select(oi => new
                    {
                        oi.ProductId,
                        oi.Quantity,
                        oi.UnitPrice,
                        oi.TotalPrice,
                        ProductName = oi.Product.Name
                    })
                })
                .ToListAsync();

            return Ok(orders);
        }

        // ------------------------
        // Admin: Get Single Order by ID
        // ------------------------
        [HttpGet("GetOrderById/{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetOrderByIdForAdmin(int id)
        {
            var order = await _context.Orders
                .Where(o => o.Id == id)
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
                .AsNoTracking()
                .Select(o => new
                {
                    o.Id,
                    o.UserId,
                    o.OrderDate,
                    o.TotalAmount,
                    o.Status,
                    o.ShippingName,
                    o.ShippingPhone,
                    o.ShippingAddress,
                    o.City,
                    o.PostalCode,
                    o.PaymentMethod,
                    o.PaymentStatus,
                    o.Notes,
                    Items = o.OrderItems.Select(oi => new
                    {
                        oi.ProductId,
                        oi.Quantity,
                        oi.UnitPrice,
                        oi.TotalPrice,
                        ProductName = oi.Product.Name
                    })
                })
                .FirstOrDefaultAsync();

            if (order == null)
                return NotFound();

            return Ok(order);
        }

        // ------------------------
        // Admin: Change Order Status
        // ------------------------
        [HttpPut("ChangeStatus/{id}")]
        public async Task<IActionResult> ChangeStatus(int id, [FromQuery] string status)
        {
            var order = await _context.Orders.FindAsync(id);
            if (order == null)
                return NotFound(new { message = "Order not found" });

            // Optional: validate allowed statuses
            var validStatuses = new[] { "Pending", "Confirmed", "Shipped", "Delivered", "Cancelled" };
            if (!validStatuses.Contains(status))
                return BadRequest(new { message = "Invalid status value" });

            order.Status = status;
            _context.Orders.Update(order);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Status updated successfully" });
        }
        // ------------------------
        // Admin: Delete Order
        // ------------------------
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteOrder(int id)
        {
            var order = await _context.Orders
                .Include(o => o.OrderItems)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order == null)
                return NotFound(new { message = "Order not found" });

            // Remove related order items first (if cascade delete is not set in DB)
            _context.OrderItems.RemoveRange(order.OrderItems);

            _context.Orders.Remove(order);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Order deleted successfully" });
        }

        // ------------------------
        // Test User (Debug)
        // ------------------------
        [HttpGet("TestUser")]
        [Authorize]
        public IActionResult TestUser()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
                return Unauthorized("UserId not found");

            return Ok(new { UserId = userId });
        }
    }
}
