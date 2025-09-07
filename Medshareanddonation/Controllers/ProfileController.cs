using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;

namespace Medshareanddonation.Controllers
{
    [Route("[controller]")]
    public class ProfileController : Controller
    {
        // Full-page profile view (requires authorization for API)
        [HttpGet("Details")]
        public IActionResult Details()
        {
            // Just render the Razor page
            return View("UserProfile");
        }

        // API endpoint to get user info via JWT
        [HttpGet("GetData")]
        [AllowAnonymous] // We'll manually validate the token from header
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
