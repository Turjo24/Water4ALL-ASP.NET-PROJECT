public class ApplicationUser : IdentityUser
{
    public string name { get; set; }
    public string Role { get; set; }
}
// Remove the following property from AuthApiController if present:
// public string Role { get; set; }
    