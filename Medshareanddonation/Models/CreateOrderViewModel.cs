using System.ComponentModel.DataAnnotations;

namespace Medshareanddonation.Models
{
    public class CreateOrderViewModel
    {
        [Required]
        [StringLength(100)]
        [Display(Name = "Full Name")]
        public string ShippingName { get; set; }

        [Required]
        [StringLength(15)]
        [Phone]
        [Display(Name = "Phone Number")]
        public string ShippingPhone { get; set; }

        [Required]
        [StringLength(500)]
        [Display(Name = "Shipping Address")]
        public string ShippingAddress { get; set; }

        [StringLength(100)]
        public string? City { get; set; }

        [StringLength(10)]
        [Display(Name = "Postal Code")]
        public string? PostalCode { get; set; }

        [Display(Name = "Payment Method")]
        public string PaymentMethod { get; set; } = "Cash on Delivery";

        [Display(Name = "Order Notes")]
        public string? Notes { get; set; }

        public List<CartItem> CartItems { get; set; } = new List<CartItem>();
        public decimal TotalAmount { get; set; }
    }

    public class OrderDetailsViewModel
    {
        public Order Order { get; set; }
        public List<OrderItem> OrderItems { get; set; }
    }

}
