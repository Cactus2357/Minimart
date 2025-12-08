namespace MinimartApi.Models {
    public class SaleItem {
        public int SaleItemId { get; set; }
        public int SaleId { get; set; }
        public virtual Sale Sale { get; set; }

        public int ProductId { get; set; }
        public virtual Product Product { get; set; }

        public int Quantity { get; set; }
        public decimal Price { get; set; }
    }
}
