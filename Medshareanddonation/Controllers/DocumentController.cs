using Microsoft.AspNetCore.Mvc;

namespace Medshareanddonation.Controllers
{
    [Route("Document/{section?}")]
    public class DocumentController : Controller
    {
        [HttpGet]
        public IActionResult Index(string section = "Boiling")
        {
            // Detect if AJAX request (fetch with header X-Requested-With)
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return GetPartialView(section);
            }

            // Return full page with sidebar, navbar, and JavaScript
            ViewData["Section"] = section;
            return View();
        }

        private IActionResult GetPartialView(string section)
        {
            return section switch
            {
                "Boiling" => PartialView("_Boiling"),
                "Filtration" => PartialView("_Filtration"),
                "ChemicalTreatment" => PartialView("_Chemical"),
                "UVPurification" => PartialView("_UV"),
                _ => PartialView("_Boiling"),
            };
        }
    }
}
