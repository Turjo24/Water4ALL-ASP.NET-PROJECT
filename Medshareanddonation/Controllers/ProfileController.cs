using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;

namespace Medshareanddonation.Controllers
{
    [Route("[controller]")]
    public class ProfileController : Controller
    {
        // Direct profile views
        [HttpGet("UserProfile")]
        public IActionResult UserProfile()
        {
            return View("UserProfile"); // Razor page for normal users
        }

        [HttpGet("AdminProfile")]
        public IActionResult AdminProfile()
        {
            return View("AdminProfile"); // Razor page for admins
        }

        // Optional: Details route (can be used if needed)
        [HttpGet("Details")]
        [AllowAnonymous]
        public IActionResult Details(string token = null)
        {
            if (!string.IsNullOrEmpty(token))
            {
                var handler = new JwtSecurityTokenHandler();
                var jwt = handler.ReadJwtToken(token);
                var role = jwt.Claims.FirstOrDefault(c => c.Type.Contains("role"))?.Value ?? "User";

                if (role == "Admin")
                    return View("AdminProfile");
            }

            return View("UserProfile");
        }

        // API to fetch user info
        [HttpGet("GetData")]
        [AllowAnonymous]
        public IActionResult GetData()
        {
            var authHeader = Request.Headers["Authorization"].FirstOrDefault();
            if (string.IsNullOrEmpty(authHeader) || !authHeader.StartsWith("Bearer "))
                return Unauthorized();

            var token = authHeader.Substring("Bearer ".Length).Trim();
            var handler = new JwtSecurityTokenHandler();
            var jwt = handler.ReadJwtToken(token);

            var userName = jwt.Claims.FirstOrDefault(c =>
                    c.Type == "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name"
                )?.Value ?? "Guest";

            var email = jwt.Claims.FirstOrDefault(c => c.Type.Contains("email"))?.Value ?? "No Email";
            var role = jwt.Claims.FirstOrDefault(c => c.Type.Contains("role"))?.Value ?? "User";
            var date = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

            return Json(new { userName, email, role, date });
        }
    }
}
