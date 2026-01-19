using MinimartApi.Enums;
using System.ComponentModel.DataAnnotations;

namespace MinimartApi.Db.Models {
    public class Order {
        [Key]
        public int OrderId { get; set; }

        [Required]
        public Guid CustomerId { get; set; }
        public User Customer { get; set; }

        [Required]
        public int AddressId { get; set; }
        public Address Address { get; set; }

        [Required, MaxLength(50)]
        public OrderStatus CurrentStatus { get; set; }

        [Range(0, double.MaxValue)]
        public decimal TotalAmount { get; set; }

        [Required]
        public int PaymentId { get; set; }
        public Payment Payment { get; set; }

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        public ICollection<OrderItem> Items { get; set; }
    }
}
