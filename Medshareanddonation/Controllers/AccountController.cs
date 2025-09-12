using Microsoft.AspNetCore.Mvc;

namespace Medshareanddonation.Controllers
{
    // Provides compatibility for /Account/Login by redirecting to our MVC auth page
    public class AccountController : Controller
    {
        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            if (!string.IsNullOrEmpty(returnUrl))
            {
                TempData["ReturnUrl"] = returnUrl;
            }
            return RedirectToAction("SignIn", "Auth");
        }
    }
}


