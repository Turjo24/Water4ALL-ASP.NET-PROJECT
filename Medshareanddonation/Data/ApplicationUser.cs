using Medshareanddonation.Models;
using Microsoft.AspNetCore.Identity;

namespace Medshareanddonation.Data
{
    public class ApplicationUser : IdentityUser
    {

        public string name { get; set; }    

        public string Role { get; set; } = "User"; // Add Role property

        public ICollection<DonationRequest> DonationRequests { get; set; }

        public ICollection<Order> Orders { get; set; }
        public ICollection<OrderItem> OrderItems{ get; set; } 

    }
}
