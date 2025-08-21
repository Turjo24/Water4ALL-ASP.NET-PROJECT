using Microsoft.AspNetCore.Mvc;

public class AuthController : Controller
{
    public IActionResult SignIn()
    {
        return View(); // Will look for Views/Auth/SignIn.cshtml
    }

    public IActionResult SignUp()
    {
        return View(); // Will look for Views/Auth/SignUp.cshtml
    }
}
