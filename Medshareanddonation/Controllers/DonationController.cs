using Medshareanddonation.Data;
using Medshareanddonation.Models;
using Microsoft.AspNetCore.Authorization;       
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using ApplicationUser = Medshareanddonation.Data.ApplicationUser;

namespace Medshareanddonation.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DonationController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly string _jwtKey;
        private readonly string _jwtIssuer;
        private readonly string _jwtAudience;
        private readonly int _jwtExpiry;

        public DonationController(ApplicationDbContext context, UserManager<ApplicationUser> userManager, 
            SignInManager<ApplicationUser> signInManager,
          RoleManager<IdentityRole> roleManager, IConfiguration configuration
          )
        {
            _context = context;
            _userManager = userManager;
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
            _jwtKey = configuration["Jwt:Key"] ?? throw new ArgumentNullException("Jwt:Key");
            _jwtIssuer = configuration["Jwt:Issuer"] ?? throw new ArgumentNullException("Jwt:Issuer");
            _jwtAudience = configuration["Jwt:Audience"] ?? throw new ArgumentNullException("Jwt:Audience");
            _jwtExpiry = int.TryParse(configuration["Jwt:ExpiryInMinutes"], out var expiry) ? expiry : 60;
        }



        // In the Create method, check if user is null before accessing user.Id
        [Authorize(Roles = "User")]
        [HttpPost("Create")]
        public async Task<ActionResult<DonationRequest>> Create([FromBody] DonationRequest model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // 🔽 JWT theke UserId fetch
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (userId == null)
                return Unauthorized();

            model.UserId = userId;

            _context.DonationRequests.Add(model);
            await _context.SaveChangesAsync();

            return Ok(model);
        }


        // In the MyRequests method, check if user is null before accessing user.Id
        [Authorize(Roles = "User")]
        [HttpGet("MyRequests")]
        public async Task<ActionResult<IEnumerable<DonationRequest>>> MyRequests()
        {
            var user = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (user == null)
                return Unauthorized();

            var requests = await _context.DonationRequests
                .Where(r => r.UserId == user)
                .ToListAsync();

            return Ok(requests);
        }

        // GET: api/Donation/AllRequests
        [HttpGet("AllRequests")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<IEnumerable<DonationRequest>>> AllRequests()
        {
            var requests = await _context.DonationRequests
                .ToListAsync();

            return Ok(requests);
        }

        // PUT: api/Donation/ChangeStatus/5
        [HttpPut("ChangeStatus/{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ChangeStatus(int id, [FromQuery] string status)
        {
            var request = await _context.DonationRequests.FindAsync(id);
            if (request == null) return NotFound();

            request.Status = status;
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // PUT: api/Donation/AssignVolunteer/5
        [HttpPut("AssignVolunteer/{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AssignVolunteer(int id, [FromQuery] string volunteerId)
        {
            var request = await _context.DonationRequests.FindAsync(id);
            if (request == null) return NotFound();

            request.AssignedVolunteerId = volunteerId;
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // GET: api/Donation/5
        [HttpGet("{id}")]
        public async Task<ActionResult<DonationRequest>> GetRequestById(int id)
        {
            var request = await _context.DonationRequests.FindAsync(id);
            if (request == null) return NotFound();

            return Ok(request);
        }

        [HttpGet]
        [Authorize]
        public IActionResult TestUser()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
                return Unauthorized("UserId not found");

            return Ok(new { UserId = userId });
        }
    }
}
