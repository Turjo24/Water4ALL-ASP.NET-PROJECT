using System.ComponentModel.DataAnnotations;

namespace Medshareanddonation.Models
{
    // Order Model
    public class Order
    {
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; } // User who placed the order

        [Required]
        public DateTime OrderDate { get; set; } = DateTime.Now;

        [Required]
        public decimal TotalAmount { get; set; }

        [Required]
        public string Status { get; set; } = "Pending"; // Pending, Confirmed, Shipped, Delivered, Cancelled

        // Shipping Information
        [Required]
        [StringLength(100)]
        public string ShippingName { get; set; }

        [Required]
        [StringLength(15)]
        [Phone]
        public string ShippingPhone { get; set; }

        [Required]
        [StringLength(500)]
        public string ShippingAddress { get; set; }

        [StringLength(100)]
        public string? City { get; set; }

        [StringLength(10)]
        public string? PostalCode { get; set; }

        // Payment Information
        public string? PaymentMethod { get; set; } // Cash on Delivery, Card, etc.
        public string? PaymentStatus { get; set; } = "Pending"; // Pending, Paid, Failed

        public string? Notes { get; set; }

        // Navigation Properties
        public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
    }
}