using Microsoft.AspNetCore.Mvc;

namespace Medshareanddonation.Controllers
{
    public class volunterMVCcontroller : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
        public IActionResult AssignedDonations()
        {
            return View();
        }
        
        public IActionResult AllReport()
        {
            return View();
        }
    }
}
