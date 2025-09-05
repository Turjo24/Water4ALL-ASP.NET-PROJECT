using System;
using System.ComponentModel.DataAnnotations;

namespace Medshareanddonation.Models
{
    public class DonationRequest
    {
        internal object User;

        [Key]
        public int Id { get; set; }
       
        public string? UserId { get; set; } // FK to ApplicationUser

        public string Name { get; set; }

      
        public string PhoneNumber { get; set; }

       
        public string LocationAddress { get; set; }

       
        public double WaterLiters { get; set; }

 
        public string Reason { get; set; }

        public string Status { get; set; } = "Pending"; // Pending, Approved, Rejected

        public string AssignedVolunteerId { get; set; } = "NULL";// Optional FK to ApplicationUser

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
