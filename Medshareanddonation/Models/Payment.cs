namespace Medshareanddonation.Models
{
    public class Payment
    {
        public int Id { get; set; }
        public int OrderId { get; set; }
       // public Order Order { get; set; }

        public string Gateway { get; set; } // "COD", "Nagad", "bKash"
        public string TransactionId { get; set; } // for online payment
        public decimal Amount { get; set; }
        public DateTime PaidAt { get; set; } = DateTime.UtcNow;
        public string Status { get; set; } = "Pending"; // Pending, Confirmed, Failed
    }
}
