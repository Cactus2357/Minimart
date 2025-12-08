namespace MinimartApi.Models {
    public class StockTransaction {
        public int StockTransactionId { get; set; }
        public int ProductId { get; set; }
        public virtual Product? Product { get; set; }

        public string Type { get; set; } = string.Empty; // e.g., "IN", "OUT", "ADJUSTMENT"
        public int Quantity { get; set; }
        public DateTime TransactionDate { get; set; }
        public string Reference { get; set; } = string.Empty; // e.g., purchase, sale, adjustment reference
    }
}
