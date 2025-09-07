using Microsoft.AspNetCore.Mvc;

namespace Medshareanddonation.Controllers
{
    public class productMVCcontroller : Controller
    {
        public IActionResult AdminProducts()
        {
            return View();
        }

        public IActionResult UserProducts()
        {
            return View();
        }
    }
}
