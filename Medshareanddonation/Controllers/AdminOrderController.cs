using Microsoft.AspNetCore.Mvc;

namespace Medshareanddonation.Controllers
{
    public class AdminOrderController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
