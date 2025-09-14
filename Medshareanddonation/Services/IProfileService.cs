namespace Medshareanddonation.Services
{
    public interface IProfileService
    {
        Task<UserProfile> GetUserProfileAsync(string userId);

        Task<bool> UpdateNameAndUserNameAsync(string userId, string newName, string newUserName);
    }

    public class UserProfile
    {
        public string name { get; set; }
        public string UserName { get; set; }

        public string Email { get; set; }

        public string Role { get; set; }
    }
}
