using MinimartApi.Enums;
using System.ComponentModel.DataAnnotations;

namespace MinimartApi.Db.Models {
    public class Payment {
        [Key]
        public int PaymentId { get; set; }

        [Required]
        public int OrderId { get; set; }
        public Order Order { get; set; }

        [Required, MaxLength(50)]
        public string Method { get; set; } // e.g., Credit Card, PayPal, etc.

        [MaxLength(255)]
        public string ProviderTransactionId { get; set; }

        [Range(0, double.MaxValue)]
        public decimal Amount { get; set; }

        [Required, MaxLength(50)]
        public string Status { get; set; } // e.g., Pending, Completed, Failed, Refunded

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
