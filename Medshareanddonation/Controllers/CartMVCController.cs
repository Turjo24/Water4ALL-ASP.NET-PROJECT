using Medshareanddonation.Data;
using Medshareanddonation.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using System.Text.Json;

namespace Medshareanddonation.Controllers
{
    [Route("CartMVC")]
    public class CartMVCController : Controller
    {
        private const string SessionCartKey = "CART_ITEMS";
        private readonly ApplicationDbContext _db;

        public CartMVCController(ApplicationDbContext db)
        {
            _db = db;
        }

        [HttpGet("")]
        [HttpGet("Index")]
        [AllowAnonymous]
        public IActionResult Index()
        {
            return View();
        }

        private List<CartItem> GetCart()
        {
            var json = HttpContext.Session.GetString(SessionCartKey);
            if (string.IsNullOrWhiteSpace(json)) return new List<CartItem>();
            try
            {
                var items = JsonSerializer.Deserialize<List<CartItem>>(json) ?? new List<CartItem>();
                return items;
            }
            catch
            {
                return new List<CartItem>();
            }
        }

        private void SaveCart(List<CartItem> items)
        {
            var json = JsonSerializer.Serialize(items);
            HttpContext.Session.SetString(SessionCartKey, json);
        }

        public class AddToCartRequest
        {
            public int ProductId { get; set; }
            public int Quantity { get; set; } = 1;
        }

        [HttpPost("AddToCart")]
        [IgnoreAntiforgeryToken]
        [AllowAnonymous]
        public async Task<IActionResult> AddToCart([FromBody] AddToCartRequest request)
        {
            if (request == null || request.ProductId <= 0 || request.Quantity <= 0)
                return BadRequest(new { message = "Invalid request" });

            var product = await _db.Products.AsNoTracking()
                .FirstOrDefaultAsync(p => p.Id == request.ProductId && p.IsActive);
            if (product == null)
                return NotFound(new { message = "Product not found" });

            var cart = GetCart();
            var existing = cart.FirstOrDefault(ci => ci.ProductId == request.ProductId);
            if (existing == null)
            {
                cart.Add(new CartItem
                {
                    ProductId = product.Id,
                    Quantity = request.Quantity,
                    Price = product.Price
                });
            }
            else
            {
                existing.Quantity += request.Quantity;
                existing.Price = product.Price; // keep price in sync with product price
            }

            SaveCart(cart);

            return Ok(new
            {
                message = "Added to cart",
                cartCount = cart.Sum(i => i.Quantity)
            });
        }

        [HttpGet("GetCart")]
        [AllowAnonymous]
        public async Task<IActionResult> GetCartItems()
        {
            var cart = GetCart();
            if (cart.Count == 0)
            {
                return Ok(new { items = Array.Empty<object>(), total = 0m });
            }

            var productMap = new Dictionary<int, Product>();
            foreach (var id in cart.Select(c => c.ProductId).Distinct())
            {
                var prod = await _db.Products.AsNoTracking().FirstOrDefaultAsync(p => p.Id == id);
                if (prod != null)
                {
                    productMap[id] = prod;
                }
            }

            var result = cart.Select(ci => new
            {
                ci.ProductId,
                ci.Quantity,
                ci.Price,
                TotalPrice = ci.TotalPrice,
                Product = productMap.TryGetValue(ci.ProductId, out var p) ? new
                {
                    p.Name,
                    p.ImageUrl
                } : null
            }).ToList();

            return Ok(new
            {
                items = result,
                total = result.Sum(r => r.TotalPrice)
            });
        }

        public class CheckoutRequest
        {
            public string ShippingName { get; set; }
            public string ShippingPhone { get; set; }
            public string ShippingAddress { get; set; }
            public string? City { get; set; }
            public string? PostalCode { get; set; }
            public string PaymentMethod { get; set; } = "Cash on Delivery";
            public string? Notes { get; set; }
        }

        [HttpPost("Checkout")]
        [IgnoreAntiforgeryToken]
        [Authorize]
        public async Task<IActionResult> Checkout([FromBody] CheckoutRequest request)
        {
            var cart = GetCart();
            if (cart.Count == 0) return BadRequest(new { message = "Cart is empty" });

            // Refresh prices and calculate totals from DB
            var productMap = new Dictionary<int, Product>();
            foreach (var id in cart.Select(c => c.ProductId).Distinct())
            {
                var prod = await _db.Products.AsNoTracking().FirstOrDefaultAsync(p => p.Id == id);
                if (prod == null || !prod.IsActive) return BadRequest(new { message = "One or more items are unavailable" });
                productMap[id] = prod;
            }

            foreach (var item in cart)
            {
                var prod = productMap[item.ProductId];
                item.Price = prod.Price;
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;

            var order = new Order
            {
                UserId = userId,
                OrderDate = DateTime.Now,
                TotalAmount = cart.Sum(i => i.TotalPrice),
                Status = "Pending",
                ShippingName = request.ShippingName,
                ShippingPhone = request.ShippingPhone,
                ShippingAddress = request.ShippingAddress,
                City = request.City,
                PostalCode = request.PostalCode,
                PaymentMethod = request.PaymentMethod,
                Notes = request.Notes,
                OrderItems = cart.Select(ci => new OrderItem
                {
                    ProductId = ci.ProductId,
                    Quantity = ci.Quantity,
                    UnitPrice = ci.Price,
                    TotalPrice = ci.TotalPrice
                }).ToList()
            };

            _db.Orders.Add(order);
            await _db.SaveChangesAsync();

            // Clear cart
            SaveCart(new List<CartItem>());

            return Ok(new { orderId = order.Id, redirectUrl = Url.Action("OrderComplete", new { id = order.Id }) });
        }

        [HttpGet("OrderComplete/{id:int}")]
        [Authorize]
        public async Task<IActionResult> OrderComplete(int id)
        {
            var order = await _db.Orders
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
                .FirstOrDefaultAsync(o => o.Id == id);
            if (order == null) return NotFound();

            return View(order);
        }
    }
}


