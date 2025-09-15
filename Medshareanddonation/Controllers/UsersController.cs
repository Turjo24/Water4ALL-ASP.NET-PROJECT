using Medshareanddonation.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using ApplicationUser = Medshareanddonation.Data.ApplicationUser;

namespace Medshareanddonation.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class UserController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public UserController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: api/User/AllUsers
        [HttpGet("AllUsers")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<IEnumerable<object>>> GetAllUsers()
        {
            try
            {
                var users = await _userManager.Users
                    .Select(u => new
                    {
                        Id = u.Id,
                        Name = u.name,
                        Email = u.Email,
                        Role = u.Role,
                        UserName = u.UserName
                    })
                    .ToListAsync();

                return Ok(users);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error fetching users", error = ex.Message });
            }
        }

        // GET: api/User/Volunteers
        [HttpGet("Volunteers")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<IEnumerable<object>>> GetVolunteers()
        {
            try
            {
                var volunteers = await _userManager.Users
                    .Where(u => u.Role == "Volunteer")
                    .Select(u => new
                    {
                        Id = u.Id,
                        Name = u.name,
                        Email = u.Email,
                        UserName = u.UserName
                    })
                    .ToListAsync();

                return Ok(volunteers);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error fetching volunteers", error = ex.Message });
            }
        }

        // GET: api/User/UsersByRole/{role}
        [HttpGet("UsersByRole/{role}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<IEnumerable<object>>> GetUsersByRole(string role)
        {
            try
            {
                var validRoles = new[] { "Admin", "Volunteer", "User" };
                if (!validRoles.Contains(role))
                {
                    return BadRequest("Invalid role specified");
                }

                var users = await _userManager.Users
                    .Where(u => u.Role == role)
                    .Select(u => new
                    {
                        Id = u.Id,
                        Name = u.name,
                        Email = u.Email,
                        Role = u.Role,
                        UserName = u.UserName
                    })
                    .ToListAsync();

                return Ok(users);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error fetching users by role", error = ex.Message });
            }
        }

        // GET: api/User/VolunteerInfo/{id} - Allow users to get volunteer info for their assigned requests
        [HttpGet("VolunteerInfo/{volunteerId}")]
        [Authorize] // Any authenticated user can access this
        public async Task<ActionResult<object>> GetVolunteerInfo(string volunteerId)
        {
            try
            {
                if (string.IsNullOrEmpty(volunteerId) || volunteerId == "NULL")
                {
                    return NotFound("No volunteer assigned");
                }

                var volunteer = await _userManager.Users
                    .Where(u => u.Id == volunteerId && u.Role == "Volunteer")
                    .Select(u => new
                    {
                        Id = u.Id,
                        Name = u.name,
                        Email = u.Email
                    })
                    .FirstOrDefaultAsync();

                if (volunteer == null)
                {
                    return NotFound("Volunteer not found");
                }

                return Ok(volunteer);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error fetching volunteer info", error = ex.Message });
            }
        }

        // GET: api/User/{id}
        [HttpGet("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<object>> GetUserById(string id)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(id);
                if (user == null)
                {
                    return NotFound("User not found");
                }

                var userInfo = new
                {
                    Id = user.Id,
                    Name = user.name,
                    Email = user.Email,
                    Role = user.Role,
                    UserName = user.UserName,
                    EmailConfirmed = user.EmailConfirmed,
                    PhoneNumber = user.PhoneNumber,
                    TwoFactorEnabled = user.TwoFactorEnabled,
                    LockoutEnabled = user.LockoutEnabled,
                    AccessFailedCount = user.AccessFailedCount
                };

                return Ok(userInfo);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error fetching user details", error = ex.Message });
            }
        }
    }
}