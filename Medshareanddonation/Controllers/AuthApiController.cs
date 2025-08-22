using Medshareanddonation.Data;
using Medshareanddonation.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Medshareanddonation.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthApiController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly string _jwtKey;
        private readonly string _jwtIssuer;
        private readonly string _jwtAudience;
        private readonly int _jwtExpiry;

        public AuthApiController(UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            IConfiguration configuration)
        {
            _userManager = userManager;
            _signInManager = signInManager;
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

            var existingUser = await _userManager.FindByEmailAsync(registermodel.Email);
            if (existingUser != null)
            {
                return BadRequest("Email already in use");
            }

            var user = new ApplicationUser
            {
                UserName = registermodel.Email,
                Email = registermodel.Email,
                name = registermodel.Name
            };

            var result = await _userManager.CreateAsync(user, registermodel.Password);
            if (!result.Succeeded)
            {
                return BadRequest(result.Errors);
            }

            return Ok(new { Message = "User registered successfully" });
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

            var token = GenerateJwtToken(user);
            return Ok(new { Token = token });
        }

        [HttpPost("Logout")]

        public async Task<IActionResult> Logout()
        {

            await Task.CompletedTask;
            return Ok(new { Message = "User logged out successfully" });
        }

        private string GenerateJwtToken(ApplicationUser user)
        {
            var claims = new[]
            {

                new Claim(JwtRegisteredClaimNames.Sub, user.Email),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(JwtRegisteredClaimNames.Sub,user.Id)
               // new Claim(ClaimTypes.NameIdentifier, user.Id)
            };

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
