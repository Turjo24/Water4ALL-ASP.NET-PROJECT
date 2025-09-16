using Medshareanddonation.Data;
using Medshareanddonation.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

[Route("Orders")]
public class OrdersController : Controller
{
    private readonly ApplicationDbContext _db;

    public OrdersController(ApplicationDbContext db)
    {
        _db = db;
    }

    // GET /Orders/OrderComplete/5
    [HttpGet("OrderComplete/{id:int}")]
    [Authorize]
    public async Task<IActionResult> OrderComplete(int id)
    {
        var order = await _db.Orders
            .Include(o => o.OrderItems)
            .ThenInclude(oi => oi.Product)
            .FirstOrDefaultAsync(o => o.Id == id);

        if (order == null) return NotFound();

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var isAdmin = User.IsInRole("Admin");

        if (!isAdmin && order.UserId != userId)
            return Forbid();

        return View(order); // Views/Orders/OrderComplete.cshtml
    }
}
