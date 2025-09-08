using Medshareanddonation.Services;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Medshareanddonation.Controllers
{
    [Route("[controller]")]
    public class ProfileController : Controller
    {
        private readonly IProfileService _profileService;

        public ProfileController(IProfileService profileService)
        {
            _profileService = profileService;
        }

        [HttpGet("AdminProfile")]
        public IActionResult AdminProfile()
        {
            return View("AdminProfile");
        }

        [HttpGet("UserProfile")]
        public IActionResult UserProfile()
        {
            return View("UserProfile");
        }

        // Get user data from JWT
        [HttpGet("GetData")]
        public IActionResult GetData()
        {
            var authHeader = Request.Headers["Authorization"].FirstOrDefault();
            if (string.IsNullOrEmpty(authHeader) || !authHeader.StartsWith("Bearer "))
                return Unauthorized("No token provided");

            var token = authHeader.Substring("Bearer ".Length).Trim();
            var jwt = new JwtSecurityTokenHandler().ReadJwtToken(token);

            var userName = jwt.Claims.FirstOrDefault(c =>
       c.Type == "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name"
   )?.Value ?? "Guest";
            var email = jwt.Claims.FirstOrDefault(c => c.Type.Contains("email") || c.Type == ClaimTypes.Email)?.Value ?? "No Email";
            var role = jwt.Claims.FirstOrDefault(c => c.Type.Contains("role"))?.Value ?? "User";
            var date = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

            return Json(new { userName, email, role, date });
        }

        // GET: Update Name Page
        [HttpGet("UpdateName")]
        public IActionResult UpdateName()
        {
            string currentName = "";
            var authHeader = Request.Headers["Authorization"].FirstOrDefault();

            if (!string.IsNullOrEmpty(authHeader) && authHeader.StartsWith("Bearer "))
            {
                var token = authHeader.Substring("Bearer ".Length).Trim();
                try
                {
                    var payload = new JwtSecurityTokenHandler().ReadJwtToken(token);
                    currentName = payload.Claims.FirstOrDefault(c => c.Type.Contains("name"))?.Value ?? "";
                }
                catch { }
            }

            return View(model: currentName);
        }

        // POST: Permanently update username
        [HttpPost("UpdateName")]
        public async Task<IActionResult> UpdateName([FromBody] UpdateNameRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.NewName))
                return BadRequest("Username cannot be empty");

            var authHeader = Request.Headers["Authorization"].FirstOrDefault();
            if (string.IsNullOrEmpty(authHeader) || !authHeader.StartsWith("Bearer "))
                return Unauthorized("No token provided");

            var token = authHeader.Substring("Bearer ".Length).Trim();
            var jwt = new JwtSecurityTokenHandler().ReadJwtToken(token);

            var userId = jwt.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized("Invalid token");

            // Update username permanently in the database
            var success = await _profileService.UpdateUserNameAsync(userId, request.NewName);
            if (!success)
                return BadRequest("Update failed");

            return Ok(new { success = true, newName = request.NewName });
        }
    }

    public class UpdateNameRequest
    {
        public string NewName { get; set; }
    }
}
