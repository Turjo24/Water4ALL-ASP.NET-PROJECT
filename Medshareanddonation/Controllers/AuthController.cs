using Microsoft.AspNetCore.Mvc;

namespace Medshareanddonation.Controllers
{
    public class Authcontroller : Controller
    {
        // GET: /Auth/SignUp
        public IActionResult SignUp()
        {
            return View();
        }

        // GET: /Auth/SignIn  
        public IActionResult SignIn()
        {
            return View();
        }
    }
}
