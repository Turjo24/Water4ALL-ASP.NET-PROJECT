using Medshareanddonation.Data;
using Medshareanddonation.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Medshareanddonation.Controllers
{
    [AllowAnonymous]
    [Route("OrderMvc")]
    public class OrderMvcController : Controller
    {
        private readonly ApplicationDbContext _db;

        public OrderMvcController(ApplicationDbContext db)
        {
            _db = db;
        }

        // GET: /OrderMvc/AdminOrders
        [HttpGet("AdminOrders")]
        public IActionResult AdminOrders()
        {
            // Explicitly point to OrderMvc folder
            return View("~/Views/OrderMvc/AdminOrder.cshtml");
        }
        [HttpGet("MyOrders")]
        public IActionResult MyOrders()
        {
            return View("~/Views/OrderMvc/MyOrder.cshtml");
        }


    }
}
