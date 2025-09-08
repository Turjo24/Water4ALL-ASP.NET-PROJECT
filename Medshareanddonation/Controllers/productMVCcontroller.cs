using Microsoft.AspNetCore.Mvc;

namespace Medshareanddonation.Controllers
{
    public class ProductMVCController : Controller
    {
        public IActionResult Create()
        {
            return View();
        }

        [Route("All")]
        public IActionResult ProductView()
        {
            return View();
        }


        public IActionResult Details()
        {
            return View();
        }


    }
}
