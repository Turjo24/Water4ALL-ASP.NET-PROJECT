using Medshareanddonation.Data;
using Medshareanddonation.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using ApplicationUser = Medshareanddonation.Data.ApplicationUser;

namespace Medshareanddonation.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthApiController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly string _jwtKey;
        private readonly string _jwtIssuer;
        private readonly string _jwtAudience;
        private readonly int _jwtExpiry;

        public AuthApiController(UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            RoleManager<IdentityRole> roleManager,
            IConfiguration configuration)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
            _jwtKey = configuration["Jwt:Key"] ?? throw new ArgumentNullException("Jwt:Key");
            _jwtIssuer = configuration["Jwt:Issuer"] ?? throw new ArgumentNullException("Jwt:Issuer");
            _jwtAudience = configuration["Jwt:Audience"] ?? throw new ArgumentNullException("Jwt:Audience");
            _jwtExpiry = int.TryParse(configuration["Jwt:ExpiryInMinutes"], out var expiry) ? expiry : 60;
        }

        [HttpPost("Register")]
        public async Task<IActionResult> Register([FromBody] Registermodel registermodel)
        {
            if (registermodel == null ||
                string.IsNullOrEmpty(registermodel.Name) ||
                string.IsNullOrEmpty(registermodel.Email) ||
                string.IsNullOrEmpty(registermodel.Password))
            {
                return BadRequest("Invalid registration details");
            }

            // Validate role
            var validRoles = new[] { "Admin", "Volunteer", "User" };
            if (!validRoles.Contains(registermodel.Role))
            {
                registermodel.Role = "User"; // Default to User if invalid role
            }

            var existingUser = await _userManager.FindByEmailAsync(registermodel.Email);
            if (existingUser != null)
            {
                return BadRequest("Email already in use");
            }

            var user = new ApplicationUser
            {
                UserName = registermodel.Email,
                Email = registermodel.Email,
                name = registermodel.Name,
                Role = registermodel.Role
            };

            var result = await _userManager.CreateAsync(user, registermodel.Password);
            if (!result.Succeeded)
            {
                return BadRequest(result.Errors);
            }

            // Create roles if they don't exist
            await EnsureRolesExist();

            // Assign role to user
            await _userManager.AddToRoleAsync(user, registermodel.Role);

            return Ok(new { Message = "User registered successfully", Role = registermodel.Role });
        }

        [HttpPost("Login")]
        public async Task<IActionResult> Login([FromBody] LoginModel loginModel)
        {
            if (loginModel == null || string.IsNullOrEmpty(loginModel.Email) || string.IsNullOrEmpty(loginModel.Password))
            {
                return BadRequest("Invalid login details");
            }

            var user = await _userManager.FindByEmailAsync(loginModel.Email);
            if (user == null)
            {
                return Unauthorized("Invalid credentials");
            }

            var result = await _signInManager.CheckPasswordSignInAsync(user, loginModel.Password, false);
            if (!result.Succeeded)
            {
                return Unauthorized("Invalid credentials");
            }

            var userRoles = await _userManager.GetRolesAsync(user);
            var token = GenerateJwtToken(user, userRoles);

            return Ok(new
            {
                Token = token,
                Role = user.Role,
                UserId = user.Id,
                Name = user.name
            });
        }

        [HttpPost("Logout")]
        public async Task<IActionResult> Logout()
        {
            await Task.CompletedTask;
            return Ok(new { Message = "User logged out successfully" });
        }

        [HttpPost("CreateAdmin")]
        public async Task<IActionResult> CreateAdmin()
        {
            // Check if admin already exists
            var adminUser = await _userManager.FindByEmailAsync("admin@medshare.com");
            if (adminUser != null)
            {
                return BadRequest("Admin already exists");
            }

            // Create admin user
            var admin = new ApplicationUser
            {
                UserName = "admin@medshare.com",
                Email = "admin@medshare.com",
                name = "System Admin",
                Role = "Admin",
                EmailConfirmed = true
            };

            var result = await _userManager.CreateAsync(admin, "Admin@123");
            if (!result.Succeeded)
            {
                return BadRequest(result.Errors);
            }

            await EnsureRolesExist();
            await _userManager.AddToRoleAsync(admin, "Admin");

            return Ok(new
            {
                Message = "Admin created successfully",
                Email = "admin@medshare.com",
                Password = "Admin@123"
            });
        }

        private async Task EnsureRolesExist()
        {
            var roles = new[] { "Admin", "Volunteer", "User" };

            foreach (var role in roles)
            {
                if (!await _roleManager.RoleExistsAsync(role))
                {
                    await _roleManager.CreateAsync(new IdentityRole(role));
                }
            }
        }

        private string GenerateJwtToken(ApplicationUser user, IList<string> roles)
        {
            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Email),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(ClaimTypes.Name, user.name),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim("Role", user.Role)
            };

            // Add role claims
            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _jwtIssuer,
                audience: _jwtAudience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(_jwtExpiry),
                signingCredentials: creds);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}