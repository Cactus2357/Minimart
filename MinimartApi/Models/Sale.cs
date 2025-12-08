namespace MinimartApi.Models {
    public class Sale {
        public int SaleId { get; set; }
        
        public int? CustomerId { get; set; }
        public virtual Customer? Customer { get; set; }

        public int UserId { get; set; }
        public virtual User User { get; set; }

        public DateTime SaleDate { get; set; }
        public decimal TotalAmount { get; set; }
        public string PaymentMethod { get; set; } = string.Empty; // e.g., "CASH", "CREDIT_CARD", "MOBILE_PAYMENT"

        public virtual ICollection<SaleItem> Items { get; set; }
    }
}
