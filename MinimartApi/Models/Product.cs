namespace MinimartApi.Models {
    public class Product {
        public int ProductId { get; set; }
        public int CategoryId { get; set; }
        public virtual Category? Category { get; set; }

        public string Name { get; set; } = string.Empty;
        public string SKU { get; set; } = string.Empty;
        public string Unit { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public decimal CostPrice { get; set; }
        public string Barcode { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public bool IsActive { get; set; }

        public virtual Stock? Stock { get; set; }
        public virtual ICollection<SaleItem> SaleItems { get; set; }
        public virtual ICollection<PurchaseOrderItem> PurchaseOrderItems { get; set; }
    }
}
