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

        [Route("ProductMVC/Details/{id:int}")]
        public IActionResult Details(int id)
        {
            // Create a ViewModel or use ViewBag to pass the product ID
            ViewBag.ProductId = id;

            // You can also create a model if needed
            var model = new { ProductId = id };

            return View(model);
        }
    }
}