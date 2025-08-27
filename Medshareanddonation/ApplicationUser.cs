using Microsoft.AspNetCore.Identity;

namespace Medshareanddonation.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string name { get; set; }
        public string Role { get; set; } = "User"; // Add Role property
    }
}