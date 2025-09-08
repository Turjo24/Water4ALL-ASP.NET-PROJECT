using Medshareanddonation.Data;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace Medshareanddonation.Services
{
    public class ProfileService : IProfileService
    {
        private readonly ApplicationDbContext _context;

        public ProfileService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<bool> UpdateUserNameAsync(string userId, string newName)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null) return false;

            user.UserName = newName;
            _context.Users.Update(user);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<ApplicationUser> GetUserByEmailAsync(string email)
        {
            return await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
        }
    }
}