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

        [HttpGet("UserProfile")]
        public IActionResult UserProfile()
        {
            return View();
        }

        [HttpGet("GetProfile")]
        public async Task<IActionResult> GetProfile()
        {
            var authHeader = Request.Headers["Authorization"].FirstOrDefault();
            if (string.IsNullOrEmpty(authHeader) || !authHeader.StartsWith("Bearer "))
                return Unauthorized("No token provided");

            var token = authHeader.Substring("Bearer ".Length).Trim();
            var jwt = new JwtSecurityTokenHandler().ReadJwtToken(token);

            var userId = jwt.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized("Invalid token");

            var userProfile = await _profileService.GetUserProfileAsync(userId);
            if (userProfile == null)
                return NotFound("User not found");

            var currentDateTime = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

            return Json(new
            {
                name = userProfile.name,
                userName = userProfile.UserName,
                email = userProfile.Email,
                role = userProfile.Role,
                createdAt = currentDateTime,
                updatedAt = currentDateTime
            });
        }

        [HttpPost("UpdateProfile")]
        public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.NewName) || string.IsNullOrWhiteSpace(request.NewUserName))
                return BadRequest("Name and Username cannot be empty");

            var authHeader = Request.Headers["Authorization"].FirstOrDefault();
            if (string.IsNullOrEmpty(authHeader) || !authHeader.StartsWith("Bearer "))
                return Unauthorized("No token provided");

            var token = authHeader.Substring("Bearer ".Length).Trim();
            var jwt = new JwtSecurityTokenHandler().ReadJwtToken(token);

            var userId = jwt.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized("Invalid token");

            var success = await _profileService.UpdateNameAndUserNameAsync(userId, request.NewName, request.NewUserName);
            if (!success)
                return BadRequest("Update failed");

            var updatedProfile = await _profileService.GetUserProfileAsync(userId);
            var currentDateTime = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

            return Ok(new
            {
                success = true,
                name = updatedProfile.name,
                userName = updatedProfile.UserName,
                email = updatedProfile.Email,
                role = updatedProfile.Role,
                updatedAt = currentDateTime
            });
        }
    }

    public class UpdateProfileRequest
    {
        public string NewName { get; set; }
        public string NewUserName { get; set; }
    }
}
