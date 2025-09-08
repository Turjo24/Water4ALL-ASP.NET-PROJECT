using Medshareanddonation.Data;
using System.Threading.Tasks;

namespace Medshareanddonation.Services
{
    public interface IProfileService
    {
        Task<bool> UpdateUserNameAsync(string userId, string newName);
        Task<ApplicationUser> GetUserByEmailAsync(string email);
    }
}