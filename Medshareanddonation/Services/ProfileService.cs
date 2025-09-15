using Medshareanddonation.Data;
using Microsoft.EntityFrameworkCore;

namespace Medshareanddonation.Services
{
    public class ProfileService : IProfileService
    {
        private readonly ApplicationDbContext _dbContext;

        public ProfileService(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<UserProfile> GetUserProfileAsync(string userId)
        {
            var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null) return null;

            return new UserProfile
            {
                name = user.name, // lowercase per your DB model
                UserName = user.UserName,
                Email = user.Email,
                Role = user.Role,
            };
        }

        public async Task<bool> UpdateNameAndUserNameAsync(string userId, string newName, string newUserName)
        {
            var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null) return false;

            user.name = newName;
            user.UserName = newUserName;

            _dbContext.Users.Update(user);
            await _dbContext.SaveChangesAsync();

            return true;
        }
    }
}
