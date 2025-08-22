using Microsoft.AspNetCore.Identity;

namespace Medshareanddonation.Data
{
    public class ApplicationUser : IdentityUser
    {
        public string name { get; set; }    
    }
}
