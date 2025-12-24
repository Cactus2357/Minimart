namespace MinimartApi.Models {
    public class Sale {
        public int SaleId { get; set; }
        
        public int? CustomerId { get; set; }

        public int UserId { get; set; }
        public string Status { get; set; } = string.Empty; // e.g., "COMPLETED", "REFUNDED", "CANCELLED"
        public DateTime SaleDate { get; set; }
        public DateTime RefundDate { get; set; }
        public decimal TotalAmount { get; set; }
        public string PaymentMethod { get; set; } = string.Empty; // e.g., "CASH", "CREDIT_CARD", "MOBILE_PAYMENT"

        public virtual Customer? Customer { get; set; }
        public virtual User User { get; set; }
        public virtual ICollection<SaleItem> Items { get; set; }
    }
}
