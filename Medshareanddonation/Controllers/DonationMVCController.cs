using Microsoft.AspNetCore.Mvc;

namespace Medshareanddonation.Controllers
{
    public class DonationMVCController : Controller
    {
        public IActionResult Create()
        {
            return View();
        }
        public IActionResult MyRequests()
        {
            return View();
        }
        public IActionResult AllRequests()
        {
            return View();
        }
    }
}
