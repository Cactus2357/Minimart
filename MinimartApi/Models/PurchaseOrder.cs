namespace MinimartApi.Models {
    public class PurchaseOrder {
        public int PurchaseOrderId { get; set; }
        public int SupplierId { get; set; }
        public virtual Supplier Supplier { get; set; }

        public DateTime OrderDate { get; set; }
        public DateTime? ReceivedDate { get; set; }
        public decimal TotalAmount { get; set; }
        public string Status { get; set; } = string.Empty; // e.g., "PENDING", "RECEIVED", "CANCELLED"
        public virtual ICollection<PurchaseOrderItem> Items { get; set; }
    }
}
